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
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {

    [Serializable]
    public class InMemoryFailureHandler : IFailureHandler {

        public InMemoryFailureHandler() {
            Units = new HashSet<IProcessableUnit>();
        }

        private HashSet<IProcessableUnit> Units { get; set; }

        public uint Order {
            get;
            set;
        }

        public IRequestResult Accept(IProcessableUnit unit) {
            return this.ExecuteWithResult(() => Units.Add(unit));
        }

        public IRequestResult MarkAsProcessed(IProcessableUnit unit) {
            return this.ExecuteWithResult(() => Units.Remove(unit));
        }

        public bool Any() {
            return Units.Any();
        }

        public bool CanProcess() {
            return true;
        }

        public IRequestResult<IProcessableUnit> Next {
            get {
                return RequestResult<IProcessableUnit>.Create(Any() ? Units.First() : null);
            }
        }

    }
}
