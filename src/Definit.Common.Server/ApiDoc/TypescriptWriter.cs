using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Definit.QData.Model;

namespace Definit.Common.Server.ApiDoc
{
    public class TypescriptWriter
    {
        private readonly Dictionary<Type, string> _customTypes = new Dictionary<Type, string>();
        private readonly List<Type> _dtoTypes = new List<Type>();
        private readonly List<Type> _enumTypes = new List<Type>();
        private readonly IQDataEntityModel _entityModel;

        public TypescriptWriter(IQDataEntityModel entityModel)
        {
            _entityModel = entityModel;
        }

        public void RegisterDto<Tdto>()
        {
            _dtoTypes.Add(typeof(Tdto));
        }

        public void RegisterEnum<T>()
        {
            _enumTypes.Add(typeof(T));
        }

        public void RegisterCustomType<T>(string typescriptNamspace)
        {
            _customTypes.Add(typeof(T), typescriptNamspace);
        }

        public void WriteEntities(StringBuilder sb)
        {
            var namespaces = GetNameSpacesForCustomProperties(_entityModel.GetEntityTypes());
            namespaces.Add("Enums");
            foreach (var ns in namespaces)
            {
                sb.AppendLine("import { " + ns + " } from './" + ns.ToLower() + "';");
            }
            sb.AppendLine();
            sb.AppendLine("declare namespace Entities {");
            foreach (var entityType in _entityModel.GetEntityTypes())
            {
                sb.AppendLine("\tinterface " + entityType.Name + " {");
                var qdataEntityInfo = _entityModel.GetEntityInfo(entityType);
                foreach(var propInfo in entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
                {
                    var qdataPropertyInfo = qdataEntityInfo.Properties.FirstOrDefault(x => x.Name == propInfo.Name);
                    if (qdataPropertyInfo == null)
                        continue;
                    sb.AppendLine("\t\t" + propInfo.Name + ": " + TranslatePropertyType(propInfo.PropertyType) + ";");
                }

                foreach (var navProp in qdataEntityInfo.CompositeEntities)
                {
                    sb.AppendLine("\t\t" + navProp.Name + ": Entities." + navProp.EntityType + ";");
                }

                foreach (var navProp in qdataEntityInfo.NavigationProperties)
                {
                    sb.AppendLine("\t\t" + navProp.Name + ": Entities." + navProp.EntityType + ";");
                }



                sb.AppendLine("\t}");
            }
            sb.AppendLine("}");
        }

        public void WriteEnums(StringBuilder sb)
        {
            sb.AppendLine("export namespace Enums {");
            foreach (var en in GetEnums())
            {
                WriteEnum(sb, en);
            }
            sb.AppendLine("}");
        }

        public void WriteDataTransferObjects(StringBuilder sb)
        {
            var namespaces = GetNameSpacesForCustomProperties(_dtoTypes);
            namespaces.Add("Enums");
            namespaces.Add("Entities");
            foreach (var ns in namespaces)
            {
                sb.AppendLine("import { " + ns + " } from './" + ns.ToLower() + "';");
            }
            sb.AppendLine();
            sb.AppendLine("declare namespace DTOs {");

            foreach (var dtoType in _dtoTypes)
            {
                WriteInterface(sb, dtoType);
            }
            sb.AppendLine("}");
        }

        private HashSet<string> GetNameSpacesForCustomProperties(IEnumerable<Type> ownerTypes)
        {
            var namespaces = new HashSet<string>();
            foreach (var propType in ownerTypes.SelectMany(x => x.GetProperties()).Select(x => x.PropertyType).Distinct())
            {
                if (_customTypes.ContainsKey(propType))
                {
                    var customTypeNamespace = _customTypes[propType];
                    if (!namespaces.Contains(customTypeNamespace))
                    {
                        namespaces.Add(customTypeNamespace);
                    }
                }
            }

            return namespaces;
        }

        public void WriteMetadata(StringBuilder sb)
        {
            sb.AppendLine("export namespace Metadata {");            
            sb.AppendLine("\texport const Entities = ");
            sb.AppendLine("\t{"); ;
            foreach (var entityType in _entityModel.GetEntityTypes())
            {
                var qdataEntityInfo = _entityModel.GetEntityInfo(entityType);
                sb.AppendLine("\t\t" + @"""" + entityType.Name + @""":{");

                sb.AppendLine("\t\t\t" + @"""Properties"":[");
                foreach (var qdataProperty in qdataEntityInfo.Properties)
                {
                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(qdataProperty);
                    sb.AppendLine("\t\t\t\t" + serialized + ", ");
                }
                sb.AppendLine("\t\t\t],");

                sb.AppendLine("\t\t\t" + @"""CompositeEntities"":[");
                foreach (var qdataProperty in qdataEntityInfo.CompositeEntities)
                {
                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(qdataProperty);
                    sb.AppendLine("\t\t\t\t" + serialized + ", ");
                }
                sb.AppendLine("\t\t\t],");

                sb.AppendLine("\t\t},");
            }
            sb.AppendLine("\t}"); ;

            sb.AppendLine("\texport const Enums = ");
            sb.AppendLine("\t{"); ;
            foreach (var enumType in GetEnums())
            {
                sb.AppendLine("\t\t" + @"""" + enumType.Name + @""":[");
                foreach(var enumValue in Enum.GetValues(enumType))
                {
                    var enumInfo = new EnumValueInfo()
                    {
                        Value = (int)enumValue,
                        Name = enumValue.ToString(),
                        DisplayName = GetEnumDescription((Enum)enumValue)
                    };

                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(enumInfo);
                    sb.AppendLine("\t\t\t" + serialized + ", ");
                }
                sb.AppendLine("\t\t],");
            }
            sb.AppendLine("\t}"); ;
            sb.AppendLine("}"); ;
        }

        public void WriteCustomTypes(StringBuilder sb)
        {
            foreach (var nspaceGroup in _customTypes.GroupBy(x => x.Value))
            {
                sb.AppendLine("export namespace " + nspaceGroup.Key + " {");

                foreach (var customType in nspaceGroup)
                {
                    WriteInterface(sb, customType.Key);
                }
                sb.AppendLine("}");
            }
        }

        private static void WriteEnum(StringBuilder sb, Type enumType)
        {
            sb.AppendLine("\texport const enum " + enumType.Name + " {");
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                sb.AppendLine("\t\t" + enumValue + " = " + (int)enumValue + ",");
            }
            sb.AppendLine("\t}"); ;
        }

        private void WriteInterface(StringBuilder sb, Type clrType)
        {
            sb.AppendLine("\tinterface " + clrType.Name + " {");
            foreach (var propInfo in clrType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
            {
                sb.AppendLine("\t\t" + propInfo.Name + ": " + TranslatePropertyType(propInfo.PropertyType) + ";");
            }
            sb.AppendLine("\t}");
        }

        private List<Type> GetEnums()
        {
            var enums = new List<Type>();
            foreach (var entityType in _entityModel.GetEntityTypes())
            {
                var qdataEntityInfo = _entityModel.GetEntityInfo(entityType);
                foreach (var propInfo in entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
                {
                    var qdataPropertyInfo = qdataEntityInfo.Properties.FirstOrDefault(x => x.Name == propInfo.Name);
                    if (qdataPropertyInfo != null && (qdataPropertyInfo.PropertyRole == QDataPropertyRoles.None || qdataPropertyInfo.PropertyRole == QDataPropertyRoles.Computed))
                        continue;

                    if (propInfo.PropertyType.IsEnum && !enums.Contains(propInfo.PropertyType))
                    {
                        enums.Add(propInfo.PropertyType);
                    }
                }
            }

            foreach(var enumType in _enumTypes)
            {
                if (!enums.Contains(enumType))
                    enums.Add(enumType);
            }

            foreach (var dtoType in _dtoTypes)
            {
                foreach (var propInfo in dtoType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
                {
                    if (propInfo.PropertyType.IsEnum && !enums.Contains(propInfo.PropertyType))
                    {
                        enums.Add(propInfo.PropertyType);
                    }
                }
            }

            return enums;
        }

        string TranslatePropertyType(Type clrType)
        {
            var underlyingType = Nullable.GetUnderlyingType(clrType);
            if (underlyingType != null)
                clrType = underlyingType;

            if (_customTypes.ContainsKey(clrType))
                return _customTypes[clrType] + "." + clrType.Name;

            if(_dtoTypes.Contains(clrType))
                return "DTOs." + clrType.Name;

            if (clrType.IsEnum)
                return "Enums." + clrType.Name;

            switch (clrType.Name)
            {
                case "Boolean":
                    return "boolean";
                case "String":
                case "Char":
                    return "string";
                case "Byte":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                case "Single":
                case "Double":
                case "Decimal":
                case "IntPtr":
                case "UIntPtr":
                    return "number";
                case "DateTime":
                case "DateTimeOffset":
                    return "Date";
            }

            if(_entityModel.GetEntityTypes().Contains(clrType))
            {
                return "Entities." + clrType.Name;
            }

            var ien = clrType
             .GetInterfaces()
             .SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if(ien != null)
            {
                var genericType = ien.GetGenericArguments()[0];
                var translatedGenericType = TranslatePropertyType(genericType);
                return translatedGenericType + "[]";
            }
            return "any";
        }

        private class EnumValueInfo
        {
            public int Value { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
        }

        static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

}
