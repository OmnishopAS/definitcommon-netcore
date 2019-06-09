using System;
using Microsoft.EntityFrameworkCore;
using Definit.QData.ChangeSets;

namespace Definit.QData.AspNetCore
{
    public class DefaultChangeSetContext : IChangeSetContext
    {
        protected readonly DbContext _dbContext;

        public DefaultChangeSetContext(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual void AddNewEntity(object entity)
        {
            _dbContext.Add(entity);
        }

        public virtual void DeleteEntity(object entity)
        {
            _dbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
        }

        public virtual object LoadEntity(Type entityType, object[] keyValues)
        {
            //Returns from context (memory) if already loaded. Loads from database if not already loaded.
            return _dbContext.Find(entityType, keyValues);
        }
    }
}
