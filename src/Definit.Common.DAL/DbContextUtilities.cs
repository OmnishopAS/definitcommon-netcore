using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Definit.Common.DAL
{
    public static class DbContextUtilities
    {
        public static T GetNewOrExisting<T>(this DbContext context, Func<T, bool> predicate)
            where T : class
        {            
            var t = context.ChangeTracker.Entries<T>()
                .Select(x => x.Entity)
                .SingleOrDefault(predicate);

            if(t==default(T))
            {
                t = context.Set<T>().SingleOrDefault(predicate);
            }

            return t;               
        }
    }
}
