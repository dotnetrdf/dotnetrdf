using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Nodes
{
    public class StringNode
        : LiteralNode, IValuedNode
    {
        public StringNode(IGraph g, String value, Uri datatype)
            : base(g, value, datatype) { }

        public StringNode(IGraph g, String value, String lang)
            : base(g, value, lang) { }

        public StringNode(IGraph g, String value)
            : base(g, value) { }

        #region IValuedNode Members

        public string AsString()
        {
            return this.Value;
        }

        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public bool AsBoolean()
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this);
        }

        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }

        #endregion
    }
}
