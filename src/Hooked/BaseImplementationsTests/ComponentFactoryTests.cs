using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;

namespace BaseImplementationsTests {

    [TestClass]
    public class ComponentFactoryTests {

        private ComponentFactory Factory { get; set; }

        [TestInitialize]
        public void Initialize() {
            Factory = new ComponentFactory();
            Factory.Register<ISomeComponent, SomeComponent>();
            Factory.Register<IComplexComponent, ConstructedComplexComponent>();
            Factory.Register<IAncillaryComponent, AncillaryComponent>();
        }

        [TestMethod]
        public void Instantiate_Naked() {
            var x = Factory.Instantiate<ISomeComponent>();
            Assert.IsInstanceOfType(x, typeof(SomeComponent));
        }

        [TestMethod]
        public void Instantiate_Via_Constructor() {
            var x = Factory.Instantiate<IComplexComponent>();
            Assert.IsInstanceOfType(x, typeof(ConstructedComplexComponent));
            x.DelegatedWork();
        }

        [TestMethod]
        public void Instantiate_Via_Constructor_With_InjectableProperties() {
            var x = Factory.Instantiate<IComplexComponent>();
            Assert.IsInstanceOfType(x, typeof(ConstructedComplexComponent));
            Assert.IsInstanceOfType(x.OtherComp, typeof(AncillaryComponent));
        }

    }

    public interface ISomeComponent {
        void Work();
    }

    public class SomeComponent : ISomeComponent {
        public void Work() {  }
    }

    public interface IComplexComponent {
        IAncillaryComponent OtherComp { get; }
        void DelegatedWork();
    }

    public interface IAncillaryComponent {
        void Work();
    }

    public class AncillaryComponent : IAncillaryComponent {
        public void Work() { }
    }

    public class ConstructedComplexComponent : IComplexComponent {

        public ConstructedComplexComponent(ISomeComponent comp) {
            Comp = comp;
        }

        public void DelegatedWork() {
            Comp.Work();
        }

        public IAncillaryComponent OtherComp{ get; set; }

        private ISomeComponent Comp { get; set; }

    }

}
