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
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources;
using ComplexOmnibus.Hooked.BaseEngineImplementations.Engine;

namespace Hooked {
    
    public class Initializer {

        public Action<IComponentFactory> OnServiceRegistration { get; set; }

        public Func<IConfigurationSource, IConfigurationSource> OnConfiguring { get; set; }

        public string Description { get; set; }

        public static Initializer Http {
            get {
                return new Initializer {
                    OnServiceRegistration = f => { 
                        f.Register<IMessageSource, HttpMessageSource>(); 
                        f.Register<IMessageHandler, InMemoryMessageHandler>(); 
                    },
                    OnConfiguring = c => c.Set<string, HttpMessageSource>(HttpMessageSource.AddressKey, "http://localhost:55555/"),
                    Description = "Http message source, in memory message handling"
                };
            }
        }

        public static Initializer HttpResourceLimited {
            get {
                return new Initializer {
                    OnServiceRegistration = f => { 
                        f.Register<IMessageSource, HttpMessageSource>(); 
                        f.Register<IMessageHandler, ResourceLimitingMessageHandler>();
                        f.Register<IResourceLimitingAssistant, PersistentLimitingAssistant>();
                    },
                    OnConfiguring = c => c.Set<string, HttpMessageSource>(HttpMessageSource.AddressKey, "http://localhost:55555/"),
                    Description = "Http message source, resource limited message handling (persistent)"
                };
            }
        }

        public static Initializer MSMQ {
            get {
                return new Initializer {
                    OnServiceRegistration = f => { 
                        f.Register<IMessageSource, MSMQQueue>(); 
                        f.Register<IMessageHandler, InMemoryMessageHandler>(); 
                    },
                    OnConfiguring = c => c.Set<string, MSMQQueue>(MSMQQueue.AddressKey, @".\private$\nhooked").Set<int, MSMQQueue>(MSMQQueue.TimeoutKey, 500),
                    Description = "MSMQ message source, in memory message handling"
                };
            }
        }

    }
}
