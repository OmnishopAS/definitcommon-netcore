using Definit.QData.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Definit.QData.AspNetCore
{
    /// <summary>
    /// IQDataEntityModel implementation that gets entity metadata from EntityFrameworkCore model
    /// metadata from EFCore model can be overriden by placing attributes on entity class and/or properties
    /// </summary>
    public class QDataEntityModelEFCore : IQDataEntityModel
    {
        readonly IModel _contextModel;

        public QDataEntityModelEFCore(IModel contextModel)
        {
            _contextModel = contextModel;
        }

        public EntityKey[] GetKeysForEntity(Type entityType)
        {
            return _contextModel.FindEntityType(entityType)
                .FindPrimaryKey()
                .Properties
                .Select(x => new EntityKey() {  Name = x.Name, PropertyInfo = x.PropertyInfo })
                .ToArray();                            
        }

        public IEnumerable<Type> GetEntityTypes()
        {
            return _contextModel.GetEntityTypes().Select(x => x.ClrType).ToList();
        }

        public QDataEntityInfo GetEntityInfo(Type entityClrType)
        {
            var retValue = new QDataEntityInfo()
            {
                Properties = new List<EntityPropertyInfo>(),
                CompositeEntities = new List<EntityNavigationPropertyInfo>(),
                NavigationProperties = new List<EntityNavigationPropertyInfo>()
            };
            var compAttribute = entityClrType.GetCustomAttribute<CompositeEntityAttribute>(true);

            var entType = _contextModel.FindEntityType(entityClrType);
            var entKeys = GetKeysForEntity(entityClrType);
            var entProperties = entType.GetProperties();

            foreach (var propInfo in entityClrType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
            {
                var propertyRole = QDataPropertyRoles.None;
                var propAvailAttribute = GetAttributeOrNull<QDataPropertyRoleAttribute>(propInfo);
                if (propAvailAttribute != null)
                {
                    propertyRole = propAvailAttribute.QDataPropertyRole;
                }
                else
                {
                    var scalarProperty = entProperties.FirstOrDefault(x => x.Name == propInfo.Name);
                    if(scalarProperty!=null)
                    {
                        if (entKeys.Any(x => x.Name == scalarProperty.Name))
                        {
                            propertyRole = QDataPropertyRoles.Key;
                        }
                        else
                        {
                            propertyRole = QDataPropertyRoles.ReadWrite;
                        }
                    }
                }

                if(propertyRole!=QDataPropertyRoles.None)
                {
                    var displayAttribute = GetAttributeOrNull<DisplayAttribute>(propInfo);

                    var entPropInfo = new EntityPropertyInfo()
                    {
                        Name = propInfo.Name,
                        PropertyRole = propertyRole,
                        DisplayName = displayAttribute?.Name,
                        Description = displayAttribute?.Description,
                    };
                    retValue.Properties.Add(entPropInfo);
                }
            }


            foreach (var scalarProperty in entType.GetProperties())
            {
                //Default is ReadWrite for all entityframework mapped properties, 
                //unless property is part of primarykey or overridden by attribute

                if(scalarProperty.PropertyInfo!=null)
                {
                    var propertyRole = QDataPropertyRoles.ReadWrite;
                    var propAvailAttribute = GetAttributeOrNull<QDataPropertyRoleAttribute>(scalarProperty.PropertyInfo);
                    if (propAvailAttribute != null)
                    {
                        propertyRole = propAvailAttribute.QDataPropertyRole;
                    }
                    else
                    {
                        if (entKeys.Any(x => x.Name == scalarProperty.Name))
                        {
                            propertyRole = QDataPropertyRoles.Key;
                        }
                    }

                }
                else
                {
                    //Når dette skjer er det som oftest en navProperty vi har glemt å mappe...
                    System.Diagnostics.Debug.WriteLine(scalarProperty.ToString());
                }
            }

            foreach(var navProperty in entType.GetNavigations())
            {
                var entName = navProperty.TargetEntityType.ClrType.Name;
                if (navProperty.IsCollection)
                {
                    entName += "[]";
                }

                var navEntityInfo = new EntityNavigationPropertyInfo()
                {
                    Name = navProperty.Name,
                    EntityType = entName
                };
                if (compAttribute!=null && compAttribute.NavPropertyNames.Contains(navProperty.Name))
                {
                    retValue.CompositeEntities.Add(navEntityInfo);
                }
                else
                {
                    retValue.NavigationProperties.Add(navEntityInfo);
                }
            }
            return retValue;
        }

        private static Tattribute GetAttributeOrNull<Tattribute>(PropertyInfo propertyInfo)
            where Tattribute : Attribute
        {
            return propertyInfo.GetCustomAttributes(typeof(Tattribute), true).FirstOrDefault() as Tattribute;
        }
    }
}
