using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.ServiceProcess;

namespace Omnishop.Common.Server
{
    public class ServerWindowsService : WebHostService
    {
        readonly IWebHost _webHost;
        readonly ILogger _logger;

        public ServerWindowsService(IWebHost webHost)
            : base(webHost)
        {
            _webHost = webHost;
            _logger = webHost.Services.GetService<ILogger<ServerWindowsService>>();
            AutoLog = true;
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogInformation("Service OnStarting");
            //Sets timeout for service start to 2 minutes
            base.RequestAdditionalTime(120 * 1000);
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            base.OnStarted();

            _logger.LogInformation("Service OnStarted - ASP.NET Server has started.");
            var feature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();
            if (feature != null)
            {
                foreach (var url in feature.Addresses)
                {
                    _logger.LogInformation("Listening on url: " + url);
                }
            }

        }

        protected override void OnPause()
        {
            _logger.LogInformation("Service OnPause");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            _logger.LogInformation("Service OnContinue");
            base.OnContinue();
        }

        protected override void OnCustomCommand(int command)
        {
            _logger.LogInformation("Service OnCustomCommand " + command);
            base.OnCustomCommand(command);
        }


        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.LogInformation("Service OnPowerEvent " + powerStatus.ToString());
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _logger.LogInformation("Service OnSessionChange, Reason: " + changeDescription.Reason.ToString());
            base.OnSessionChange(changeDescription);
        }

        protected override void OnShutdown()
        {
            _logger.LogInformation("Service OnShutdown");
            base.OnShutdown();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("Service OnStopping");
            base.OnStopping();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("Service OnStopped");
            base.OnStopped();
        }
    }
}
