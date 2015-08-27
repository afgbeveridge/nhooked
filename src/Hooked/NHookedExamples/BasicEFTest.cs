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
using ComplexOmnibus.Hooked.Infra;
using ComplexOmnibus.Hooked.BaseEngineImplementations.Engine;
using ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Ancillary;
using ComplexOmnibus.Hooked.BaseImplementations.Core.Sinks;
using ComplexOmnibus.Hooked.BaseImplementations.Core.Stores;
using ComplexOmnibus.Hooked.Mapping;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;
using ComplexOmnibus.Hooked.CommonLibraryImplementations.Logging;
using ComplexOmnibus.Hooked.CommonLibraryImplementations.DI;

namespace Hooked {
    
    public class BasicEFTest : IBasicTest {

        private IEngine ExecutingEngine { get; set; }

        private IComponentFactory Factory { get; set; }

        private Initializer InitializerHelper { get; set; }

        public void Init(Initializer init) {

            MappingInitializer.Execute();

            InitializerHelper = init;

            Factory = new CastleWindsorBackedComponentFactory();

            RegisterServices();

            ExecutingEngine = Factory.Instantiate<IEngine>();

            ExecutingEngine.UniqueId = "2";

            AddTestSubscriptions();

        }

        private void RegisterServices() {

            Factory.Register<IEngine, Engine>();
            Factory.Register<IMessageProcessor, MessageProcessor>();

            Factory.Register<ILogger, NLogLogger>((NLogLogger)new NLogLogger().Configure());
            Factory.Register<IMessageMatcher, ChannelMonickerMessageMatcher>();
            Factory.Register<ISubscriptionStore, PersistentSubscriptionStore>();
            Factory.Register<IFailureHandler, InMemoryFailureHandler>();

            Factory.Register<IHydrationService, DatabaseHydrationService>();
            Factory.Register<IContentParser, JsonContentParser>();
            Factory.Register<IConfigurationSource, DictionaryConfigurationSource>(BuildConfiguration());
            Factory.Register<ITopicStore, PersistentTopicStore>();
            Factory.Register<IWorkPolicy, BasicWorkPolicy>();
            Factory.Register<IAuditService, DatabaseAuditService>();

            InitializerHelper.OnServiceRegistration(Factory);
        }

        public void Start() {
            ExecutingEngine.Start();
        }

        public void Stop() {
            ExecutingEngine.Stop();
            Factory.CleanUp();
        }

        private DictionaryConfigurationSource BuildConfiguration() {
            return 
                InitializerHelper.OnConfiguring(
                    Configuration
                    .Instance
                    .HighThroughputSettings
                    .Into(new DictionaryConfigurationSource())) 
                as DictionaryConfigurationSource;
        }

        private void AddTestSubscriptions() {

            ISubscriptionStore store = Factory.Instantiate<ISubscriptionStore>();

            if (store.Count() == 0) {
                Topic t = new Topic { Name = "Test", UniqueId = Guid.NewGuid().ToString(), Description = "Testing topic only" };
                // No frills subscription
                Subscription subs = new Subscription {
                    Topic = t,
                    ChannelMonicker = "Test",
                    UniqueId = "_TEST_",
                    Sink = new ConsoleMessageSink(),
                    QualityConstraints = QualityAttributes.Default,
                    Name = "Simple",
                    Description = "Lossy subscription"
                };
                Assert.True(store.Add(subs).Success, () => "Could not add subscription");
                // Reliable http, with 10 second backoff
                var quality = QualityAttributes.Default;
                quality.SinkQuality = new SinkQualityAttributes { RequestTimeout = 5000 };
                quality.GuaranteeDelivery = true;
                quality.MaxRetry = 3;
                quality.BackOffPeriod = 10000;
                quality.EndureQuietude = 2000;
                subs = new Subscription {
                    Topic = t,
                    ChannelMonicker = "ReliableRemoteClient",
                    UniqueId = Guid.NewGuid().ToString(),
                    Sink = new HttpMessageSink { Target = new Uri("http://localhost:8080/api/demo"), MimeType = "application/json" },
                    QualityConstraints = quality,
                    Name = "Reliable",
                    Description = "Lossless subscription"
                };
                Assert.True(store.Add(subs).Success, () => "Could not add subscription");
            }

            //var s = store.FindById("_TEST_").Containee;
            //s.QualityConstraints.EndureQuietude = 667;
            //s.QualityConstraints.SinkQuality.RequestTimeout = 1;
            //store.Update(s);
        }
    }
}
