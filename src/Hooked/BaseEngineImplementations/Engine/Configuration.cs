using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    public class Configuration {

        private ThreadLocal<Settings> TargetSettings = new ThreadLocal<Settings>(() => Settings.Create(20, 10));

        // Do not care that this is not a singleton right now
        public static Configuration Instance {
            get {
                return new Configuration();
            }
        }

        public Configuration HighThroughputSettings  {
            get {
                return this.Fluently(() => TargetSettings.Value = Settings.Create(10, 10));
            }
        }

        public Configuration LowThroughputSettings {
            get {
                return this.Fluently(() => TargetSettings.Value = Settings.Create(250, 500));
            }
        }

        public TType Into<TType>(TType cfg) where TType : IConfigurationSource {
            var settings = TargetSettings.Value;
            cfg.Set<int, Engine>(Engine.ProcessorDelayKey, settings.ProcessorDelay);
            cfg.Set<int, MessageProcessor>(MessageProcessor.DefaultDelayKey, settings.MessageProcessorDelay);
            return cfg;
        }

        public TType SetThroughput<TType>(TType cfg, Settings settings) where TType : IConfigurationSource {
            TargetSettings.Value = settings;
            return Into(cfg);
        }

        public sealed class Settings {

            internal int ProcessorDelay { get; set; }
            
            internal int MessageProcessorDelay { get; set; }
            
            internal static Settings Create(int pdelay, int mdelay) {
                return new Settings { 
                    ProcessorDelay = pdelay,
                    MessageProcessorDelay = mdelay
                };
            }
        }
    }
}
