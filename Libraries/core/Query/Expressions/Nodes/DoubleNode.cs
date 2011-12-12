using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class DoubleNode
        : NumericNode
    {
        private double _value;

        public DoubleNode(IGraph g, double value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble), SparqlNumericType.Double)
        {
            this._value = value;
        }

        public DoubleNode(IGraph g, double value)
            : this(g, value, value.ToString()) { }

        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to downcast Double to Long");
            }
        }

        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to cast Double to Decimal");
            }
        }

        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to downcast Double to Float");
            }
        }

        public override double AsDouble()
        {
            return this._value;
        }
    }
}
