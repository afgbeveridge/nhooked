using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Infra;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    /// <summary>
    /// Refactor needed; anyway, this type has an injected resource limiting assistant
    /// </summary>
    public class ResourceLimitingMessageHandler : BaseMessageHandler {

        public ResourceLimitingMessageHandler(IWorkPolicy policy, IAuditService auditService, ILogger logger) : base(policy, auditService, logger) {
        }

        public IResourceLimitingAssistant Limiter { get; set; }

        protected override void Initializing() {
            Assert.True(Limiter.IsNotNull(), () => "Cannot instantiate; missing IResourceLimitingAssistant injection");
            Limiter.Initializing();
        }

        protected override void Accepting(IProcessableUnit bundle) {
            Limiter.Accept(bundle);
        }

        protected override bool Any() {
            return Limiter.Any();
        }

        protected override IProcessableUnit NextUnit() {
            return Limiter.NextUnit();
        }

        protected override StateContainer Hydrating(IHydrationObject obj) {
            ResourceLimitingStateContainer container = obj.State.Deserialize<ResourceLimitingStateContainer>();
            Limiter.Hydrate(container.LimiterState);
            return container;
        }

        protected override StateContainer Dehydrating() {
            return new ResourceLimitingStateContainer { LimiterState = Limiter.Dehydrate().Containee };
        }

        protected override void Remove(IProcessableUnit unit, bool delivered) {
            Limiter.Remove(unit, delivered);
            unit.Delivered = delivered;
            Audit(unit);
        }

        protected override PolicyResultHandler PolicyAnalysisHandler {
            get {
                var handler = base.PolicyAnalysisHandler;
                handler.Discard = u => Remove(u, false);
                handler.Completed = u => Remove(u, true);
                // No action necessary for retry, as is by default 'do nothing's
                return handler;
            }
        }

        [Serializable]
        protected class ResourceLimitingStateContainer : StateContainer {
            internal IHydrationObject LimiterState { get; set; }
        }
    }
}
