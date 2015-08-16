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
using System.Collections.Concurrent;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Util;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using PRCHOICE = System.Func<ComplexOmnibus.Hooked.Interfaces.Engine.IWorkPolicyConclusion, bool>;
using PREXEC = System.Action<ComplexOmnibus.Hooked.Interfaces.Core.IProcessableUnit, ComplexOmnibus.Hooked.BaseEngineImplementations.Engine.BaseMessageHandler.PolicyResultHandler>;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
	
	public abstract class BaseMessageHandler : IMessageHandler {

        private List<Tuple<PRCHOICE, PREXEC>> PolicyHandlers { get; set; }

        public BaseMessageHandler() : this(null) {
        }

        public BaseMessageHandler(string id) {
            UniqueId = id;
            PolicyHandlers = new List<Tuple<Func<IWorkPolicyConclusion, bool>, Action<IProcessableUnit, PolicyResultHandler>>> {
                Tuple.Create<PRCHOICE, PREXEC>(pc => pc.Completed, (u, h) => h.Completed(u)),
                Tuple.Create<PRCHOICE, PREXEC>(pc => pc.Discard, (u, h) => h.Discard(u)),
                Tuple.Create<PRCHOICE, PREXEC>(pc => pc.Retry, (u, h) => h.Retry(u)),
                Tuple.Create<PRCHOICE, PREXEC>(pc => pc.PassToFailureHandling, (u, h) => h.ActivateFailureHandling(u)), 
                Tuple.Create<PRCHOICE, PREXEC>(pc => pc.Block, (u, h) => h.Block(u)) 
            };
            //Blocked = false;
        }

		public IEnumerable<IFailureHandler> FailureHandlerSet { get; set; }

		public virtual IRequestResult Initialize(IComponentFactory factory) {
            Factory = factory;
            Initializing(factory);
			return Result();
		}

        protected abstract void Initializing(IComponentFactory factory);

        public IRequestResult UnInitialize() {
            return Result();
        }

		public virtual IRequestResult Accept(ISubscription subscription, IMessage message) {
			var bundle = new ProcessableUnit { Subscription = subscription, Message = message };
            Accepting(bundle);
            RealizeIdentifier(subscription);
            BundlePrototype.IsNull(() => BundlePrototype = subscription);
			return Result();
		}

        protected abstract void Accepting(IProcessableUnit bundle);

        protected ISubscription BundlePrototype { get; private set; }

        protected virtual void RealizeIdentifier(ISubscription subs) {
            // We tie ourselves to a channel i.e. specific 'customer' endpoint processing, by default
            UniqueId = subs.ChannelMonicker;
        }

		public virtual IRequestResult CanAccept(ISubscription subscription, IMessage message) {
            bool acceptable = BundlePrototype.IsNull() || subscription.CompatibleWith(BundlePrototype);
			return Result(acceptable);
		}

		public virtual IRequestResult HasWork() {
			// Examine the total set of processable units, pending + failure handler set
			return Result(Any());
		}

        protected abstract bool Any();

		public async virtual Task<IRequestResult> Work() {
			// Examine the total set of processable units, pending + failure handler set.
			// Select the message based on the nature of the subscriptions quality attributes
            // For now, do the simple thing
            IsWorking = true;
            bool ok = true;
            try {
                if (!HasWork().Success) {
                    if (BundlePrototype.QualityConstraints.EndureQuietude.HasValue)
                        await Task.Delay((int) BundlePrototype.QualityConstraints.EndureQuietude.Value);
                }
                else {
                    IRequestResult<IProcessableUnit> result = await ProcessNextUnit();
                    if (result.Containee.IsNotNull()) {
                        var conclusion = Factory.Instantiate<IWorkPolicy>().Analyze(result);
                        var handler = PolicyAnalysisHandler;
                        var action = PolicyHandlers.FirstOrDefault(h => h.Item1(conclusion));
                        action.IsNotNull(() => action.Item2(result.Containee, handler));
                    }
                }
            }
            finally {
                IsWorking = false;
            }
			return Result(ok);
		}

        public virtual bool Viable { 
            get {
                ValidateBlockedStatus();
                return !Cease && !Blocked && (HasWork().Success || IsWorking || BundlePrototype.QualityConstraints.EndureQuietude.HasValue);
            }
        }

        private void ValidateBlockedStatus() {
            EnsureOperationalStatus();
            var status = OperationStatus;
            if (status.Blocked && status.NextReactivationDate.HasValue) {
                status.Blocked = status.NextReactivationDate.Value > DateTime.Now;
                status.Blocked
                    .IfFalse(() => Factory.Instantiate<ILogger>().LogInfo("Unblocking after backoff: " + BundlePrototype.ToString()));
            }
        }

        protected virtual void Unblocking() { 
        }

        protected abstract Task<IRequestResult<IProcessableUnit>> ProcessNextUnit();

        protected virtual PolicyResultHandler PolicyAnalysisHandler {
            get {
                return new PolicyResultHandler {
                    ActivateFailureHandling = u => { 
                        FailureHandlerSet.FirstOrDefault(h => h.Accept(u).Success);
                        Remove(u);
                    },
                    Block = u => this.Blocked = true
                };
            }
        }

        public virtual bool Cease { get; set; }

        public IComponentFactory Factory { private get { return CurrentFactory; } set { CurrentFactory = value; } }

        [NonSerialized]
        private IComponentFactory CurrentFactory;

		public bool IsWorking { get; protected set; }

        public string UniqueId { get; set; }

        private BlockedStatus OperationStatus { get; set; }

        public virtual bool Blocked { 
            get {
                EnsureOperationalStatus();
                return OperationStatus.Blocked;
            } 
            set {
                EnsureOperationalStatus();
                OperationStatus.Blocked = value;
                value
                    .IfTrue(() => { 
                        if (BundlePrototype.QualityConstraints.BackOffPeriod.HasValue)
                            OperationStatus.NextReactivationDate = DateTime.Now.AddMilliseconds(BundlePrototype.QualityConstraints.BackOffPeriod.Value); 
                    })
                    .IfFalse(() => Unblocking());
            } 
        }

        private void EnsureOperationalStatus() {
            if (OperationStatus.IsNull())
                OperationStatus = new BlockedStatus();
        }

		private IRequestResult Result(bool success = true) {
			return RequestResult.Create(success);
		}

        public virtual IRequestResult Hydrate(IHydrationObject obj) {
            if (obj.IsNotNull() && obj.State.IsNotNull()) {
                StateContainer container = Hydrating(obj);
                BundlePrototype = container.BundlePrototype;
                FailureHandlerSet = Factory.InjectSelf(container.FailureHandlers);
            }
            // In case was blocked, ensure available again
            Blocked = false;
            return RequestResult.Create();
        }

        // Record subscription prototype, and any pending messages
        public virtual IRequestResult<IHydrationObject> Dehydrate() {
            StateContainer container = Dehydrating(); 
            container.BundlePrototype = BundlePrototype;
            container.FailureHandlers = FailureHandlerSet.ToArray();
            return RequestResult<IHydrationObject>.Create(new HydrationObject(GetType(), container.Serialize().ToString()));
        }

        protected abstract StateContainer Hydrating(IHydrationObject obj);

        protected abstract StateContainer Dehydrating();

        protected abstract void Remove(IProcessableUnit unit);

        [Serializable]
        protected class StateContainer {
            internal ISubscription BundlePrototype { get; set; }
            internal IFailureHandler[] FailureHandlers { get; set; }
        }

        public class PolicyResultHandler {

            internal PolicyResultHandler() {
                ActivateFailureHandling = Discard = Retry = Completed = u => { };
            }

            internal Action<IProcessableUnit> Completed { get; set; }

            internal Action<IProcessableUnit> ActivateFailureHandling { get; set; }
            
            internal Action<IProcessableUnit> Discard { get; set; }
            
            internal Action<IProcessableUnit> Retry { get; set; }

            internal Action<IProcessableUnit> Block { get; set; }
        }

        protected class BlockedStatus {
            internal bool Blocked { get; set; }
            internal DateTime? NextReactivationDate { get; set; }
        }

	}
}
