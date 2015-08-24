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

namespace ComplexOmnibus.Hooked.Interfaces.Infra {
	
    /// <summary>
    /// A wrapper interface; implement this to insert your DI container of choice
    /// </summary>
	public interface IComponentFactory {
        /// <summary>
        /// Instantiate an instance of type TType
        /// </summary>
        /// <typeparam name="TType">search type</typeparam>
        /// <returns>an instance of TType</returns>
        TType Instantiate<TType>() where TType : class;
        /// <summary>
        /// Instantiate an instance of type TType, with a constructor parameter
        /// </summary>
        /// <typeparam name="TType">search type</typeparam>
        /// <typeparam name="THint">ctor parameter type</typeparam>
        /// <param name="hint">the ctor parameter</param>
        /// <returns>an instance of TType</returns>
        TType Instantiate<TType, THint>(THint hint) where TType : class;
        /// <summary>
        /// Instantiate all types registered for a search type
        /// </summary>
        /// <typeparam name="TType">search type</typeparam>
        /// <returns>a collection of TType instances</returns>
        IEnumerable<TType> InstantiateAll<TType>() where TType : class;
        /// <summary>
        /// Instantiate all types registered for a search type
        /// </summary>
        /// <typeparam name="TType">search type</typeparam>
        /// <returns>a collection of TType instances</returns>
        IEnumerable<TType> InstantiateAll<TType>(Type registeredType) where TType : class;
        /// <summary>
        /// For some type, instantiate
        /// </summary>
        /// <typeparam name="TType">result type</typeparam>
        /// <param name="registeredType">search type</param>
        /// <returns>n instance of TType</returns>
        TType Instantiate<TType>(Type registeredType) where TType : class;
        /// <summary>
        /// Register a search type, and an implementation of that type
        /// </summary>
        /// <typeparam name="TAbstractType">An interface or abstract type to register</typeparam>
        /// <typeparam name="TImplementationType">A concrete implementation of an interface or abstract type</typeparam>
        /// <param name="singleton">A TAbstractType instance if the type being registered is to be treated as a singleton</param>
        /// <returns>Self</returns>
        IComponentFactory Register<TAbstractType, TImplementationType>(TImplementationType singleton = default(TImplementationType))
            where TAbstractType : class
            where TImplementationType : class, TAbstractType;
        /// <summary>
        /// Does the receiver have a registration for a search type?
        /// </summary>
        /// <typeparam name="TType">search type</typeparam>
        /// <returns>true if self knows of TType</returns>
        bool KnowsOf<TType>() where TType : class;
        /// <summary>
        /// Does the receiver have a registration for a search type?
        /// </summary>
        /// <param name="t">search type</param>
        /// <returns>true if self knows of t</returns>
        bool KnowsOf(Type t);
        /// <summary>
        /// Ask the receiver to clean up any resources it has, ostensibly as a prelude to exit
        /// </summary>
        void CleanUp();
	}

}
