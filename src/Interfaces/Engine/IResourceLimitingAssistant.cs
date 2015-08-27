using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.Interfaces.Engine {
    
    public interface IResourceLimitingAssistant : IHydratableDependent {
        void Initializing();
        IRequestResult Accept(IProcessableUnit bundle);
        bool Any();
        IProcessableUnit NextUnit();
        void Remove(IProcessableUnit unit, bool delivered);
    }

}
