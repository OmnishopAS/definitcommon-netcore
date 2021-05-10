using NLog;
using System;
using System.IO;
using System.Reflection;

namespace Definit.Common.Server.Logging
{
    public class NLogHelper
    {

        public static void ConfigureNLog(string dataFolder, string logFileNameBase)
        {
            //var appEnv = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application;
            var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            //Try to load nlog.config, fail silently and use default config if file does not exist or parsing fails.
            try
            {
                var configFileName = Path.Combine(appPath, "nlog.config");
                if (File.Exists(configFileName))
                {
                    NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configFileName);
                }
            }
            catch (Exception)
            {
            }

            if (NLog.LogManager.Configuration == null)
            {
                NLog.LogManager.Configuration = NLogHelper.CreateDefaultNLogConfiguration(dataFolder, logFileNameBase);
            }
        }

        /// <summary>
        /// Creates NLog Configuration for one plain text file and one log4j .xml file.
        /// The files are created in Logs subfolder of path provided in dataFolder argument.
        /// </summary>
        /// <param name="dataFolder">The folder where Logs subfolder will be created.</param>
        /// <returns></returns>
        private static NLog.Config.LoggingConfiguration CreateDefaultNLogConfiguration(string dataFolder, string logFileNameBase)
        {
            var config = new NLog.Config.LoggingConfiguration()
            {
                DefaultCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture
            };

            var txtTarget = new NLog.Targets.FileTarget("textfile")
            {
                CreateDirs = true,
                FileName = Path.Combine(dataFolder, "Logs/" +  logFileNameBase + "_${date:format=yyyy-MM-dd}.txt"),
                FileNameKind = NLog.Targets.FilePathKind.Absolute,
                Layout = new NLog.Layouts.SimpleLayout("${longdate}|${threadid}|${level:uppercase=true}|${logger}|${message}|${exception}"),
            };

            var layout = new NLog.Layouts.Log4JXmlEventLayout();
            layout.Renderer.IncludeNLogData = false;
            var xmlTarget = new NLog.Targets.FileTarget("xmlfile")
            {
                CreateDirs = true,
                FileName = Path.Combine(dataFolder, "Logs/" + logFileNameBase + "_${date:format=yyyy-MM-dd}.xml"),
                FileNameKind = NLog.Targets.FilePathKind.Absolute,
                Layout=layout               
            };
            
            config.AddTarget(txtTarget);
            config.AddTarget(xmlTarget);

            var defaultrule = CreateNLogRule(LogLevel.Trace, "*", txtTarget, xmlTarget);
            config.LoggingRules.Add(defaultrule);
            return config;
        }

        private static NLog.Config.LoggingRule CreateNLogRule(LogLevel nlogMinLogLevel, string pattern, params NLog.Targets.Target[] targets)
        {
            var rule = new NLog.Config.LoggingRule()
            {
                Final = true,
                LoggerNamePattern = pattern
            };
            foreach (var target in targets)
            {
                rule.Targets.Add(target);
            }

            rule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Fatal);
            return rule;
        }

    }
}
