using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Infra;

namespace ComplexOmnibus.Hooked.CommonLibraryImplementations.DI {

    public class CastleWindsorBackedComponentFactory : AbstractComponentFactory {

        public CastleWindsorBackedComponentFactory()
            : this(null) {
        }

        public CastleWindsorBackedComponentFactory(IWindsorContainer container) {
            Container = container ?? new WindsorContainer();
        }

        public override bool KnowsOf(Type t) {
            return Container.Kernel.GetAssignableHandlers(t).Any();
        }

        public override TType Instantiate<TType>() {
            return InjectSelf(Container.Resolve<TType>());
        }

        public override TType Instantiate<TType, THint>(THint hint) {
            return InjectSelf(Container.Resolve<TType>(new { hint = hint }));
        }

        public override IEnumerable<TType> InstantiateAll<TType>() {
            return Container.ResolveAll<TType>().Select(t => InjectSelf(t));
        }

        public override TType Instantiate<TType>(Type registeredType) {
            return InjectSelf((TType) Container.Resolve(registeredType));
        }

        public override IComponentFactory Register<TAbstractType, TImplementationType>(TImplementationType singleton = default(TImplementationType)) {
            return this.Fluently(() => {
                var instanceSupplied = singleton != default(TImplementationType);
                instanceSupplied
                    // We do not expect any type where an instance is not supplied to be anything other than transient
                    .IfFalse(() => Container.Register(Component.For<TAbstractType>().ImplementedBy<TImplementationType>().LifeStyle.Transient))
                    .IfTrue(() => Container.Register(Component.For<TAbstractType>().Instance(singleton)));
            });
        }

        public override void CleanUp() {
            Container.Dispose();
        }

        private IWindsorContainer Container { get; set; }

    }
}
