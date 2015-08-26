using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources;

namespace Hooked {
    
    public class Initializer {

        public Action<IComponentFactory> OnServiceRegistration { get; set; }

        public Func<IConfigurationSource, IConfigurationSource> OnConfiguring { get; set; }

        public static Initializer Http {
            get {
                return new Initializer {
                    OnServiceRegistration = f => f.Register<IMessageSource, HttpMessageSource>(),
                    OnConfiguring = c => c.Set<string, HttpMessageSource>(HttpMessageSource.AddressKey, "http://localhost:55555/")
                };
            }
        }

        public static Initializer MSMQ {
            get {
                return new Initializer {
                    OnServiceRegistration = f => f.Register<IMessageSource, MSMQQueue>(),
                    OnConfiguring = c => c.Set<string, MSMQQueue>(MSMQQueue.AddressKey, @".\private$\nhooked").Set<int, MSMQQueue>(MSMQQueue.TimeoutKey, 500)
                };
            }
        }

    }
}
