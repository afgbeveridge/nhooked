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
using ComplexOmnibus.Hooked.Infra;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {
    
    public class ComponentFactory : IComponentFactory {

        private Dictionary<Type, TypeContainer> Registry = new Dictionary<Type, TypeContainer>();

        private TypeContainer CheckType<TType>() {
            Type t = typeof(TType);
            Assert.True(Registry.ContainsKey(typeof(TType)), () => "Missing type " + t.Name);
            return Registry[t];
        }

        private TType Build<TType>(Type t) {
            return (TType)Activator.CreateInstance(t);
        }

        private TType InjectSelf<TType>(TType obj) where TType : class {
            // Crude, but allows consumers to using any DI container they like.....
            if (obj is IFactoryDependent) 
                ((IFactoryDependent)obj).Factory = this;
            return obj;
        }

        public bool KnowsOf<TType>() where TType : class {
            return Registry.ContainsKey(typeof(TType));
        }

        public TType Instantiate<TType>() where TType : class {
            TypeContainer t = CheckType<TType>();
            var result = (TType) t.Singleton;
            return InjectSelf(result ?? Build<TType>(t.Types.First()));
        }

        public TType Instantiate<TType, THint>(THint hint) where TType : class {
            TypeContainer t = CheckType<TType>();
            var targetType = t.Types.First();
            var ctor = targetType.GetConstructor(new[] { typeof(THint) });
            Assert.True(Registry.ContainsKey(typeof(TType)), () => "No constructor for type " + targetType.Name + " taking an argument of type " + typeof(THint).Name);
            return InjectSelf((TType) ctor.Invoke(new object[] { hint }));
        }

        public IEnumerable<TType> InstantiateAll<TType>() where TType : class {
            TypeContainer t = CheckType<TType>();
            return t.Types.Select(type => InjectSelf(Build<TType>(type)));
        }

        public TType Instantiate<TType>(Type registeredType) where TType : class {
            return InjectSelf(Build<TType>(registeredType));
        }

        public IComponentFactory Register<TAbstractType, TImplementationType>(TImplementationType singleton = default(TImplementationType)) where TAbstractType : class where TImplementationType : class {
            return this.Fluently(() => { 
                var absType = typeof(TAbstractType);
                var container = Registry.ContainsKey(absType) ? Registry[absType] : new TypeContainer();
                container.Types.Add(typeof(TImplementationType));
                container.Singleton = InjectSelf<TImplementationType>(singleton);
                Assert.True(singleton == null || container.Types.Count == 1, () => "Can't singleton multiples for " + absType.Name);
                Registry[absType] = container; 
            });
        }

        public IEnumerable<TType> InjectSelf<TType>(IEnumerable<TType> targets) where TType : IFactoryDependent {
            if (targets.IsNotNull()) {
                foreach (var dep in targets) {
                    dep.Factory = this;
                }
            }
            return targets;
        }

        private class TypeContainer {
            internal TypeContainer() {
                Types = new List<Type>();
            }

            internal object Singleton { get; set; }
            
            internal List<Type> Types { get; set; }
        }

    }
}
