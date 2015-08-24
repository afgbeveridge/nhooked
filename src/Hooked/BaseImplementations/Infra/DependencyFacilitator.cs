using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {
    
    /// <summary>
    /// Poor imitation of CW typed factory facility
    /// </summary>
    public static class DependencyFacilitator {

        public static IComponentFactory Container { get; set; }

        public static TType Delegate<TType>(Func<IComponentFactory, TType> f) {
            return f(Container);
        }

        public static void Delegate(Action<IComponentFactory> f) {
            f(Container);
        }

    }
}
