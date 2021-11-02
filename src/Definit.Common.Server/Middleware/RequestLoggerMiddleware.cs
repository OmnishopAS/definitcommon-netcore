using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Definit.Common.Server.Request;

namespace Definit.Common.Server.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            this._next = next;
            _logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    var requestPath = context.Request.Path.Value.ToLower();
                    var r = context.Request;
                    var headersString = String.Join(Environment.NewLine, r.Headers.Select(x => $"{x.Key}:{x.Value}"));
                    var requestString = $"{RequestUtilities.GetRawTargetUrlFromRequest(r)} from {context.Connection.RemoteIpAddress} {Environment.NewLine}{headersString}";
                    _logger.LogDebug(requestString);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error logging data from request");
                }
            }

            return _next(context);
        }
    }
}
