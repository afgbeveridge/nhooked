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
using System.Reflection;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Infra;

namespace ComplexOmnibus.Hooked.BaseImplementations.Infra {

    public class ComponentFactory : AbstractComponentFactory {

        private Dictionary<Type, TypeContainer> Registry = new Dictionary<Type, TypeContainer>();

        private TypeContainer CheckType<TType>() {
            return GetSingleType(typeof(TType));
        }

        private TypeContainer GetSingleType(Type t) {
            Assert.True(Registry.ContainsKey(t), () => "Missing type " + t.Name);
            return Registry[t];
        }

        private TType Build<TType>(TypeContainer t) where TType : class {
            TType obj = null;
            // Constructor inject first
            if (t.Singleton != null)
                obj = (TType) t.Singleton;
            else {
                obj = ConstructObject<TType>(t.Types.First());
                InjectProperties(obj);
            }
            return obj;
        }

        private TType ConstructObject<TType>(Type t) {
            var ctor = t.GetConstructors()
                        .Where(c => c.GetParameters().All(p => KnowsOf(p.ParameterType)))
                        .OrderByDescending(c => c.GetParameters().Length)
                        .FirstOrDefault();
            Assert.True(ctor.IsNotNull(), () => "No usable constructor for type " + t.Name);
            var args = ctor.GetParameters().Select(p => Build<object>(GetSingleType(p.ParameterType))).ToArray();
            return (TType)ctor.Invoke(args);
        }

        private void InjectProperties(object obj) {
            obj
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !p.GetIndexParameters().Any() && p.CanWrite && KnowsOf(p.PropertyType))
                .ToList()
                .ForEach(p => {
                    var val = Build<object>(GetSingleType(p.PropertyType));
                    p.SetValue(obj, val);
                });
        }

        public override bool KnowsOf(Type t) {
            return Registry.ContainsKey(t);
        }

        public override TType Instantiate<TType>() {
            return Instantiate<TType>(typeof(TType));
        }


        public override TType Instantiate<TType>(Type registeredType) {
            TypeContainer t = GetSingleType(registeredType);
            var result = (TType)t.Singleton;
            return result ?? Build<TType>(t);
        }

        public override TType Instantiate<TType, THint>(THint hint) {
            TypeContainer t = CheckType<TType>();
            var targetType = t.Types.First();
            var ctor = targetType.GetConstructor(new[] { typeof(THint) });
            Assert.True(Registry.ContainsKey(typeof(TType)), () => "No constructor for type " + targetType.Name + " taking an argument of type " + typeof(THint).Name);
            return (TType) ctor.Invoke(new object[] { hint });
        }

        public override IEnumerable<TType> InstantiateAll<TType>() {
            TypeContainer t = CheckType<TType>();
            return t.Types.Select(type => Build<TType>(TypeContainer.Mono(type)));
        }

        public override IEnumerable<TType> InstantiateAll<TType>(Type registeredType) {
            TypeContainer t = GetSingleType(registeredType);
            return t.Types.Select(type => Build<TType>(TypeContainer.Mono(type)));
        }

        public override IComponentFactory Register<TAbstractType, TImplementationType>(TImplementationType singleton = default(TImplementationType))  {
            return this.Fluently(() => { 
                var absType = typeof(TAbstractType);
                var container = Registry.ContainsKey(absType) ? Registry[absType] : new TypeContainer();
                container.Types.Add(typeof(TImplementationType));
                container.Singleton = singleton;
                Assert.True(singleton == null || container.Types.Count == 1, () => "Can't singleton multiples for " + absType.Name);
                Registry[absType] = container; 
            });
        }

        private class TypeContainer {
            internal TypeContainer() {
                Types = new List<Type>();
            }

            internal object Singleton { get; set; }
            
            internal List<Type> Types { get; set; }

            internal static TypeContainer Mono(Type t) {
                var result = new TypeContainer();
                result.Types.Add(t);
                return result;
            }
        }

    }
}
