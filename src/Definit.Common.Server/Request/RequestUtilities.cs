using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Definit.Common.Server.Request
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
