using System;
using System.Linq;
using System.Threading.Tasks;
using Definit.Common.Server.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Definit.Common.Server.Middleware
{
    public class UrlAuthorizeMiddleware 
    {
        private readonly ILogger _logger;
        private readonly Func<HttpContext, RouteValueDictionary, Task<bool>> _authorizeAction;
        private readonly RequestDelegate _next;
        private readonly RouteMatcher _routeMatcher;

        public UrlAuthorizeMiddleware(RequestDelegate next, ILogger<UrlAuthorizeMiddleware> logger, UrlAuthorizeOptions options)
        {
            _logger = logger;
            _authorizeAction = options.AuthorizeAction;
            _next = next;
            _routeMatcher = new RouteMatcher(options.RequestPath);
        }

        public async Task Invoke(HttpContext context)
        {
            if(!_routeMatcher.Match(context.Request.Path, out var routeValues))
            {
                await _next(context);
                return;
            }

            //All users (including unauthenticated users) are allowed to access endpoints (actions/pages/etc.) marked with AllowAnonymousAttribute
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() ?? false;
            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            //All other endpoints/resources (not marked with AllowAnonymousAttribute) requires authentication
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var shouldContinueWithNext = true;
            if(_authorizeAction!=null)
            {
                shouldContinueWithNext = await _authorizeAction(context, routeValues);
            }

            if(shouldContinueWithNext)
            {
                await _next(context);
            }
        }
    }

    public class UrlAuthorizeOptions
    {
        public string RequestPath { get; set; }

        //Action that returns true if middleware pipeline should continue execution, false to end execution
        public Func<HttpContext, RouteValueDictionary, Task<bool>> AuthorizeAction { get; set; }
    }

    public static class UrlAuthorizeMiddlewareExtensions
    {
        public static IApplicationBuilder UseUrlAuthorizeMiddleware(this IApplicationBuilder app, UrlAuthorizeOptions options)
        {
            return app.UseMiddleware<UrlAuthorizeMiddleware>(options);
        }

        public static IApplicationBuilder UseUrlAuthorizeMiddleware(this IApplicationBuilder app, string requestPath, Func<HttpContext, RouteValueDictionary, Task<bool>> authorizeAction)
        {
            return app.UseUrlAuthorizeMiddleware(new UrlAuthorizeOptions()
            {
                RequestPath = requestPath,
                AuthorizeAction = authorizeAction
            });
        }
    }
}
