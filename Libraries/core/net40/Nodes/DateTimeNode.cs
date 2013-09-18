/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing a Date Time value
    /// </summary>
    public class DateTimeNode
        : LiteralNode, IValuedNode
    {
        private DateTime _value;
        private DateTimeOffset? _offsetValue;

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        protected DateTimeNode(DateTimeOffset value, String lexicalValue, Uri datatype)
            : base(lexicalValue, datatype)
        {
            this._value = value.UtcDateTime;
            this._offsetValue = value;
        }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        protected DateTimeNode(DateTime value, String lexicalValue, Uri datatype)
            : base(lexicalValue, datatype)
        {
            this._value = value;
            this._offsetValue = null;
        }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Date Time value</param>
        /// <param name="datatype">Datatype URI</param>
        protected DateTimeNode(DateTimeOffset value, Uri datatype)
            : this(value, GetStringForm(value, datatype), datatype) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Date Time value</param>
        /// <param name="datatype">Datatype URI</param>
        protected DateTimeNode(DateTime value, Uri datatype)
            : this(value, GetStringForm(value, datatype), datatype) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateTimeNode(DateTimeOffset value)
            : this(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateTimeNode(DateTime value)
            : this(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateTimeNode(DateTimeOffset value, String lexicalValue)
            : this(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateTimeNode(DateTime value, String lexicalValue)
            : this(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="offsetValue">Date Time offset value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateTimeNode(DateTime value, DateTimeOffset offsetValue, String lexicalValue)
            : this(value, offsetValue, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        /// <summary>
        /// Creates a new Date Time valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="offsetValue">Date Time offset value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Data Type URI</param>
        public DateTimeNode(DateTime value, DateTimeOffset offsetValue, String lexicalValue, Uri datatype)
            : base(lexicalValue, datatype)
        {
            this._value = value;
            this._offsetValue = offsetValue;
        }

        /// <summary>
        /// Gets the String form of the Date Time
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        private static String GetStringForm(DateTimeOffset value, Uri datatype)
        {
            switch (datatype.ToString())
            {
                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateFormat);
                default:
                    throw new NodeValueException("Unable to create a DateTimeNode because datatype URI is invalid");
            }
        }

        /// <summary>
        /// Gets the String form of the Date Time
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        private static String GetStringForm(DateTime value, Uri datatype)
        {
            switch (datatype.ToString())
            {
                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateFormat);
                default:
                    throw new NodeValueException("Unable to create a DateTimeNode because datatype URI is invalid");
            }
        }

        /// <summary>
        /// Gets the date time value as a string
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to integers
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to decimals
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to floats
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to doubles
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to booleans
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Gets the date time value of the node
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            return this._value;
        }

        /// <summary>
        /// Gets the date time value of the node
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            if (this._offsetValue.HasValue)
            {
                return this._offsetValue.Value;
            }
            else
            {
                //Create the offset
                DateTimeOffset offset;
                this._offsetValue = DateTimeOffset.TryParse(this.Value, null, DateTimeStyles.AssumeUniversal, out offset)
                                        ? offset
                                        : new DateTimeOffset(this._value);
                return this._offsetValue.Value;
            }
        }

        /// <summary>
        /// Throws an error as date times cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new NodeValueException("Cannot convert Date Times to other types");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return this.DataType.AbsoluteUri;
            }
        }

        /// <summary>
        /// Gets the numeric type of the node
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }
    }

    /// <summary>
    /// Valued Node representing a Date value
    /// </summary>
    public class DateNode
        : DateTimeNode
    {
        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(IGraph g, DateTimeOffset value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTimeOffset value, String lexicalValue)
            : base(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(DateTime value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTime value, String lexicalValue)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }
    }
}
