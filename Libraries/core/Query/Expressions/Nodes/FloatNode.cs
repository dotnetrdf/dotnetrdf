using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class FloatNode
        : NumericNode
    {
        private float _value;

        public FloatNode(IGraph g, float value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeFloat), SparqlNumericType.Float)
        {
            this._value = value;
        }

        public FloatNode(IGraph g, float value)
            : this(g, value, value.ToString()) { }

        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to downcast Float to Long");
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
                throw new RdfQueryException("Unable to cast Float to Decimal");
            }
        }

        public override float AsFloat()
        {
            return this._value;
        }

        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast Float to Double");
            }
        }
    }
}
