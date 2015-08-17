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
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {
    
    public class DatabaseHydrationService : BaseHydrationService, IHydrationService {

        public void Store(string containerId, string obj) {
            new ContextHelper().InContext(ctx => {
                ctx.PersistedStates.Add(new PersistentState { ContainerId = containerId, State = obj });
                ctx.SaveChanges();
            });
        }

        public string Restore(string containerId) {
            return new ContextHelper().InContext(ctx => {
                var state = ctx.PersistedStates.FirstOrDefault(s => s.ContainerId == containerId);
                return state.IsNull() ? null : state.State;
            });
        }

        public void Remove(string containerId) {
            new ContextHelper().InContext(ctx => {
                var state = ctx.PersistedStates.FirstOrDefault(s => s.ContainerId == containerId);
                state.IsNotNull(() => {
                    ctx.PersistedStates.Remove(state);
                    ctx.SaveChanges();
                });
            });
        }

    }
}
