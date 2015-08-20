using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    public class ReschedulableInMemoryFailureHandler : InMemoryFailureHandler {

        private const int DefaultBackOffPeriod = 1000;

        public override bool HasProcessableCandidates() {
            return ProcessableCandidates().Any();
        }

        public override IRequestResult<IProcessableUnit> Next {
            get {
                var candidates = ProcessableCandidates().ToArray();
                return RequestResult<IProcessableUnit>.Create(candidates.Any() ? candidates.First() : null);
            }
        }

        private IEnumerable<IProcessableUnit> ProcessableCandidates() {
            Func<IProcessableUnit, int> backoffPeriod = u => u.Subscription.QualityConstraints.BackOffPeriod.HasValue ? u.Subscription.QualityConstraints.BackOffPeriod.Value : DefaultBackOffPeriod;
            return Units
                    .Where(u => 
                        u.Message.LastDeliveryAttempt == null || u.Message.LastDeliveryAttempt.Value < DateTime.Now.AddMilliseconds(backoffPeriod(u)))
                    .OrderBy(u => u.Message.LastDeliveryAttempt);
        }

    }
}
