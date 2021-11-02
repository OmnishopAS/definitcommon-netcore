using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Definit.Common.Server.Request
{
    public class RouteMatcher
    {
        private TemplateMatcher _templateMatcher;

        public RouteMatcher(string routeTemplate)
        {
            var template = TemplateParser.Parse(routeTemplate);
            _templateMatcher = new TemplateMatcher(template, GetDefaults(template));
        }

        public bool Match( PathString requestPath)
        {
            var routeValues = new RouteValueDictionary();
            var isMatch = _templateMatcher.TryMatch(requestPath, routeValues);
            return isMatch;
        }

        public static bool Match(string routeTemplate, PathString requestPath)
        {
            return new RouteMatcher(routeTemplate).Match(requestPath);
        }

        // This method extracts the default argument values from the template.
        private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}
