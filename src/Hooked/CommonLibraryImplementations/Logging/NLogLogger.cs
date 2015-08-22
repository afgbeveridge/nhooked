﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using NLog;
using NLog.Config;
using NLog.Targets;
using ILOGGER = ComplexOmnibus.Hooked.Interfaces.Infra.ILogger;
using LEVEL = ComplexOmnibus.Hooked.Interfaces.Infra.LogLevel;

namespace ComplexOmnibus.Hooked.CommonLibraryImplementations.Logging {

    /// <summary>
    /// An ILogger implementation that wraps NLog
    /// </summary>
    public class NLogLogger : AbstractLogger {

        private static readonly Dictionary<LEVEL, NLog.LogLevel> Mapping = new Dictionary<LEVEL, NLog.LogLevel> { 
            { LEVEL.Debug, NLog.LogLevel.Debug },
            { LEVEL.Info, NLog.LogLevel.Info },
            { LEVEL.Warning, NLog.LogLevel.Warn },
            { LEVEL.Error, NLog.LogLevel.Error },
            { LEVEL.Exception, NLog.LogLevel.Fatal }
        };

        public override ILOGGER Log(LEVEL level, string message, Exception ex = null) {
            return this.Fluently(() => {
                var type = Mapping[level];
                ex
                    .IsNull()
                    .IfTrue(() => Diagnostics.Log(type, message))
                    .IfFalse(() => Diagnostics.Log(type, ex, message));
                ;
            });
        }

        private const string DefaultLayout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff} ${logger} ${message}${newline}${exception:format=ToString,StackTrace}";
        private const string DiagnosticLoggerName = "Diagnostics";

        public override ILOGGER Configure() {
            return this.Fluently(() => {
                if (Configuration.IsNull()) {
                    Configuration = new LoggingConfiguration();
                    CreateFileTarget(DiagnosticLoggerName, "${basedir}/diagnostics.txt", DefaultLayout);
                    CreateEventLogTarget();
                    LogManager.Configuration = Configuration;
                    Diagnostics.Info("Default NLog log configuration established");
                }
            });
        }

        protected LoggingConfiguration Configuration { get; set; }

        protected virtual Logger Diagnostics {
            get {
                return LogManager.GetLogger(DiagnosticLoggerName);
            }
        }

        private void CreateFileTarget(string name, string fileName, string layout) {

            FileTarget fileTarget = new FileTarget {
                FileName = fileName,
                Layout = layout,
                ArchiveAboveSize = 10000000L
            };

            Configuration.AddTarget("file", fileTarget);

            LoggingRule rule = new LoggingRule(name, NLog.LogLevel.Debug, fileTarget);
            Configuration.LoggingRules.Add(rule);
        }

        private void CreateEventLogTarget() {
            EventLogTarget target = new EventLogTarget {
                Source = "NHooked",
                Log = "Application",
                MachineName = ".",
                Layout = DefaultLayout
            };
            Configuration.AddTarget("eventLog", target);
            Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Warn, target));
        }
    }
}
