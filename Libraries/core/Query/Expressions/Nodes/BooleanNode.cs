using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class BooleanNode
        : LiteralNode, IValuedNode
    {
        private bool _value;

        public BooleanNode(IGraph g, bool value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
        {
            this._value = value;
        }

        public BooleanNode(IGraph g, bool value)
            : this(g, value, value.ToString().ToLower()) { }

        public string AsString()
        {
            return this.Value;
        }

        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        public bool AsBoolean()
        {
            return this._value;
        }

        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        public SparqlNumericType NumericType
        {
            get 
            { 
                return SparqlNumericType.NaN; 
            }
        }
    }
}
