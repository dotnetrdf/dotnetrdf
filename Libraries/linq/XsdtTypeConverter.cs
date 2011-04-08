/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// a list of typical primitive datatypes defined in the XMLSchema datatypes (xsd) specification.
    /// </summary>
    public enum XsdtPrimitiveDataType
    {
        /// <summary>
        /// string type
        /// </summary>
        [Xsdt(true, "string")] XsdtString,
        /// <summary>
        /// boolean type
        /// </summary>
        [Xsdt(false, "boolean")] XsdtBoolean,
        /// <summary>
        /// short (i.e. 16 bit integer) type
        /// </summary>
        [Xsdt(false, "short")] XsdtShort,
        /// <summary>
        /// integer (32 bits) type
        /// </summary>
        [Xsdt(false, "integer")] XsdtInt,
        /// <summary>
        /// long (64 bits) type
        /// </summary>
        [Xsdt(false, "long")] XsdtLong,
        /// <summary>
        /// float type
        /// </summary>
        [Xsdt(false, "float")] XsdtFloat,
        /// <summary>
        /// double type
        /// </summary>
        [Xsdt(false, "double")] XsdtDouble,
        /// <summary>
        /// decimal type
        /// </summary>
        [Xsdt(false, "decimal")] XsdtDecimal,
        /// <summary>
        /// duration (i.e. TimeSpan) type
        /// </summary>
        [Xsdt(true, "duration")] XsdtDuration,
        /// <summary>
        /// Day Time Duration (i.e. Timespan) type
        /// </summary>
        [Xsdt(true, "dayTimeDuration")] XsdtDayTimeDuration,
        /// <summary>
        /// datetime type
        /// </summary>
        [Xsdt(true, "dateTime")] XsdtDateTime,
        /// <summary>
        /// time (exposed as <see cref="SoapTime"/>) type
        /// </summary>
        [Xsdt(true, "time")] XsdtTime,
        /// <summary>
        /// date type (exposed as DateTime
        /// </summary>
        [Xsdt(true, "date")] XsdtDate,
        /// <summary>
        /// year-month type (exposed as <see cref="SoapGMonthDay"/>)
        /// </summary>
        [Xsdt(true, "gYearMonth")] XsdtGYearMonth,
        /// <summary>
        /// year type (exposed as <see cref="SoapYear"/>
        /// </summary>
        [Xsdt(true, "gYear")] XsdtGYear,
        /// <summary>
        /// month-day type (exposed as <see cref="SoapGMonthDay"/>
        /// </summary>
        [Xsdt(true, "gMonthDay")] XsdtGMonthDay,
        /// <summary>
        /// day type (exposed as <see cref="SoapGDay"/>
        /// </summary>
        [Xsdt(true, "gDay")] XsdtGDay,
        /// <summary>
        /// month type (exposed as <see cref="SoapGMonth"/>
        /// </summary>
        [Xsdt(true, "gMonth")] XsdtGMonth,
        /// <summary>
        /// hex binary type (expoosed as byte[]
        /// </summary>
        [Xsdt(true, "hexBinary")] XsdtHexBinary,
        /// <summary>
        /// base 64 binary type (exposed as byte[])
        /// </summary>
        [Xsdt(true, "base64Binary")] XsdtBase64Binary,
        /// <summary>
        /// any URI type (expoosed as <see cref="Uri"/>
        /// </summary>
        [Xsdt(false, "anyUri")] XsdtAnyUri,
        /// <summary>
        /// Qname type (expoosed as <see cref="SoapQName"/>
        /// </summary>
        [Xsdt(true, "QName")] XsdtQName,
        /// <summary>
        /// notation type (expoosed as <see cref="SoapNotation"/>
        /// </summary>
        [Xsdt(true, "Notation")] XsdtNotation,
        /// <summary>
        /// error type - only used if a type is returned that the type calculators can't understand
        /// </summary>
        [Xsdt(false, "")] XsdtUnknown
    }

    /// <summary>
    /// interface to type calculators that can convert between type systems
    /// </summary>
    public interface ITypeTranslator
    {
        /// <summary>
        /// Gets the xsd datatype of a .NET type.
        /// </summary>
        /// <param name="t">a .NET type.</param>
        /// <returns>the corresponding type to be used in XSDT</returns>
        XsdtPrimitiveDataType GetDataType(Type t);

        /// <summary>
        /// Gets an XSDT encoding of the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        object Get<T>(T obj);

        object Get(Type t, object obj);
    }

    /// <summary>
    /// A small class to hold the intermediate 
    /// results of type calculations and conversions
    /// </summary>
    internal class XsdtType
    {
        public string XsdtNamespacePrefix { get; set; }

        public string RawInput { get; set; }

        public string ValuePart { get; set; }

        public string TypePart { get; set; }

        public XsdtPrimitiveDataType XsdtTypeName { get; set; }

        public XsdtAttribute XsdtAttr { get; set; }

        public Type NetType { get; set; }

        public object NetValue { get; set; }
    }

    public class XsdtTypeConverter : ITypeTranslator
    {
        private Dictionary<XsdtPrimitiveDataType, Type> typeLookup = new Dictionary<XsdtPrimitiveDataType, Type>();
        private Graph _g = new Graph();
        private SparqlFormatter _formatter;

        #region Conversion from .NET to Xsdt (string)

        public XsdtPrimitiveDataType GetDataType(Type t)
        {
            if (TypeLookup.ContainsValue(t))
                return (from x in TypeLookup where x.Value == t select x.Key).FirstOrDefault();
            else return XsdtPrimitiveDataType.XsdtUnknown;
        }

        public object Get<T>(T obj)
        {
            return Get(typeof (T), obj);
        }

        public object Get(Type t, object obj)
        {
            //PORT: Convert this just to use SparqlFormatter instead?

            string result = "";
            XsdtPrimitiveDataType dt = GetDataType(t);
            XsdtAttribute attr = GetXsdtAttrFor(dt);

            //if (dt == XsdtPrimitiveDataType.XsdtUnknown)
            //{
            //    return this._formatter.Format(this._g.CreateLiteralNode(obj.ToString()));
            //}
            //else
            //{
            //    XsdtAttribute attr = GetXsdtAttrFor(dt);

            //}

            if (dt == XsdtPrimitiveDataType.XsdtUnknown)
            {
                if (t == typeof (char))
                {
                    return "\"" + obj + "\"";
                }
                else if (attr.IsQuoted)
                {
                    return "\"" + obj + "\"";
                }
                else return obj.ToString();
            }
            switch (t.FullName)
            {
                case "System.DateTime":
                    result = GetXsdtDateRepresentationFor((DateTime) obj, dt, attr);
                    break;
                case "System.Byte[]":
                    result = Encoding.ASCII.GetString((Byte[]) obj);
                    break;
                default:
                    result = GetStringRepresentationFor(obj, dt, attr);
                    break;
            }

            if (attr.IsQuoted)
            {
                result = "\"" + result + "\"";
            }
            string xsdTypeSuffix;
            if (dt != XsdtPrimitiveDataType.XsdtInt && dt != XsdtPrimitiveDataType.XsdtString)
                xsdTypeSuffix = "^^xsd:" + attr.TypeUri;
            else
                xsdTypeSuffix = "";
            return result + xsdTypeSuffix;
        }

        public string GetXsdtDateRepresentationFor(DateTime d, XsdtPrimitiveDataType dt, XsdtAttribute attr)
        {
            // TODO: the time zone offset needs to be returned from somewhere...
            return d.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        internal string GetStringRepresentationFor<T>(T obj, XsdtPrimitiveDataType dt, XsdtAttribute attr)
        {
            return obj.ToString();
        }

        internal XsdtAttribute GetXsdtAttrFor(Type t)
        {
            return GetXsdtAttrFor(GetDataType(t));
        }

        internal XsdtAttribute GetXsdtAttrFor(XsdtPrimitiveDataType dt)
        {
            FieldInfo fi = dt.GetType().GetField(dt.ToString());
            var attrs = (XsdtAttribute[]) fi.GetCustomAttributes(typeof (XsdtAttribute), false);
            return attrs[0]; //  I know by design that there will be one and only one attribute in this array.
        }

        #endregion

        #region Conversion from Xsdt (string) to .NET

        public object Parse(INode n)
        {
            return Parse(n, "xsd:");
        }

        public object Parse(INode n, String ns)
        {
            XsdtType t = new XsdtType();
            t.XsdtNamespacePrefix = ns;
            Parse(n, t);
            return t.NetValue;
        }

        internal void Parse(INode n, XsdtType x)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    x.NetType = typeof(Uri);
                    x.ValuePart = n.ToString();
                    x.XsdtTypeName = XsdtPrimitiveDataType.XsdtAnyUri;
                    ConvertValueToNet(x);
                    break;

                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;

                    x.XsdtTypeName = XsdtPrimitiveDataType.XsdtUnknown;
                    x.ValuePart = lit.Value;

                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().StartsWith(NamespaceMapper.XMLSCHEMA))
                        {
                            x.TypePart = lit.DataType.ToString().Substring(NamespaceMapper.XMLSCHEMA.Length);
                        }
                        else
                        {
                            x.TypePart = "string";
                        }
                    }
                    else
                    {
                        if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value))
                        {
                            if (TurtleSpecsHelper.IsValidInteger(lit.Value))
                            {
                                x.TypePart = "integer";
                            }
                            else if (TurtleSpecsHelper.IsValidDecimal(lit.Value))
                            {
                                x.TypePart = "decimal";
                            }
                            else if (TurtleSpecsHelper.IsValidDouble(lit.Value))
                            {
                                x.TypePart = "double";
                            }
                            else
                            {
                                x.TypePart = "boolean";
                            }
                        }
                        else
                        {
                            x.TypePart = "string";
                        }
                    }

                    foreach (int i in Enum.GetValues(typeof(XsdtPrimitiveDataType)))
                    {
                        var result = (XsdtPrimitiveDataType)i;
                        XsdtAttribute attr = GetXsdtAttrFor(result);
                        if (attr.TypeUri == x.TypePart)
                        {
                            x.XsdtTypeName = result;
                        }
                    }

                    if (TypeLookup.ContainsKey(x.XsdtTypeName))
                    {
                        x.NetType = TypeLookup[x.XsdtTypeName];
                    }
                    else
                    {
                        throw new LinqToRdfException("XsdtTypeConverter does not know how to convert the XML Schema Datatype " + x.TypePart);
                    }

                    ConvertValueToNet(x);
                    break;

                default:
                    throw new LinqToRdfException("XsdtTypeConverter can only convert URI and Literal Nodes");
            }
        }

        private void ConvertValueToNet(XsdtType x)
        {
            if (x.NetType == null)
            {
                throw new LinqToRdfException("unable to convert when no .net type was found");
            }

            switch (x.XsdtTypeName)
            {
                case XsdtPrimitiveDataType.XsdtString:
                    x.NetValue = x.ValuePart;
                    x.NetType = x.NetValue.GetType();
                    break;
                case XsdtPrimitiveDataType.XsdtBoolean:
                case XsdtPrimitiveDataType.XsdtShort:
                case XsdtPrimitiveDataType.XsdtInt:
                case XsdtPrimitiveDataType.XsdtLong:
                case XsdtPrimitiveDataType.XsdtFloat:
                case XsdtPrimitiveDataType.XsdtDouble:
                case XsdtPrimitiveDataType.XsdtDecimal:
                    x.NetValue = Convert.ChangeType(x.ValuePart, x.NetType);
                    break;
                case XsdtPrimitiveDataType.XsdtDuration:
                    ParseDuration(x);
                    break;
                case XsdtPrimitiveDataType.XsdtDateTime:
                    ParseDateTime(x);
                    break;
                case XsdtPrimitiveDataType.XsdtTime:
                    ParseTime(x);
                    break;
                case XsdtPrimitiveDataType.XsdtDate:
                    ParseDate(x);
                    break;
                case XsdtPrimitiveDataType.XsdtGYearMonth:
                    ParseYearMonth(x);
                    break;
                case XsdtPrimitiveDataType.XsdtGYear:
                    ParseYear(x);
                    break;
                case XsdtPrimitiveDataType.XsdtGMonthDay:
                    ParseMonthDay(x);
                    break;
                case XsdtPrimitiveDataType.XsdtGDay:
                    ParseDay(x);
                    break;
                case XsdtPrimitiveDataType.XsdtGMonth:
                    ParseMonth(x);
                    break;
                case XsdtPrimitiveDataType.XsdtHexBinary:
                    ParseHexBinary(x);
                    break;
                case XsdtPrimitiveDataType.XsdtBase64Binary:
                    ParseBase64Binary(x);
                    break;
                case XsdtPrimitiveDataType.XsdtAnyUri:
                    ParseUri(x);
                    break;
                case XsdtPrimitiveDataType.XsdtQName:
                    ParseQName(x);
                    break;
                case XsdtPrimitiveDataType.XsdtNotation:
                    ParseNotation(x);
                    break;
                case XsdtPrimitiveDataType.XsdtUnknown:
                    ParseObject(x);
                    break;
                default:
                    break;
            }
        }

        private object UnescapeXsdtEntities(string p)
        {
            // TODO: explore whether there ixs a need to unescape the entities in an xsd string
            return p;
        }

        #region Parsing strings to .NET types

        private void ParseObject(XsdtType x)
        {
            x.NetValue = x.ValuePart;
            x.NetType = typeof (string);
        }

        private void ParseNotation(XsdtType x)
        {
            SoapNotation u = SoapNotation.Parse(x.ValuePart);
            x.NetValue = u;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseQName(XsdtType x)
        {
            SoapQName u = SoapQName.Parse(x.ValuePart);
            x.NetValue = u;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseUri(XsdtType x)
        {
            SoapAnyUri u = SoapAnyUri.Parse(x.ValuePart);
            x.NetValue = new Uri(u.Value);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseBase64Binary(XsdtType x)
        {
            SoapBase64Binary hb = SoapBase64Binary.Parse(x.ValuePart);
            x.NetValue = hb.Value;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseHexBinary(XsdtType x)
        {
            SoapHexBinary hb = SoapHexBinary.Parse(x.ValuePart);
            x.NetValue = hb.Value;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseMonth(XsdtType x)
        {
            x.NetValue = SoapMonth.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseDay(XsdtType x)
        {
            x.NetValue = SoapDay.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseMonthDay(XsdtType x)
        {
            x.NetValue = SoapMonthDay.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseYear(XsdtType x)
        {
            x.NetValue = SoapYear.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseYearMonth(XsdtType x)
        {
            x.NetValue = SoapYearMonth.Parse(x.ValuePart).Value;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseDate(XsdtType x)
        {
            x.NetValue = SoapDate.Parse(x.ValuePart).Value;
            x.NetType = x.NetValue.GetType();
        }

        private void ParseTime(XsdtType x)
        {
            x.NetValue = SoapTime.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseDateTime(XsdtType x)
        {
            x.NetValue = SoapDateTime.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        private void ParseDuration(XsdtType x)
        {
            x.NetValue = SoapDuration.Parse(x.ValuePart);
            x.NetType = x.NetValue.GetType();
        }

        #endregion

        #endregion

        public XsdtTypeConverter()
        {
            this._formatter = new SparqlFormatter(this._g);

            typeLookup.Add(XsdtPrimitiveDataType.XsdtString, typeof (string));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtBoolean, typeof (Boolean));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtFloat, typeof (Single));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDouble, typeof (Double));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDecimal, typeof (Decimal));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDuration, typeof (TimeSpan));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDayTimeDuration, typeof(TimeSpan));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtShort, typeof (Int16));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtInt, typeof (Int32));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtLong, typeof (Int64));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtHexBinary, typeof (Byte[]));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtAnyUri, typeof (Uri));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDateTime, typeof (DateTime));
            typeLookup.Add(XsdtPrimitiveDataType.XsdtDate, typeof (DateTime));
        }

        public Dictionary<XsdtPrimitiveDataType, Type> TypeLookup
        {
            get { return typeLookup; }
            set { typeLookup = value; }
        }
    }
}