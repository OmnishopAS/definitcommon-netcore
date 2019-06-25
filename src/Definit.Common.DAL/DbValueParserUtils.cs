using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Definit.Common.DAL
{
    public static class DbValueParserUtils
    {
        public static string ParseToString(object value, string defaultValue = null)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            return value.ToString().Trim();
        }

        public static T ParseToValue<T>(object value, T? defaultValue = null)
            where T : struct
        {
            if (value == null || value == DBNull.Value)
            {
                if (defaultValue.HasValue)
                    return defaultValue.Value;
                return default(T);
            }
            return (T)System.Convert.ChangeType(value, typeof(T));
        }

        public static T FieldValueOrDefault<T>(DbDataReader reader, string fieldName, T defaultValue=default(T))
            where T:struct
        {
            var rowValue = reader[fieldName];
            if (rowValue == DBNull.Value)
                return defaultValue;
            return (T)rowValue;
        }

    }
}
