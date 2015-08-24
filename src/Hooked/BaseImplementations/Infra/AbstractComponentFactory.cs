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
