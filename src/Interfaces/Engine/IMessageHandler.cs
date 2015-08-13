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
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.Interfaces.Engine {
	
	/// <summary>
	/// An opaque message handler; what it does inside is up to it :-)
	/// </summary>
    public interface IMessageHandler : IInitializable, IIdentifiable, IHydratableDependent {
		/// <summary>
		/// Accept a new message. The receiver might already be processing a message;
		/// if so, the receiver should buffer the supplied message in an appropriate manner to that it can process next 
		/// </summary>
		/// <param name="subscription"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		IRequestResult Accept(ISubscription subscription, IMessage message);
        /// <summary>
        /// If the receiver can accept this message and subscription, let it be so
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="message"></param>
        /// <returns>true for success if acceptance is possible</returns>
		IRequestResult CanAccept(ISubscription subscription, IMessage message);
		/// <summary>
		/// Returns a wrapped boolean; if true, then the receiver should be called again
		/// </summary>
		/// <returns></returns>
		IRequestResult HasWork();
		/// <summary>
		/// Ask the receiver to do something
		/// </summary>
		/// <returns></returns>
		Task<IRequestResult> Work();
        /// <summary>
        /// Asks the receiver to cease work
        /// </summary>
        bool Cease { get; set; }
		/// <summary>
		/// Returns true if the receiver is actually working at the time of call
		/// </summary>
		bool IsWorking { get; }
        /// <summary>
        /// The active failure handler set for the receiver
        /// </summary>
		IEnumerable<IFailureHandler> FailureHandlerSet { get; set; }
        /// <summary>
        /// Returns true if the receiver should remain in an active state if possible
        /// </summary>
        bool Viable { get; }
        
	}
}
