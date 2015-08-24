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
	
	public interface IFailureHandler : IHydratableDependent {
		/// <summary>
		/// The order in the failure chain of the receiver
		/// </summary>
		uint Order { get; set; }
		/// <summary>
		/// When a failure handler accepts a message, this fact has to be recorded so that we can 
		/// activate them either on a periodic basis, and when the engine starts (or just discard them)
		/// </summary>
		/// <param name="message"></param>
		/// <returns>success if true if accepted</returns>
		IRequestResult Accept(IProcessableUnit unit);
		/// <summary>
		/// Has the receiver any failed messages that it can offer up for processing? A receiver may be given messages that it can't process,
		/// for example, it needs to wait for a back off period to expire (or similar)
		/// </summary>
		/// <returns>true if the receiver has any failed messages that are candidates for processing</returns>
		bool HasProcessableCandidates();
		/// <summary>
		/// Returns the next (if any) failed message associated with the receiver. Ordering will be honoured as necessary
		/// </summary>
		IRequestResult<IProcessableUnit> Next { get; }
		
	}

}
