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
using System.Collections.Concurrent;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Util;
using ComplexOmnibus.Hooked.BaseImplementations.Core;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    public class InMemoryMessageHandler : BaseMessageHandler {

        private ConcurrentQueue<IProcessableUnit> Pending { get; set; }

        public InMemoryMessageHandler() : this(null) {
        }

        public InMemoryMessageHandler(string id) : base(id) {
        }

        protected override void Initializing(IComponentFactory factory) {
            Pending = new ConcurrentQueue<IProcessableUnit>();
        }

        protected override void Accepting(SubscriptionBundle bundle) { 
            Pending.Enqueue(bundle);
        }

        protected override bool Any() {
            return Pending.Any();
        }

        protected async override Task<IRequestResult<IProcessableUnit>> ProcessNextUnit() {
            bool ok = false;
            IProcessableUnit unit;
            if (Pending.TryPeek(out unit)) 
                ok = (await unit.Subscription.Sink.Dispatch(unit.Message, unit.Subscription.QualityConstraints)).Success;
            return RequestResult<IProcessableUnit>.Create(unit, ok);
        }

        protected override PolicyResultHandler PolicyAnalysisHandler {
            get {
                var handler = base.PolicyAnalysisHandler;
                IProcessableUnit unit;
                handler.Discard = handler.Completed = u => Pending.TryDequeue(out unit);
                // No action necessary for retry, as is by default 'do nothing's
                return handler;
            }
        }

        protected override StateContainer Hydrating(IHydrationObject obj) {
            InMemoryStateContainer container = obj.State.Deserialize<InMemoryStateContainer>();
            Pending = container.Pending;
            return container;
        }

        protected override StateContainer Dehydrating() {
            return new InMemoryStateContainer { Pending = Pending };
        }

        [Serializable]
        protected class InMemoryStateContainer : StateContainer {
            internal ConcurrentQueue<IProcessableUnit> Pending { get; set; }
        }

    }
}
