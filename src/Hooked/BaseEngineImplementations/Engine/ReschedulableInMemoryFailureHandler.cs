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
