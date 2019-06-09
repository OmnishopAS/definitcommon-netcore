using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData.Edm;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Definit.Common.Server.OData
{
    public class EnableODataQueryAttribute : EnableQueryAttribute
    {

        public EnableODataQueryAttribute()
        {
            base.MaxNodeCount = int.MaxValue;
            base.EnsureStableOrdering = false;
        }

        /// <summary>
        /// Default value if querystring does not include $top argument.
        /// </summary>
        public int DefaultTop { get; set; }

        /// <summary>
        /// Multiple expand entries must be separated by ','
        /// </summary>
        public string AllowedExpands { get; set; }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }

        public override object ApplyQuery(object entity, ODataQueryOptions queryOptions)
        {
            var retValue = base.ApplyQuery(entity, queryOptions);
            return retValue;
        }

        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            queryable = ApplyExpand(queryable, queryOptions.RawValues.Expand);
            queryable = ApplyOrderBy(queryable, queryOptions.RawValues.OrderBy);
            var retValue = base.ApplyQuery(queryable, queryOptions);

            ExecuteODataQuery(retValue);
            return retValue;
        }

        private static void ExecuteODataQuery(IQueryable retValue)
        {
            //Forces execution of query.
            //This is done to ensure any exceptions are thrown here instead of after serialization has started.
            var listToDiscard = new List<object>();
            foreach (var res in retValue)
            {
                listToDiscard.Add(res);
            }
        }

        private static IQueryable ApplyExpand(IQueryable queryable, string rawExpand)
        {
            if (!string.IsNullOrWhiteSpace(rawExpand))
            {
                var expandParameters = rawExpand.Split(',')
                                      .Select(x => x.Replace('/', '.').Trim());

                var queryType = queryable.GetType().GetGenericArguments()[0];
                var methodType = typeof(EnableODataQueryAttribute).GetMethod("ApplyExpandGeneric", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var applyExpandGenericMethod = methodType.MakeGenericMethod(queryType);

                queryable = (IQueryable)applyExpandGenericMethod.Invoke(null, new object[] { queryable, expandParameters });
            }

            return queryable;
        }

        private static IQueryable<TEntity> ApplyExpandGeneric<TEntity>(IQueryable<TEntity> query, IEnumerable<string> expandParameters)
            where TEntity : class
        {
            foreach(var expand in expandParameters)
            {
                query = query.Include(expand);
            }
            return query;
        }

        private static IQueryable ApplyOrderBy(IQueryable queryable, string rawOrderBy)
        {
            if (!string.IsNullOrWhiteSpace(rawOrderBy))
            {
                var orderByParameters = rawOrderBy.Split(',');


                var queryType = queryable.GetType().GetGenericArguments()[0];
                var methodType = typeof(EnableODataQueryAttribute).GetMethod("ApplyOrderByGeneric", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var applyOrderByGenericMethod = methodType.MakeGenericMethod(queryType);

                queryable = (IQueryable)applyOrderByGenericMethod.Invoke(null, new object[] { queryable, orderByParameters });
            }

            return queryable;
        }

        private static IQueryable<TEntity> ApplyOrderByGeneric<TEntity>(IQueryable<TEntity> query, IEnumerable<string> orderByParameters)
            where TEntity : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            var firstPart = true;
            foreach(var part in orderByParameters)
            {
                string[] words = part.Split(' ');
                string field = words[0];
              
                var orderByMethodName = firstPart ?
                    (words.Length == 2 && words[1] == "desc" ? "OrderByDescending" : "OrderBy") :
                    (words.Length == 2 && words[1] == "desc" ? "ThenByDescending" : "ThenBy");

                var orderbyPropInfo = typeof(TEntity).GetProperty(field);
                var propertyAccess = Expression.MakeMemberAccess(parameter, orderbyPropInfo);
                var orderbyLambda = Expression.Lambda(propertyAccess, parameter);

                if (orderbyLambda != null)
                {
                    MethodCallExpression resultExpression = Expression.Call(typeof(Queryable), orderByMethodName, new Type[] { typeof(TEntity), orderbyPropInfo.PropertyType }, query.Expression, Expression.Quote(orderbyLambda));
                    query = query.Provider.CreateQuery<TEntity>(resultExpression);
                }

                firstPart = false;
            }

            return query;
        }


        public override IEdmModel GetModel(Type elementClrType, HttpRequest request, ActionDescriptor actionDescriptor)
        {
            var retValue = base.GetModel(elementClrType, request, actionDescriptor);
            return retValue;
        }

        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            //Hacks. All this could be done properly by implementing own logic instead of inheriting from EnableQueryAttribute

            //OData will throw if top > maxTop. We rather like to reduce top to MaxTop.
            if (queryOptions.Top!=null && queryOptions.Top.Value > this.MaxTop)
            {                                
                var rawValuesTopProperty = queryOptions.RawValues.GetType().GetProperty("Top", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                rawValuesTopProperty.SetValue(queryOptions.RawValues, this.MaxTop.ToString());

                var topValueRawProperty = queryOptions.Top.GetType().GetProperty("RawValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                topValueRawProperty.SetValue(queryOptions.Top, this.MaxTop.ToString());

                var topValueField = queryOptions.Top.GetType().GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                topValueField.SetValue(queryOptions.Top, this.MaxTop);
            }

            //Sets SelectExpand to null (private setter)
            //ODatas way of handling SelectExpand messes with our return values. 
            //We use our own implementation of Expand and does not support Select for now.
            var selectExpandProperty = queryOptions.GetType().GetProperty("SelectExpand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            selectExpandProperty.SetValue(queryOptions, null);

            //Sets OrderBy to null (private setter)
            //ODatas throws when ordering on BKey properties since they are not recognized as primitive properties.
            //An alternative approach could be to configure model so BKey properties are recognized as primitive properties. (if that is possible)
            var orderByProperty = queryOptions.GetType().GetProperty("OrderBy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            orderByProperty.SetValue(queryOptions, null);


            base.ValidateQuery(request, queryOptions);
        }


        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
