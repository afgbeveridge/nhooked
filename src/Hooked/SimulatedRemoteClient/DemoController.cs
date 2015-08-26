using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Diagnostics.Tracing;
using Newtonsoft.Json.Linq;

namespace SimulatedRemoteClient {

    public class DemoController : ApiController {

        public void Post([FromBody]JObject value) {
            ITraceWriter traceWriter = Configuration.Services.GetTraceWriter();
            traceWriter.Trace(Request, "Inbound simulation", TraceLevel.Info, "{0}", "Log only.");
        }

    }
}
