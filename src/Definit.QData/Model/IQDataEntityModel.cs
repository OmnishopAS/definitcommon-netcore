using System;
using System.Collections.Generic;

namespace Definit.QData.Model
{
    public interface IQDataEntityModel
    {
        IEnumerable<Type> GetEntityTypes();
        QDataEntityInfo GetEntityInfo(Type entityType);
        EntityKey[] GetKeysForEntity(Type entityType);
    }

}