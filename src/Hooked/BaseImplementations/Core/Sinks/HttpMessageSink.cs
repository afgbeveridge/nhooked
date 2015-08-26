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
using System.Net;
using System.Net.Http;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseImplementations.Core.Sinks {
    
    /// <summary>
    /// A configurable Http message sink (currently only POST) that directs an IMessage to an Http Uri target
    /// </summary>
    [Serializable]
    public class HttpMessageSink : IMessageSink, IHydratableDependent {

        private const int DefaultTimeout = 4999;

        public Uri Target { get; set; }

        public string MimeType { get; set; }

        public async Task<IRequestResult> Dispatch(IMessage message, ISinkQualityAttributes attrs) {
            RequestResult result = RequestResult.Create();
            this.GuardedExecutionAsync(async () => {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(attrs.RequestTimeout.HasValue ? attrs.RequestTimeout.Value : DefaultTimeout);
                // This is somewhat of a cheat. The expectation is that the body WILL be a string
                var body = message.Body.IsNull() || message.Body.Ancillary.IsNull() ? String.Empty : message.Body.Ancillary;
                
                var res = await client.PostAsync(Target, new StringContent(body, Encoding.ASCII, MimeType));
                result.Success = res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.StatusCode == HttpStatusCode.NoContent;
            },
            ex => {
                result.Success = false;
                result.Message = ex.ToString();
            });
            return result;
        }


        public IRequestResult Hydrate(IHydrationObject obj) {
            if (obj.IsNotNull() && obj.State.IsNotNull()) {
                var container = obj.State.Deserialize<StateContainer>();
                Target = container.Target;
                MimeType = container.MimeType;
            }
            return RequestResult.Create();
        }

        public IRequestResult<IHydrationObject> Dehydrate() {
            StateContainer container = new StateContainer { Target = Target, MimeType = MimeType };
            return RequestResult<IHydrationObject>.Create(new HydrationObject(GetType(), container.Serialize().ToString()));
        }

        [Serializable]
        protected class StateContainer {
            internal Uri Target { get; set; }
            internal string MimeType { get; set; }
        }
    }
}
