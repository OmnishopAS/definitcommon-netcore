using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Omnishop.POS.DAL
{
    public class CompareUtilities
    {
        public static bool IsEqual(object valueA, object valueB)
        {
            IComparable selfValueComparer;

            selfValueComparer = valueA as IComparable;

            // both values are null
            if (valueA == null && valueB == null)
                return true;

            // one of the values is null
            if (valueA == null && valueB != null || valueA != null && valueB == null)
                return false;

            if (selfValueComparer != null)
                return  selfValueComparer.CompareTo(valueB) == 0;

            if (valueA.GetType().IsArray && valueB.GetType().IsArray)
            {
                var lst1 = valueA as IList;
                var lst2 = valueB as IList;
                if (lst1.Count != lst2.Count)
                    return false;

                for (int i = 0; i < lst1.Count; i++)
                {
                    if (!IsEqual(lst1[i], lst2[i]))
                        return false;
                }
                return true;
            }

            return object.Equals(valueA, valueB);           
        }

        public static bool IsNullOrDefault(object value)
        {
            if (value == null)
                return true;

            var valueTypeName = value.GetType().FullName;
            if (valueTypeName == typeof(byte).FullName)
                return (byte)value == 0;
            if (valueTypeName == typeof(short).FullName)
                return (short)value == 0;
            if (valueTypeName == typeof(int).FullName)
                return (int)value == 0;
            if (valueTypeName == typeof(long).FullName)
                return (long)value == 0;
            if (valueTypeName == typeof(sbyte).FullName)
                return (sbyte)value == 0;
            if (valueTypeName == typeof(ushort).FullName)
                return (ushort)value == 0;
            if (valueTypeName == typeof(uint).FullName)
                return (uint)value == 0;
            if (valueTypeName == typeof(ulong).FullName)
                return (ulong)value == 0;
            if (valueTypeName == typeof(decimal).FullName)
                return (decimal)value == 0m;
            if (valueTypeName == typeof(float).FullName)
                return (float)value == 0f;
            if (valueTypeName == typeof(double).FullName)
                return (double)value == 0d;
            if (valueTypeName == typeof(bool).FullName)
                return (bool)value == false;

            return false;
        }

        public static bool IsNumericType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
