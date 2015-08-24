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
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Ancillary;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;
using Newtonsoft.Json;

namespace ComplexOmnibus.Hooked.BaseImplementations.Ancillary {

    /// <summary>
    /// Content parser that translates a canonical Json object into an IMessage
    /// </summary>
    public class JsonContentParser : IContentParser {

        private const string DefaultResponse = "{{ \"accepted\": {0} }}";

        public JsonContentParser()
            : this((b, ex) => string.Format(DefaultResponse, b.ToString().ToLowerInvariant())) {
        }

        public ILogger Logger { get; set; }

        public JsonContentParser(Func<bool, Exception, string> responseFormatter) {
            ResponseFormatter = responseFormatter;
        }

        public IRequestResult<IMessage> Interpret(string content) {
            RequestResult<IMessage> result = RequestResult<IMessage>.Create();
            this.GuardedExecution(() => {
                var obj = JsonConvert.DeserializeObject<dynamic>(content);
                result.Containee = new ExtensibleMessage {
                    TopicName = obj.TopicName,
                    ChannelMonicker = obj.ChannelMonicker,
                    Sequence = obj.Sequence,
                    Body = new ObjectContainer {
                        Ancillary = obj.Body.ToString()
                    }
                };
                result.Message = ResponseFormatter(true, null);
            },
            ex => {
                result.Success = false;
                result.Message = ResponseFormatter(false, ex);
                Logger.LogWarning("Rejecting message: " + content, ex);
            });
            return result;
        }

        private Func<bool, Exception, string> ResponseFormatter { get; set; }

    }
}
