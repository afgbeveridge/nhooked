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
using System.Messaging;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources {
	
	public class MSMQQueue : IMessageSource {

		private MessageQueue Queue { get; set; }

		public MSMQQueue() { 
		}

		public IRequestResult<IMessage> Retrieve {
			get {
				Message msg = Queue.Receive();
				return CreateResult(msg);
			}
		}

		public IRequestResult<IMessage> Publish(IMessage msg) {
			RequestResult<IMessage> result = RequestResult<IMessage>.Create(msg);
			this.GuardedExecution(() => Queue.Send(msg),
				ex => {
					result.Success = false;
					result.Message = ex.ToString();
				}
			);
			return result;
		}

		public IRequestResult<IMessage> Peek {
			get {
				Message msg = Queue.Peek();
				return CreateResult(msg);
			}
		}

		public bool CanPeek {
			get { return true; }
		}

		// TODO: Init self, create queue etc
		public IRequestResult Initialize() {
			// TODO: Create Queue
			Queue.Formatter = new BinaryMessageFormatter();
			return new RequestResult { Success = true };
		}

        public IRequestResult UnInitialize() {
            Queue.Dispose();
            return new RequestResult { Success = true };
        }

		private IRequestResult<IMessage> CreateResult(Message msg) {
			RequestResult<IMessage> result = RequestResult<IMessage>.Create();
			msg.IsNotNull(() => {
				result.Containee = msg.Body.IsNull() ? null : msg.Body.Deserialize<ExtensibleMessage>();
			});
			return result;
		}

        // Hydration is not necessary

        public IRequestResult Hydrate(IHydrationObject memento) {
            return RequestResult.Create();
        }

        public IRequestResult<IHydrationObject> Dehydrate() {
            return null;
        }

	}
}
