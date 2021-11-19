using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Definit.Common.Server
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class BasicAuthenticationActionFilterAttribute : ActionFilterAttribute
    {
        public string BasicRealm { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var authHeader = request.Headers["Authorization"].FirstOrDefault(x=>x.StartsWith("Basic"));
            if (!String.IsNullOrEmpty(authHeader))
            {
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authHeader.Substring(6))).Split(':');
                var user = new { Name = cred[0], Pass = cred[1] };

                if(ValidateCredentials(filterContext, user.Name, user.Pass))
                {
                    return;
                }
            }
            filterContext.Result = new UnauthorizedObjectResult("Access denied. User is not authenticated.");
            var res = filterContext.HttpContext.Response;
            res.StatusCode = 401;
            res.Headers.Add("WWW-Authenticate", String.Format("Basic realm=\"{0}\"", BasicRealm ?? "BasicRealm"));
        }

        protected abstract bool ValidateCredentials(ActionExecutingContext filterContext, string username, string password);
    }
}
