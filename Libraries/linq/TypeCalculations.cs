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
using System.Text;

namespace VDS.RDF.Linq
{
    public interface ITypeTranslator2
    {
        bool IsPrimitiveType(string typeName);
        string this[PrimitiveDataType val] { get; }
    }
    public enum SupportedTypeDomain
    {
        DotNet,
        XmlSchemaDatatypes
    }
    public enum PrimitiveDataType : int
    {
        // numerical types
        SHORT,
        INT,
        LONG,
        FLOAT,
        DOUBLE,
        DECIMAL,
        // logical truth values
        BOOLEAN,
        // string and binary data
        STRING,
        HEXBINARY,
        BASE64BINARY,
        // date and time values
        DURATION,
        DATETIME,
        TIME,
        DATE,
        GYEARMONTH,
        GYEAR,
        GMONTHDAY,
        GDAY,
        GMONTH,
        // identifiers
        ANYURI,
        QNAME,
        NOTATION,
        // other
        UNKNOWN
    }
    public static class TypeTranslationProvider
    {
        public static ITypeTranslator2 GetTypeTranslator(SupportedTypeDomain td)
        {
            if (td == SupportedTypeDomain.DotNet)
                return new DotnetTypeTranslator();
            else
                return new XsdtTypeTranslator();
        }
    }
    public class TypeTranslatorBase
    {
        public bool IsPrimitiveType(string typeName)
        {
            try
            {
                PrimitiveDataType dt = (PrimitiveDataType)Enum.Parse(typeof(PrimitiveDataType), typeName.ToUpperInvariant());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public sealed class DotnetTypeTranslator : TypeTranslatorBase, ITypeTranslator2
    {
    	public string this[PrimitiveDataType val]
        {
            get
            {
                switch (val)
                {
                    case PrimitiveDataType.SHORT:
                        return "short";
                    case PrimitiveDataType.INT:
                        return "int";
                    case PrimitiveDataType.LONG:
                        return "long";
                    case PrimitiveDataType.FLOAT:
                        return "float";
                    case PrimitiveDataType.DOUBLE:
                        return "double";
                    case PrimitiveDataType.DECIMAL:
                        return "decimal";
                    case PrimitiveDataType.BOOLEAN:
                        return "bool";
                    case PrimitiveDataType.STRING:
                        return "string";
                    case PrimitiveDataType.HEXBINARY:
                        return "byte[]";
                    case PrimitiveDataType.BASE64BINARY:
                        return "char[]";
                    case PrimitiveDataType.DURATION:
                        return "TimeSpan";
                    case PrimitiveDataType.DATETIME:
                        return "DateTime";
                    case PrimitiveDataType.TIME:
                        return "DateTime"; // dubious
                    case PrimitiveDataType.DATE:
                        return "DateTime";
                    case PrimitiveDataType.GYEARMONTH:
                        return "DateTime";
                    case PrimitiveDataType.GYEAR:
                        return "uint";
                    case PrimitiveDataType.GMONTHDAY:
                        return "DateTime";
                    case PrimitiveDataType.GDAY:
                        return "uint";
                    case PrimitiveDataType.GMONTH:
                        return "uint";
                    case PrimitiveDataType.ANYURI:
                        return "string";
                    case PrimitiveDataType.QNAME:
                        return "string";
                    case PrimitiveDataType.NOTATION:
                    case PrimitiveDataType.UNKNOWN:
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
    public sealed class XsdtTypeTranslator : TypeTranslatorBase, ITypeTranslator2
    {
    	public string this[PrimitiveDataType val]
        {
            get
            {
                switch (val)
                {
                    case PrimitiveDataType.SHORT:
                        return "short";
                    case PrimitiveDataType.INT:
                        return "integer";
                    case PrimitiveDataType.LONG:
                        return "long";
                    case PrimitiveDataType.FLOAT:
                        return "float";
                    case PrimitiveDataType.DOUBLE:
                        return "double";
                    case PrimitiveDataType.DECIMAL:
                        return "decimal";
                    case PrimitiveDataType.BOOLEAN:
                        return "boolean";
                    case PrimitiveDataType.STRING:
                        return "string";
                    case PrimitiveDataType.HEXBINARY:
                        return "hexBinary";
                    case PrimitiveDataType.BASE64BINARY:
                        return "base64Binary";
                    case PrimitiveDataType.DURATION:
                        return "duration";
                    case PrimitiveDataType.DATETIME:
                        return "dateTime";
                    case PrimitiveDataType.TIME:
                        return "time"; // dubious
                    case PrimitiveDataType.DATE:
                        return "date";
                    case PrimitiveDataType.GYEARMONTH:
                        return "gYearMonth";
                    case PrimitiveDataType.GYEAR:
                        return "gYear";
                    case PrimitiveDataType.GMONTHDAY:
                        return "gMonthDay";
                    case PrimitiveDataType.GDAY:
                        return "gDay";
                    case PrimitiveDataType.GMONTH:
                        return "gMonth";
                    case PrimitiveDataType.ANYURI:
                        return "anyUri";
                    case PrimitiveDataType.QNAME:
                        return "QName";
                    case PrimitiveDataType.NOTATION:
                        return "NOTATION";
                    case PrimitiveDataType.UNKNOWN:
                        return "string";
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
