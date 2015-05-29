/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Xml;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;

namespace VDS.RDF
{
    /// <summary>
    /// Provides extension methods for converting primitive types into appropriately typed Literal Nodes
    /// </summary>
    public static class LiteralExtensions
    {
        /// <summary>
        /// Creates a new Boolean typed literal
        /// </summary>
        /// <param name="b">Boolean</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the boolean</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this bool b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(b.ToString().ToLower(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the byte</returns>
        /// <remarks>
        /// Byte in .Net is actually equivalent to Unsigned Byte in XML Schema so depending on the value of the Byte the type will either be xsd:byte if it fits or xsd:usignedByte
        /// </remarks>
        public static INode ToLiteral(this byte b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            if (b > 128)
            {
                //If value is > 128 must use unsigned byte as the type as xsd:byte has range -127 to 128 
                //while .Net byte has range 0-255
                return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte));
            }
            else
            {
                return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
            }
        }

        /// <summary>
        /// Creates a new Byte typed literal
        /// </summary>
        /// <param name="b">Byte</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the signed bytes</returns>
        /// <remarks>
        /// SByte in .Net is directly equivalent to Byte in XML Schema so the type will always be xsd:byte
        /// </remarks>
        public static INode ToLiteral(this sbyte b, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(b), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte));
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this DateTime dt, INodeFactory factory)
        {
            return ToLiteral(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this DateTime dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this DateTimeOffset dt, INodeFactory factory)
        {
            return ToLiteral(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Date Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the date time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this DateTimeOffset dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaDateTimeFormat : XmlSpecsHelper.XmlSchemaDateTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
        }

        /// <summary>
        /// Creates a new Date typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralDate(this DateTime dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
        }

        /// <summary>
        /// Creates a new Date typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralDate(this DateTimeOffset dt, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(XmlSpecsHelper.XmlSchemaDateFormat), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate));
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralTime(this DateTime dt, INodeFactory factory)
        {
            return ToLiteralTime(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralTime(this DateTime dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
        }

        
        /// <summary>
        /// Creates a new duration typed literal
        /// </summary>
        /// <param name="t">Time Span</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time span</returns>
        public static INode ToLiteral(this TimeSpan t, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(XmlConvert.ToString(t), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration));
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory)
        {
            return ToLiteralTime(dt, factory, true);
        }

        /// <summary>
        /// Creates a new Time typed literal
        /// </summary>
        /// <param name="dt">Date Time</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds</param>
        /// <returns>Literal representing the time</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteralTime(this DateTimeOffset dt, INodeFactory factory, bool precise)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(dt.ToString(precise ? XmlSpecsHelper.XmlSchemaTimeFormat : XmlSpecsHelper.XmlSchemaTimeFormatImprecise), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime));
        }

        /// <summary>
        /// Creates a new Decimal typed literal
        /// </summary>
        /// <param name="d">Decimal</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the decimal</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this decimal d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
        }

        /// <summary>
        /// Creates a new Double typed literal
        /// </summary>
        /// <param name="d">Double</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the double</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this double d, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(d.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble));
        }

        /// <summary>
        /// Creates a new Float typed literal
        /// </summary>
        /// <param name="f">Float</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the float</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this float f, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(f.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeFloat));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the short</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this short i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="i">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this int i, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(i.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Integer typed literal
        /// </summary>
        /// <param name="l">Integer</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the integer</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Factory argument is null</exception>
        public static INode ToLiteral(this long l, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");

            return factory.CreateLiteralNode(l.ToString(CultureInfo.InvariantCulture), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new String typed literal
        /// </summary>
        /// <param name="s">String</param>
        /// <param name="factory">Node Factory to use for Node creation</param>
        /// <returns>Literal representing the string</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Graph/String argument is null</exception>
        public static INode ToLiteral(this String s, INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory", "Cannot create a Literal Node in a null Node Factory");
            if (s == null) throw new ArgumentNullException("s", "Cannot create a Literal Node for a null String");

            return factory.CreateLiteralNode(s, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }
    }
}