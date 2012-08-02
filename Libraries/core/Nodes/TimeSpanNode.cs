using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing a Time Span value
    /// </summary>
    public class TimeSpanNode
        : LiteralNode, IValuedNode
    {
        private TimeSpan _value;

        public TimeSpanNode(IGraph g, TimeSpan value)
            : this(g, value, XmlConvert.ToString(value), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

        public TimeSpanNode(IGraph g, TimeSpan value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

        public TimeSpanNode(IGraph g, TimeSpan value, String lexicalValue, Uri dtUri)
            : base(g, lexicalValue, dtUri)
        {
            this._value = value;
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
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to decimals
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to floats
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to doubles
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to booleans
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Gets the date time value of the node
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        public TimeSpan AsTimeSpan()
        {
            return this._value;
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
}
