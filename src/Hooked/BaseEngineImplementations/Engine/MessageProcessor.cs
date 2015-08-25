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

    public class MessageProcessor : IMessageProcessor {

        private const int DefaultDelay = 1000;
		private List<HandlerBundle> ActiveHandlers = new List<HandlerBundle>();
        public const string DefaultDelayKey = "taskDelay";

		public MessageProcessor(IMessageSource msgSource, IMessageMatcher matcher, ISubscriptionStore store, IConfigurationSource cfg) {
            LiveMessageSource = msgSource;
            MessageMatcher = matcher;
            SubscriptionStore = store;
            TaskDelay = cfg.Get<int>(this, DefaultDelayKey, DefaultDelay);
		}

		public IRequestResult Next() {
			IRequestResult<IMessage> result = LiveMessageSource.Retrieve;
			if (result.Success) { // we have a message
				IMessage message = result.Containee;
				// See if any active tasks can accept it; or create a new one if we can
				ISubscription subs = FindApplicableSubscription(message);
				subs
					.IsNull()
					.IfTrue(() => Logger.LogWarning("No subscription for message: " + message.ToString()))
					.IfFalse(() => AssignMessageToHandler(subs, message));
			}
			return new RequestResult { Success = result.Success };
		}

		public IRequestResult Terminate() {
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
            return MessageMatcher.MatchSubscription(SubscriptionStore, message);
		}

		private void AssignMessageToHandler(ISubscription subs, IMessage message) {
			HandlerBundle bundle = ActiveHandlers.FirstOrDefault(b => b.Handler.CanAccept(subs, message).Success);
			bool stateAcceptable = bundle.IsNull() || bundle.HostingTask.Status == TaskStatus.WaitingForActivation, reanimated = false;
            // There is a handler that can accept the current subscription, but its task has ceased
            // Reuse it to ensure no messages are lost
            if (bundle.IsNotNull() && !stateAcceptable) {
                ActiveHandlers.Remove(bundle);
                bundle.HostingTask.Wait();
                // If the handler is blocked, we still allow this flow to proceed, as this places a message in the handler and the task immediately exits - but the message is preserved
                bundle = CreateBundle(bundle.Handler, subs, message);
                ActiveHandlers.Add(bundle);
                reanimated = true;
                Logger.LogInfo("Reanimated handler for (" + subs.Topic.Name + "," + subs.ChannelMonicker + ")");
            }
            bundle
                .IsNull()
                .IfTrue(() => CreateNewHandler(subs, message))
                .IfFalse(() => { if (!reanimated) bundle.Handler.Accept(subs, message); });
		}

		private void CreateNewHandler(ISubscription subs, IMessage message) {
            IMessageHandler handler = DependencyFacilitator.Delegate(f => f.Instantiate<IMessageHandler>());
			ActiveHandlers.Add(CreateBundle(handler, subs, message));
		}

        private HandlerBundle CreateBundle(IMessageHandler handler, ISubscription subs = null, IMessage message = null) {
            Logger.LogInfo("Activating handler, topic = " + (subs.IsNull() ? "Not known" : subs.Topic.Name) + ", channel = " + (subs.IsNull() ? "Not known" : subs.ChannelMonicker));
            handler.Initialize();
            HandlerBundle bundle = new HandlerBundle {
                Handler = handler,
                HostingTask = Task.Run(async () => {
                    IMessageHandler processor = handler as IMessageHandler;
                    // Can be null if being hydrated
                    if (subs.IsNotNull())
                        processor.Accept(subs, message);
                    while (processor.Viable) {
                        if (processor.IsWorking)
                            await Task.Delay(TaskDelay);
                        else
                            await processor.Work();
                    }
                })
            };
            return bundle;
        }

		private IMessageSource LiveMessageSource { get; set; }

        private IMessageMatcher MessageMatcher { get; set; }

        private ISubscriptionStore SubscriptionStore { get; set; }

        private int TaskDelay { get; set; }

        public ILogger Logger { get; set; }

        public IHydrationService HydrationService { get; set; }

        [Serializable]
		private class HandlerBundle {
			internal IMessageHandler Handler { get; set; }
			internal Task HostingTask { get; set; }
		}

        public IRequestResult Hydrate {
            get {
                // Hydrate and populate
                var obj = HydrationService.Restore(ContainerId);
                (obj.IsNotNull())
                    .IfTrue(() => {
                        HydrationContainer container = obj.Deserialize<HydrationContainer>();
                        LiveMessageSource = HydrationService.Restore<IMessageSource>(container.MessageSource);
                        ActiveHandlers.AddRange(container.Handlers.Select(def => CreateBundle(HydrationService.Restore<IMessageHandler>(def))));
                        MessageMatcher = container.MessageMatcher.Deserialize<IMessageMatcher>();
                    });
			    LiveMessageSource.Initialize();
                HydrationService.Remove(ContainerId);
                return RequestResult.Create();
            }
        }

        public IRequestResult Dehydrate {
            get {
                // Check each handler to see if its memento is null; if so, ignore
                // Also, dehydrate the IMessageSource
                HydrationContainer container = new HydrationContainer { 
                    MessageSource = LiveMessageSource.Dehydrate().Containee,
                    MessageMatcher = MessageMatcher.Serialize(),
                    Handlers = ActiveHandlers.Select(h => h.Handler.Dehydrate().Containee).ToArray()
                };
                HydrationService.Store(ContainerId, container.Serialize().ToString());
                return RequestResult.Create();
            }
        }

        public string ContainerId { get; set; }

        [Serializable]
        private class HydrationContainer {
            internal IHydrationObject MessageSource { get; set; }
            internal object MessageMatcher { get; set; }
            internal IHydrationObject[] Handlers { get; set; }
        }

    }

}
