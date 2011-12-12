using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class DateTimeNode
        : LiteralNode, IValuedNode
    {
        private DateTimeOffset _value;


        protected DateTimeNode(IGraph g, DateTimeOffset value, String lexicalValue, Uri datatype)
            : base(g, lexicalValue, datatype)
        {
            this._value = value;
        }

        protected DateTimeNode(IGraph g, DateTimeOffset value, Uri datatype)
            : this(g, value, GetStringForm(value, datatype), datatype) { }

        public DateTimeNode(IGraph g, DateTimeOffset value)
            : this(g, value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        public DateTimeNode(IGraph g, DateTimeOffset value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime)) { }

        private static String GetStringForm(DateTimeOffset value, Uri datatype)
        {
            switch (datatype.ToString())
            {
                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                    return value.ToString(XmlSpecsHelper.XmlSchemaDateFormat);
                default:
                    throw new RdfQueryException("Unable to create a DateTimeNode because datatype URI is invalid");
            }
        }

        public string AsString()
        {
            return this.Value;
        }

        public long AsInteger()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public float AsFloat()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public double AsDouble()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public bool AsBoolean()
        {
            throw new RdfQueryException("Cannot convert Date Times to other types");
        }

        public DateTimeOffset AsDateTime()
        {
            return this._value;
        }

        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }
    }

    public class DateNode
        : DateTimeNode
    {
        public DateNode(IGraph g, DateTimeOffset value)
            : base(g, value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        public DateNode(IGraph g, DateTimeOffset value, String lexicalValue)
            : base(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }
    }
}
