using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Definit.Common
{
    public static class PropertiesUtilities
    {
        public static propType GetPropertyValue<propType>(this object instance, string propertyName, bool ignoreCase = false)
        {
            var pos = propertyName.IndexOf('.');
            if (pos >= 0)
            {
                var subObject = instance.GetPropertyValue<object>(propertyName.Substring(0, pos), ignoreCase);
                if (subObject != null)
                {
                    return subObject.GetPropertyValue<propType>(propertyName.Substring(pos + 1), ignoreCase);
                }
            }
            else
            {
                var bf = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
                if (ignoreCase)
                    bf = BindingFlags.IgnoreCase | bf;
                var property = instance.GetType().GetProperty(propertyName, bf);
                if (property != null)
                {
                    return (propType)property.GetValue(instance);
                }
            }
            return default(propType);
        }

        public static object GetPropertyValue(this object instance, string propertyName, bool ignoreCase = false)
        {
            return GetPropertyValue<object>(instance, propertyName, ignoreCase);
        }

        public static void SetPropertyValue<propType>(this object instance, string propertyName, propType value, bool ignoreCase = false)
        {
            var pos = propertyName.IndexOf('.');
            if (pos >= 0)
            {
                var subObject = instance.GetPropertyValue<object>(propertyName.Substring(0, pos), ignoreCase);
                if (subObject != null)
                {
                    subObject.SetPropertyValue<propType>(propertyName.Substring(pos + 1), value, ignoreCase);
                }
            }
            else
            {
                var bf = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
                if (ignoreCase)
                    bf = BindingFlags.IgnoreCase | bf;
                var property = instance.GetType().GetProperty(propertyName, bf);
                if (property != null && property.CanWrite)
                {
                    var valueToSet = ConvertType(value, property.PropertyType);
                    property.SetValue(instance, valueToSet, null);
                }
            }
        }

        private static object ConvertType(object value, Type destType)
        {
            if (value == null)
            {
                return CreateDefaultInstance(destType);
            }

            if (value.GetType()==typeof(string) && string.IsNullOrEmpty(value.ToString()) && destType != typeof(string))
            {
                return CreateDefaultInstance(destType);
            }

            var sourceType = value.GetType();
            if (sourceType == destType || destType.IsAssignableFrom(sourceType))
            {
                return value;
            }

            if (destType.IsEnum)
            {
                return Enum.Parse(destType, value.ToString());
            }

            var checkedType = destType;
            if (destType.IsGenericType && (destType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                checkedType = destType.GenericTypeArguments[0];
            return Convert.ChangeType(value, checkedType);
        }

        private static object CreateDefaultInstance(Type destType)
        {
            if (destType.IsValueType)
                return Activator.CreateInstance(destType);            //Creates default value (usually 0) for destType
            else
                return null;
        }
    }
}
