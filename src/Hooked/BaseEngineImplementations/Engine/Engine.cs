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
using System.Threading;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Infra;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
	
	/// <summary>
	/// The abstract registration/add etc methods are designed to be overridden to use whatever container makes sense
	/// </summary>
	public class Engine : IEngine {

        public Engine(IComponentFactory factory) {
            Factory = factory;
        }

        public IEngine LogProvider<TLogger>() where TLogger : class, ILogger, new() {
            return this.Fluently(() => Factory.Register<ILogger, TLogger>(new TLogger()));
        }

        public IEngine MessageMatcher<TMatcher>() where TMatcher : class, IMessageMatcher {
            return this.Fluently(() => Factory.Register<IMessageMatcher, TMatcher>());
        }

        public IEngine SubscriptionStore<TStore>() where TStore : class, ISubscriptionStore {
            return this.Fluently(() => Factory.Register<ISubscriptionStore, TStore>());
        }

        public IEngine AddFailureHandler<THandler>() where THandler : class, IFailureHandler {
            return this.Fluently(() => Factory.Register<IFailureHandler, THandler>());
        }

        public IEngine MessageSource<THandler>() where THandler : class, IMessageSource {
            return this.Fluently(() => Factory.Register<IMessageSource, THandler>());
        }

        public IEngine MessageHandler<THandler>() where THandler : class, IMessageHandler {
            return this.Fluently(() => Factory.Register<IMessageHandler, THandler>());
        }

        public IComponentFactory Factory { get; set; }

        public virtual IEngine OrderComparator(IComparer<IMessage> comparator) {
            return this;
        }

		public IEnumerable<IFailureHandler> CreateFailureHandlerSet {
			get {
                return Factory.InstantiateAll<IFailureHandler>().OrderBy(h => h.Order);
			} 
		}

		private ILogger Logger { 
			get {
				return Factory.Instantiate<ILogger>();
			} 
		}

		public IEngine Start() {
			// Spin off a task that is the processing task
			// In that:
			//  See if any failure handlers have to execute; create 'handlers for them'
			//  Create queue
			//  Process messages
			CancellationToken = new CancellationTokenSource();
			CancellationToken token = CancellationToken.Token;
			Processor = new MessageProcessor(this);
            Processor.ContainerId = UniqueId;
            var hydrationResult = Processor.Hydrate;
            Assert.True(hydrationResult.Success, () => "Could not hydrate: " + hydrationResult.Message);
			ProcessorTask = Task.Run(async () => {
                try {
                    while (!token.IsCancellationRequested) {
                        if (!Processor.Next().Success) {
                            await Task.Delay(1000); // TODO: Make configuration
                        }
                    }
                    Processor.Terminate();
                }
                catch (Exception ex) {
                    Factory.Instantiate<ILogger>().LogWarning("Abrupt processor exit: " + ex.ToString());
                    // To try and counter this unfortunate occurrence, we attempt an orderly Stop
                    Stop();
                }
			}, 
            token);
			return this;
		}

		public IEngine Stop() {
			return this.Fluently(() => { 
				// Wait for any active tasks, perform appropriate actions to ensure no loss
				this.GuardedExecution(() => {
                    this.GuardedExecutionAsync(async () => {
						CancellationToken.Cancel();
						await ProcessorTask;
					});
					// IFailureHandlerRegister register = Factory.Instantiate<IFailureHandlerRegister>();
					// register.IsNotNull(() => Logger.LogIfNot(register.Dehydrate, LogLevel.Error));
                    Logger.LogIfNot(Processor.Dehydrate, LogLevel.Error);
				});
			});
		}
        
        public string UniqueId { get; set; }

		private CancellationTokenSource CancellationToken { get; set; }

		private Task ProcessorTask { get; set; }

        private MessageProcessor Processor { get; set; }
	}

}
