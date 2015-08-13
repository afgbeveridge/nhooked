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
    
    // TODO: Currently only supports Post
    [Serializable]
    public class HttpMessageSink : IMessageSink {

        private const int DefaultTimeout = 4999;

        public Uri Target { get; set; }

        public async Task<IRequestResult> Dispatch(IMessage message, ISinkQualityAttributes attrs) {
            RequestResult result = RequestResult.Create();
            this.GuardedExecutionAsync(async () => {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(attrs.RequestTimeout.HasValue ? attrs.RequestTimeout.Value : DefaultTimeout);
                // This is somewhat of a cheat. The expectation is that the body WILL be a string
                var body = message.Body.IsNull() || message.Body.Ancillary.IsNull() ? String.Empty : message.Body.Ancillary;
                var res = await client.PostAsync(Target, new ByteArrayContent(ASCIIEncoding.Default.GetBytes(body.ToString())));
                result.Success = res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted;
            },
            ex => {
                result.Success = false;
                result.Message = ex.ToString();
            });
            return result;
        }
    }
}
