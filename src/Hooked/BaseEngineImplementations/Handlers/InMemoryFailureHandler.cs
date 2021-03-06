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
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {

    [Serializable]
    public class InMemoryFailureHandler : BaseHydratableDependent, IFailureHandler {

        public InMemoryFailureHandler() {
            Units = new List<IProcessableUnit>();
        }

        protected List<IProcessableUnit> Units { get; private set; }

        public ILogger Logger { get; set; }

        public uint Order { get; set; }

        public IRequestResult Accept(IProcessableUnit unit) {
            Logger.LogInfo("Accepting a failed unit: " + unit.ToString());
            return this.ExecuteWithResult(() => Units.Add(unit));
        }

        public virtual bool HasProcessableCandidates() {
            return Units.Any();
        }

        public virtual IRequestResult<IProcessableUnit> Next {
            get {
                return RequestResult<IProcessableUnit>.Create(Units.Any() ? Units.First() : null);
            }
        }

        public override IRequestResult Hydrate(IHydrationObject memento) {
            if (memento.IsNotNull() && memento.State.IsNotNull())
                Units = memento.State.Deserialize<List<IProcessableUnit>>();
            return RequestResult.Create();
        }

        public override IRequestResult<IHydrationObject> Dehydrate() {
            return RequestResult<IHydrationObject>.Create(new HydrationObject { ConcreteType = GetType(), State = Units.Serialize().ToString(), ServiceInterface = typeof(IFailureHandler) });
        }

    }
}
