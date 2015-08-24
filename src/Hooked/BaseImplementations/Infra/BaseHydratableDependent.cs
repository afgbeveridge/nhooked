using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {
    
    public class BaseHydratableDependent : IHydratableDependent {

        public virtual IRequestResult Hydrate(IHydrationObject memento) {
            return RequestResult.Create();
        }

        public virtual IRequestResult<IHydrationObject> Dehydrate() {
            return RequestResult<IHydrationObject>.Create(new HydrationObject { ConcreteType = GetType() });
        }

    }
}
