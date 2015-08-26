using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace SimulatedRemoteClient {
    
    public class StartUp {

        public void Configuration(IAppBuilder app) {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.EnableSystemDiagnosticsTracing();
            app.UseWebApi(config);
        }
    }
}
