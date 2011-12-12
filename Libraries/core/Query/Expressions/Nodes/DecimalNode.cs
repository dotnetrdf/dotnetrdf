using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class DecimalNode
        : NumericNode
    {
        private decimal _value;

        public DecimalNode(IGraph g, decimal value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal), SparqlNumericType.Decimal)
        {
            this._value = value;
        }

        public DecimalNode(IGraph g, decimal value)
            : this(g, value, value.ToString()) { }

        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to downcast Decimal to Long");
            }
        }

        public override decimal AsDecimal()
        {
            return this._value;
        }

        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to cast Decimal to Float");
            }
        }

        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to cast Decimal to Double");
            }
        }
    }
}
