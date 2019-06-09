using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnishop.Common.Server
{
    public static class RequestUtilities
    {
        public static string GetSiteRootUrl(HttpRequest request)
        {
            var serverRootUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            if (!serverRootUrl.EndsWith("/"))
                serverRootUrl += "/";
            return serverRootUrl;
        }

        public static string GetRawTargetUrlFromRequest(HttpRequest request)
        {
            var httpRequestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            return httpRequestFeature?.RawTarget;
        }
    }
}
