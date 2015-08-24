using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Infra;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {
    
    public abstract class AbstractComponentFactory : IComponentFactory {

        protected AbstractComponentFactory() {
            DependencyFacilitator.Container = this;
        }

        public bool KnowsOf<TType>() where TType : class {
            return KnowsOf(typeof(TType));
        }

        public abstract bool KnowsOf(Type t);

        public abstract TType Instantiate<TType>() where TType : class;

        public abstract TType Instantiate<TType, THint>(THint hint) where TType : class;

        public abstract IEnumerable<TType> InstantiateAll<TType>() where TType : class;

        public abstract IEnumerable<TType> InstantiateAll<TType>(Type registeredType) where TType : class;

        public abstract TType Instantiate<TType>(Type registeredType) where TType : class;

        public abstract IComponentFactory Register<TAbstractType, TImplementationType>(TImplementationType singleton = default(TImplementationType))
            where TAbstractType : class
            where TImplementationType : class, TAbstractType;

        public virtual void CleanUp() {
        }

    }
}
