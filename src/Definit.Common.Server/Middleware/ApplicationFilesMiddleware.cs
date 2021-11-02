using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using System.IO;
using System.Text;

namespace Definit.Common.Server.Middleware
{
    /// <summary>
    /// Middleware that intercepts requests for resources in given path (ApplicationFilesOptions.RequestPath)
    /// If requested file does not exist, request path is rewritten to default resource (Normally index.html of SPA application)
    /// </summary>
    public class ApplicationFilesMiddleware
    {
        protected readonly ApplicationFilesOptions _options;
        protected readonly PathString _matchUrl;
        private readonly PathString _parentPath;
        private readonly string _requiredRole;
        private readonly RequestDelegate _next;
        private readonly IFileProvider _fileProvider;
        protected readonly string _applicationFile;

        /// <summary>
        /// Creates a new instance of the ApplicationFilesMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="hostingEnv">The <see cref="IHostingEnvironment"/> used by this middleware.</param>
        /// <param name="options">The configuration options for this middleware.</param>
        public ApplicationFilesMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, IOptions<ApplicationFilesOptions> options)
        {
            if (hostingEnv == null)
            {
                throw new ArgumentNullException(nameof(hostingEnv));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            var optionsValue = options.Value;
            _applicationFile = optionsValue.ApplicationFile;
            _fileProvider = optionsValue.FileProvider ?? Helpers.ResolveFileProvider(hostingEnv);
            _matchUrl = optionsValue.RequestPath;
            _parentPath = optionsValue.StaticFilesRootPath;
            _requiredRole = optionsValue.RequiredRole;

            if (string.IsNullOrEmpty(_applicationFile))
            {
                _applicationFile = _matchUrl;
            }
        }

        public virtual Task Invoke(HttpContext context)
        {
            PathString subpath;
            if (Helpers.IsGetOrHeadMethod(context.Request.Method) && Helpers.TryMatchPath(context, _matchUrl, forDirectory: false, subpath: out subpath))
            {
                if (!string.IsNullOrEmpty(_requiredRole))
                {
                    if (!context.User.IsInRole(_requiredRole))
                    {
                        context.Response.Redirect($"/page/login?returnUrl={context.Request.Path}&userType={_requiredRole}", false);
                        return Task.CompletedTask;
                    }
                }

                return ProcessRequest(context);
            }
            return _next(context);
        }

        private async Task ProcessRequest(HttpContext context)
        {
            var filePath = System.IO.Path.GetRelativePath(_parentPath, context.Request.Path);
            var fileInfo = _fileProvider.GetFileInfo(filePath);
            if (!fileInfo.Exists)
            {
                context.Request.Path = new PathString(_applicationFile);
            }

            if (context.Request.Path.Value == _applicationFile)
            {
                context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";

                Stream originalBody = context.Response.Body;
                context.Response.Body = new MemoryStream();
                context.Request.EnableBuffering();

                await _next(context);

                string originalContent;
                using (StreamReader stream = new StreamReader(context.Response.Body))
                {
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    originalContent = await stream.ReadToEndAsync();
                }

                string responseContentString = ProcessApplicationFile(context, originalContent);

                var responseContentBytes = Encoding.UTF8.GetBytes(responseContentString);
                using (var memStreamModified = new MemoryStream(responseContentBytes))
                {
                    context.Response.Body.Dispose();
                    context.Response.ContentLength = null;
                    await memStreamModified.CopyToAsync(originalBody);
                    context.Response.Body = originalBody;
                    context.Response.ContentLength = memStreamModified.Length;
                }

                return;
            }

            await _next(context);
        }

        private string ProcessApplicationFile(HttpContext context, string originalContent)
        {
            if (string.IsNullOrEmpty(originalContent))
            {
                return string.Empty;
            }
            string href = GetNewBaseHRef(context);

            if(href==null)
            {
                return originalContent;
            }

            if (!href.EndsWith("/"))
                href += "/";

            var content = UpdateHBaseHRef(originalContent, href);
            return content;
        }

        protected virtual string GetNewBaseHRef(HttpContext context)
        {
            var href = _matchUrl.ToString();
            return href;
        }

        private static string UpdateHBaseHRef(string originalContent, string href)
        {
            var baseStartIndex = originalContent.IndexOf("<base");
            if (baseStartIndex == -1)
            {
                return originalContent;
            }
            var baseEndIndex = originalContent.IndexOf(">", baseStartIndex);
            if (baseEndIndex == -1)
            {
                return originalContent;
            }

            var sbResponse = new StringBuilder();
            sbResponse.Append(originalContent.Substring(0, baseStartIndex - 1));
            sbResponse.Append(@"<base href=""" + href + @""">");
            sbResponse.Append(originalContent.Substring(baseEndIndex + 1));
            return sbResponse.ToString();
        }

        private static class Helpers
        {
            public static IFileProvider ResolveFileProvider(IWebHostEnvironment hostingEnv)
            {
                if (hostingEnv.WebRootFileProvider == null)
                {
                    throw new InvalidOperationException("Missing FileProvider.");
                }
                return hostingEnv.WebRootFileProvider;
            }

            public static bool IsGetOrHeadMethod(string method)
            {
                return HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
            }

            public static bool PathEndsInSlash(PathString path)
            {
                return path.Value.EndsWith("/", StringComparison.Ordinal);
            }

            public static bool TryMatchPath(HttpContext context, PathString matchUrl, bool forDirectory, out PathString subpath)
            {
                var path = context.Request.Path;

                if (forDirectory && !PathEndsInSlash(path))
                {
                    path += new PathString("/");
                }

                if (path.StartsWithSegments(matchUrl, out subpath))
                {
                    return true;
                }
                return false;
            }

        }
    }

    /// <summary>
    /// Options for selecting application default file
    /// </summary>
    public class ApplicationFilesOptions : SharedOptionsBase
    {
        public ApplicationFilesOptions()
            : this(new SharedOptions())
        {
        }

        public ApplicationFilesOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {

        }

        /// <summary>
        /// File name to select by default, when intercepting request for file that does not exist
        /// </summary>
        public string ApplicationFile { get; set; }
        public PathString StaticFilesRootPath { get; set; }
        public string RequiredRole { get; set; }
    }

    public class PosApplicationFilesMiddleware : ApplicationFilesMiddleware
    {
        public PosApplicationFilesMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, IOptions<ApplicationFilesOptions> options) 
            : base(next, hostingEnv, options)
        {
        }

        protected override string GetNewBaseHRef(HttpContext context)
        {
            var clientId = (short)context.Request.RouteValues["clientId"];
            return $"/pos/client/{clientId}" + _matchUrl;
        }
    }
}