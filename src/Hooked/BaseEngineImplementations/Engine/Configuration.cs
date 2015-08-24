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
