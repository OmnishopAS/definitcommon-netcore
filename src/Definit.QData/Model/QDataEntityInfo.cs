using System.Collections.Generic;

namespace Definit.QData.Model
{
    //public enum QDataValueAutoGenerationTypes
    //{
    //    None = 0,
    //    Always = 10,
    //    Optional = 30
    //}

    public class QDataEntityInfo
    {
        public IList<EntityPropertyInfo> Properties { get; set; }
        public IList<EntityNavigationPropertyInfo> CompositeEntities { get; set; }
        public IList<EntityNavigationPropertyInfo> NavigationProperties { get; set; }
    }

}