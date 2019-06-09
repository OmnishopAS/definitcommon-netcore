using System;

namespace Definit.QData.ChangeSets
{
    /// <summary>
    /// Used by changesetapplier to load/delete/add entities (root entry and compositions)
    /// </summary>
    public interface IChangeSetContext
    {
        void AddNewEntity(object entity);
        void DeleteEntity(object entity);
        object LoadEntity(Type entityType, object[] keyValues);
    }
}