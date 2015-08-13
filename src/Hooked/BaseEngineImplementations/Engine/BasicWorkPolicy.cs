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
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    public class BasicWorkPolicy : IWorkPolicy {

        public IComponentFactory Factory { private get; set; }

        public IWorkPolicyConclusion Analyze(IRequestResult<IProcessableUnit> result) {
            var conclusion = new WorkPolicyConclusion();
            conclusion.Completed = result.Success;
            if (!conclusion.Completed)
                ExamineFurther(conclusion, result);
            return conclusion;
        }

        private void ExamineFurther(WorkPolicyConclusion conclusion, IRequestResult<IProcessableUnit> result) {
            var unit = result.Containee;
            var quality = unit.Subscription.QualityConstraints;
            if (quality.IsNotNull()) {
                var msg = result.Containee.Message;
                // If makes sense, allow retry
                conclusion.Retry = quality.GuaranteeDelivery && (msg.Retries < quality.MaxRetry);
                msg.Retries++;
                // If need delivery, but cannot retry, go to blocked status
                if (!conclusion.Retry && quality.GuaranteeDelivery) {
                    conclusion.Block = true;
                    Factory.Instantiate<ILogger>().LogInfo("Blocking subscription: " + unit.ToString());
                }
                conclusion.PassToFailureHandling = !conclusion.Retry && !conclusion.Block;
            }
        }
    }
}
