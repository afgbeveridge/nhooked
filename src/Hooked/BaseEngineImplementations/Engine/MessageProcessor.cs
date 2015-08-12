#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Ancillary;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {

    public class MessageProcessor : IHydratable {

        private const int DefaultDelay = 1000;
		private List<HandlerBundle> ActiveHandlers = new List<HandlerBundle>();

		public MessageProcessor(IEngine engine) {
			Factory = engine.Factory;
            HostingEngine = engine;
            TaskDelay = Factory.Instantiate<IConfigurationSource>().Get<int>(this, "taskDelay", DefaultDelay);
		}

		public IRequestResult Next() {
			IRequestResult<IMessage> result = LiveMessageSource.Retrieve;
			if (result.Success) { // we have a message
				IMessage message = result.Containee;
				// See if any active tasks can accept it; or create a new one if we can
				ISubscription subs = FindApplicableSubscription(message);
				subs
					.IsNull()
					.IfTrue(() => Factory.Instantiate<ILogger>().LogWarning("No subscription for message: " + message.ToString()))
					.IfFalse(() => AssignMessageToHandler(subs, message));
			}
			return new RequestResult { Success = result.Success };
		}

		internal IRequestResult Terminate() {
            LiveMessageSource.UnInitialize();
			ActiveHandlers.ForEach(h => {
				this.GuardedExecution(() => { 
					// Ask handlers to terminate
					h.Handler.Cease = true;
				});
			});
			Task.WaitAll(ActiveHandlers.Select(h => h.HostingTask).ToArray());
			return RequestResult.Create();
		}

		private ISubscription FindApplicableSubscription(IMessage message) {
			return Factory.Instantiate<IMessageMatcher>().MatchSubscription(Factory.Instantiate<ISubscriptionStore>(), message);
		}

		private void AssignMessageToHandler(ISubscription subs, IMessage message) {
			HandlerBundle bundle = ActiveHandlers.FirstOrDefault(b => b.Handler.CanAccept(subs, message).Success);
			bool stateAcceptable = bundle.IsNull() || bundle.HostingTask.Status == TaskStatus.Running;
            if (bundle.IsNotNull() && !stateAcceptable)
                ActiveHandlers.Remove(bundle);
			stateAcceptable.IfFalse(() => {
				bundle.IsNotNull(() => {
					ActiveHandlers.Remove(bundle);
					bundle.HostingTask.Wait(); 
				});
				bundle = null;
			});
			bundle
				.IsNull()
				.IfTrue(() => CreateNewHandler(subs, message))
				.IfFalse(() => bundle.Handler.Accept(subs, message));
		}

		private void CreateNewHandler(ISubscription subs, IMessage message) {
			IMessageHandler handler = Factory.Instantiate<IMessageHandler>();
            handler.FailureHandlerSet = HostingEngine.CreateFailureHandlerSet;
			ActiveHandlers.Add(CreateBundle(handler, subs, message));
		}

        private HandlerBundle CreateBundle(IMessageHandler handler, ISubscription subs = null, IMessage message = null) {
            Factory.Instantiate<ILogger>().LogInfo("Activating handler, topic = " + (subs.IsNull() ? "Not known" : subs.Topic.Name) + ", channel = " + (subs.IsNull() ? "Not known" : subs.ChannelMonicker));
            handler.Initialize(Factory);
            HandlerBundle bundle = new HandlerBundle {
                Handler = handler,
                HostingTask = Task.Factory.StartNew(async h => {
                    IMessageHandler processor = h as IMessageHandler;
                    // Can be null if being hydrated
                    if (subs.IsNotNull())
                        processor.Accept(subs, message);
                    while (processor.Active) {
                        if (processor.IsWorking)
                            await Task.Delay(TaskDelay);
                        else
                            await processor.Work();
                    }
                },
                handler,
                TaskCreationOptions.LongRunning)
            };
            return bundle;
        }

		private IComponentFactory Factory { get; set; }

        private IEngine HostingEngine { get; set; }

		private IMessageSource LiveMessageSource { get; set; }

        private int TaskDelay { get; set; }

        [Serializable]
		private class HandlerBundle {
			internal IMessageHandler Handler { get; set; }
			internal Task HostingTask { get; set; }
		}

        public IRequestResult Hydrate {
            get {
                // Hydrate and populate
                var svc = Factory.Instantiate<IHydrationService>();
                var obj = svc.Restore(ContainerId);
                (obj.IsNotNull())
                    .IfTrue(() => {
                        HydrationContainer container = obj.Deserialize<HydrationContainer>();
                        LiveMessageSource = svc.Restore<IMessageSource>(container.MessageSource);
                        ActiveHandlers.AddRange(container.Handlers.Select(def => CreateBundle(svc.Restore<IMessageHandler>(def))));
                    })
                    .IfFalse(() => LiveMessageSource = Factory.Instantiate<IMessageSource>());
			    LiveMessageSource.Initialize(Factory);
                svc.Remove(ContainerId);
                return RequestResult.Create();
            }
        }

        public IRequestResult Dehydrate {
            get {
                // Check each handler to see if its memento is null; if so, ignore
                // Also, dehydrate the IMessageSource
                HydrationContainer container = new HydrationContainer { 
                    MessageSource = LiveMessageSource.Dehydrate().Containee,
                    Handlers = ActiveHandlers.Select(h => h.Handler.Dehydrate().Containee).ToArray()
                };
                Factory.Instantiate<IHydrationService>().Store(ContainerId, container.Serialize().ToString());
                return RequestResult.Create();
            }
        }

        public string ContainerId { get; set; }

        [Serializable]
        private class HydrationContainer {
            internal IHydrationObject MessageSource { get; set; }
            internal IHydrationObject[] Handlers { get; set; }
        }

    }

}
