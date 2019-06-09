using Microsoft.Extensions.Logging;
using System;

namespace Definit.Common.Server.Logging
{
    public class AppDomainLogger
    {
        private readonly ILogger _logger;


        public AppDomainLogger(AppDomain appDomain, ILogger logger)
        {
            _logger = logger;
            appDomain.DomainUnload += CurrentDomain_DomainUnload;
            appDomain.ProcessExit += CurrentDomain_ProcessExit;
            appDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogWarning(e.ExceptionObject as Exception, "AppDomain unhandledexception. IsTerminating: " + e.IsTerminating);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _logger.LogDebug("AppDomain ProcessExit");
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _logger.LogDebug("AppDomain DomainUnload");
        }

    }
}
