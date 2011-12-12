using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Nodes
{
    /// <summary>
    /// A Valued Node with a numeric value
    /// </summary>
    public abstract class NumericNode
        : LiteralNode, IValuedNode
    {
        private SparqlNumericType _numType = SparqlNumericType.NaN;

        public NumericNode(IGraph g, String value, Uri datatype, SparqlNumericType numType)
            : base(g, value, datatype) 
        {
            this._numType = numType;
        }

        public string AsString()
        {
            return this.Value;
        }

        public abstract long AsInteger();

        public abstract decimal AsDecimal();

        public abstract float AsFloat();

        public abstract double AsDouble();

        public bool AsBoolean()
        {
            switch (this._numType)
            {
                case SparqlNumericType.Integer:
                    return this.AsInteger() != 0;
                case SparqlNumericType.Decimal:
                    return this.AsDecimal() != Decimal.Zero;
                case SparqlNumericType.Float:
                    return this.AsFloat() != 0.0f && this.AsFloat() != Single.NaN;
                case SparqlNumericType.Double:
                    return this.AsDouble() != 0.0d && this.AsDouble() != Double.NaN;
                default:
                    return SparqlSpecsHelper.EffectiveBooleanValue(this);
            }
        }

        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Date Times");
        }

        public SparqlNumericType NumericType
        {
            get
            {
                return this._numType;
            }
        }
    }
}
