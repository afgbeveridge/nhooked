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
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.BaseEngineImplementations.Engine;
using ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Ancillary;
using ComplexOmnibus.Hooked.BaseImplementations.Core.Sinks;

namespace Hooked {
    
    public static class BasicTest {

        private static IEngine Engine { get; set; }

        public static void Init() {
            IComponentFactory factory = new ComponentFactory();
            Engine = new Engine(factory)
                         .AddFailureHandler<InMemoryFailureHandler>()
                         .LogProvider<ConsoleLogger>()
                         .MessageHandler<InMemoryMessageHandler>()
                         .MessageMatcher<ChannelMonickerMessageMatcher>()
                         .MessageSource<HttpMessageSource>()
                         .SubscriptionStore<InMemorySubscriptionStore>();

            Engine.UniqueId = "1";

            factory.Register<IHydrationService, FileSystemHydrationService>();
            factory.Register<IContentParser, JsonContentParser>();
            factory.Register<IConfigurationSource, DictionaryConfigurationSource>(BuildConfiguration());
            factory.Register<ITopicStore, InMemoryTopicStore>();
            factory.Register<IWorkPolicy, BasicWorkPolicy>();

            AddTestSubscriptions();

        }

        public static void Start() {
            Engine.Start();
        }

        public static void Stop() {
            Engine.Stop();
        }

        private static DictionaryConfigurationSource BuildConfiguration() {
            var cfg = new DictionaryConfigurationSource();
            cfg.Set<string, HttpMessageSource>("address", "http://localhost:55555/"); 
            return cfg;
        }

        private static void AddTestSubscriptions() {
            
            IComponentFactory fac = Engine.Factory;
            Topic t = new Topic { Name = "Test", UniqueId = Guid.NewGuid().ToString(), Description = "Testing topic only" };
            fac.Instantiate<ITopicStore>().Add(t);
            Subscription subs = new Subscription {
                Topic = t,
                ChannelMonicker = "Test",
                UniqueId = Guid.NewGuid().ToString(),
                Sink = new ConsoleMessageSink(),
                QualityConstraints = QualityAttributes.Default
            };
            fac.Instantiate<ISubscriptionStore>().Add(subs);
            var quality = QualityAttributes.Default;
            quality.SinkQuality = new SinkQualityAttributes { RequestTimeout = 5000 };
            quality.EndureQuietude = 1000;
            subs = new Subscription {
                Topic = t,
                ChannelMonicker = "Client",
                UniqueId = Guid.NewGuid().ToString(),
                Sink = new ConsoleMessageSink(),
                QualityConstraints = quality,
            };
            fac.Instantiate<ISubscriptionStore>().Add(subs);
            // Failing http
            subs = new Subscription {
                Topic = t,
                ChannelMonicker = "RemoteClient",
                UniqueId = Guid.NewGuid().ToString(),
                Sink = new HttpMessageSink { Target = new Uri("http://localhost:/9999/HookedIn") },
                QualityConstraints = quality,
            };
            fac.Instantiate<ISubscriptionStore>().Add(subs);
            // Reliable http
            quality = QualityAttributes.Default;
            quality.SinkQuality = new SinkQualityAttributes { RequestTimeout = 5000 };
            quality.GuaranteeDelivery = true;
            quality.MaxRetry = 3;
            subs = new Subscription {
                Topic = t,
                ChannelMonicker = "ReliableRemoteClient",
                UniqueId = Guid.NewGuid().ToString(),
                Sink = new HttpMessageSink { Target = new Uri("http://localhost:/9998/HookedIn") },
                QualityConstraints = quality,
            };
            fac.Instantiate<ISubscriptionStore>().Add(subs);
        }

    }

}
