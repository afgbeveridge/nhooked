﻿#region License
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

        private const int DefaultDelay = 500;
        public const string ProcessorDelayKey = "processorDelay";

        public Engine(IMessageProcessor processor) {
            Processor = processor;
        }

        public virtual IEngine OrderComparator(IComparer<IMessage> comparator) {
            return this;
        }

        public IEngine Start() {
            Logger.LogInfo("Main engine start");
            // Spin off a task that is the processing task
            // In that:
            //  See if any failure handlers have to execute; create 'handlers for them'
            //  Create queue
            //  Process messages
            CancellationToken = new CancellationTokenSource();
            var token = CancellationToken.Token;
            Processor.ContainerId = UniqueId;
            var hydrationResult = Processor.Hydrate;
            Assert.True(hydrationResult.Success, () => "Could not hydrate: " + hydrationResult.Message);
            ProcessorTask = Task.Run(async () => {
                try {
                    while (!token.IsCancellationRequested) {
                        if (!Processor.Next().Success) {
                            int delay = DefaultDelay;
                            Configuration.IsNotNull(() => delay = Configuration.Get<int>(this, ProcessorDelayKey, DefaultDelay));
                            await Task.Delay(delay); 
                        }
                    }
                    Processor.Terminate();
                }
                catch (Exception ex) {
                    Logger.LogWarning("Abrupt processor exit: " + ex.ToString());
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
                    Logger.LogIfNot(Processor.Dehydrate, LogLevel.Error);
                });
            });
        }

        public string UniqueId { get; set; }
    
        // Injected properties

        public ILogger Logger { get; set; }

        public IConfigurationSource Configuration { get; set; }

        private CancellationTokenSource CancellationToken { get; set; }

        private Task ProcessorTask { get; set; }

        private IMessageProcessor Processor { get; set; }
    }

}
