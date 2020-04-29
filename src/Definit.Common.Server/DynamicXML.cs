using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Xml.Linq;
using System.Collections;

namespace Omnishop.Common.Server
{
    /// <summary>
    /// http://blogs.msdn.com/b/mcsuksoldev/archive/2010/02/04/dynamic-xml-reader-with-c-and-net-4-0.aspx
    /// </summary>
    public class DynamicXmlParser : DynamicObject, IEnumerable<DynamicXmlParser>
    {
        XElement element;

        #region Explicit cast operators. Declarations copied from XElement. Implementations simply forward to wrapped XElement.
        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to an System.Int32.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Int32.
        //
        // Returns:
        //     A System.Int32 that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Int32 value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator int(DynamicXmlParser dynamicElement)
        {
            return (int)dynamicElement.element;
        }

        public static explicit operator int?(DynamicXmlParser dynamicElement)
        {
            return (int?)dynamicElement.element;
        }

        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.UInt32.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.UInt32.
        //
        // Returns:
        //     A System.UInt32 that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.UInt32 value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator uint(DynamicXmlParser dynamicElement)
        {
            return (uint)dynamicElement.element;
        }

        public static explicit operator uint?(DynamicXmlParser dynamicElement)
        {
            return (uint?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.Boolean.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Boolean.
        //
        // Returns:
        //     A System.Boolean that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Boolean value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator bool(DynamicXmlParser dynamicElement)
        {
            return (bool)dynamicElement.element;
        }

        public static explicit operator bool?(DynamicXmlParser dynamicElement)
        {
            return (bool?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to an System.Int64.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Int64.
        //
        // Returns:
        //     A System.Int64 that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Int64 value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator long(DynamicXmlParser dynamicElement)
        {
            return (long)dynamicElement.element;
        }

        public static explicit operator long?(DynamicXmlParser dynamicElement)
        {
            return (long?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.UInt64.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.UInt64.
        //
        // Returns:
        //     A System.UInt64 that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.UInt64 value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator ulong(DynamicXmlParser dynamicElement)
        {
            return (ulong)dynamicElement.element;
        }

        public static explicit operator ulong?(DynamicXmlParser dynamicElement)
        {
            return (ulong?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.String.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.String.
        //
        // Returns:
        //     A System.String that contains the content of this System.Xml.Linq.DynamicXmlParser.

        public static explicit operator string(DynamicXmlParser dynamicElement)
        {
            return (string)dynamicElement.element;
        }



        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.Guid.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Guid.
        //
        // Returns:
        //     A System.Guid that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Guid value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator Guid(DynamicXmlParser dynamicElement)
        {
            return (Guid)dynamicElement.element;
        }

        public static explicit operator Guid?(DynamicXmlParser dynamicElement)
        {
            return (Guid?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.Single.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Single.
        //
        // Returns:
        //     A System.Single that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Single value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator float(DynamicXmlParser dynamicElement)
        {
            return (float)dynamicElement.element;
        }

        public static explicit operator float?(DynamicXmlParser dynamicElement)
        {
            return (float?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.Double.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Double.
        //
        // Returns:
        //     A System.Double that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Double value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator double(DynamicXmlParser dynamicElement)
        {
            return (double)dynamicElement.element;
        }

        public static explicit operator double?(DynamicXmlParser dynamicElement)
        {
            return (double?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.Decimal.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.Decimal.
        //
        // Returns:
        //     A System.Decimal that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.Decimal value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator decimal(DynamicXmlParser dynamicElement)
        {
            return (decimal)dynamicElement.element;
        }

        public static explicit operator decimal?(DynamicXmlParser dynamicElement)
        {
            return (decimal?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.TimeSpan.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.TimeSpan.
        //
        // Returns:
        //     A System.TimeSpan that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.TimeSpan value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator TimeSpan(DynamicXmlParser dynamicElement)
        {
            return (TimeSpan)dynamicElement.element;
        }

        public static explicit operator TimeSpan?(DynamicXmlParser dynamicElement)
        {
            return (TimeSpan?)dynamicElement.element;
        }

        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.DynamicXmlParser to a System.DateTime.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.DateTime.
        //
        // Returns:
        //     A System.DateTime that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.DateTime value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator DateTime(DynamicXmlParser dynamicElement)
        {
            return (DateTime)dynamicElement.element;
        }

        public static explicit operator DateTime?(DynamicXmlParser dynamicElement)
        {
            return (DateTime?)dynamicElement.element;
        }


        //
        // Summary:
        //     Cast the value of this System.Xml.Linq.XAttribute to a System.DateTimeOffset.
        //
        // Parameters:
        //   dynamicElement:
        //     The System.Xml.Linq.DynamicXmlParser to cast to System.DateTimeOffset.
        //
        // Returns:
        //     A System.DateTimeOffset that contains the content of this System.Xml.Linq.DynamicXmlParser.
        //
        // Exceptions:
        //   System.FormatException:
        //     The dynamicElement does not contain a valid System.DateTimeOffset value.
        //
        //   System.ArgumentNullException:
        //     The dynamicElement parameter is null.

        public static explicit operator DateTimeOffset(DynamicXmlParser dynamicElement)
        {
            return (DateTimeOffset)dynamicElement.element;
        }

        public static explicit operator DateTimeOffset?(DynamicXmlParser dynamicElement)
        {
            return (DateTimeOffset?)dynamicElement.element;
        }

        #endregion

        public DynamicXmlParser(string filename)
        {
            element = XElement.Load(filename);
        }

        public DynamicXmlParser(XElement el)
        {
            element = el;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }

            XElement sub = element.Element(binder.Name);

            if (sub == null)
            {
                sub = element.Elements().SingleOrDefault(x => x.Name.LocalName == binder.Name);
            }

            if (sub == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = new DynamicXmlParser(sub);
                return true;
            }
        }


        public override string ToString()
        {
            if (element != null)
            {
                return element.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        public Dictionary<string, DynamicXmlParser> GetKeyValuePairs()
        {
            var retValue = new Dictionary<string, DynamicXmlParser>();
            foreach (var el in element.Elements())
                retValue.Add(el.Name.LocalName, new DynamicXmlParser(el));

            return retValue;
        }


        public IEnumerator<DynamicXmlParser> GetEnumerator()
        {
            foreach (var el in element.Elements())
                yield return new DynamicXmlParser(el);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var el in element.Elements())
                yield return new DynamicXmlParser(el);
        }

    }
}