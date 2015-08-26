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
using System.Messaging;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.Interfaces.Ancillary;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources {
	
	public class MSMQQueue : BaseMessageSource {

        public const string AddressKey = "queueAddress";
        public const string TimeoutKey = "queueTimeout";

        public MSMQQueue(IContentParser parser, IConfigurationSource cfg, ILogger logger)
            : base(parser, cfg, logger) {
        }

		public override IRequestResult<IMessage> Retrieve {
			get {
                Message msg = null;
                this.GuardedExecution(() => msg = Queue.Receive(TimeSpan.FromMilliseconds(Configuration.Get<int>(this, TimeoutKey))));
				return CreateResult(msg);
			}
		}

        public override IRequestResult<IMessage> Peek {
			get {
				Message msg = Queue.Peek();
				return CreateResult(msg);
			}
		}

        public override bool CanPeek {
			get { return true; }
		}

		// TODO: Init self, create queue etc
        public override IRequestResult Initialize() {
			// TODO: Create Queue
            var address = Configuration.Get<string>(this, AddressKey);
            Logger.LogInfo("Create queue: " + address);
            Queue = new MessageQueue(address);
			Queue.Formatter = new BinaryMessageFormatter();
			return new RequestResult { Success = true };
		}

        public override IRequestResult UnInitialize() {
            Queue.IsNotNull(() => Queue.Dispose());
            return new RequestResult { Success = true };
        }

        private MessageQueue Queue { get; set; }

		private IRequestResult<IMessage> CreateResult(Message msg) {
            RequestResult<IMessage> result = RequestResult<IMessage>.Create(success: false);
			(msg.IsNotNull() && msg.Body.IsNotNull())
                .IfTrue(() => {
                    var parseResult = Parser.Interpret(msg.Body.ToString());
                    result.Containee = parseResult.Containee;
                    result.Success = parseResult.Success;
            });
			return result;
		}

	}
}
