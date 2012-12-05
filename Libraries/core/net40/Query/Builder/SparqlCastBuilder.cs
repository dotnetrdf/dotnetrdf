using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for casting expressions to XPath types
    /// </summary>
    public sealed class SparqlCastBuilder
    {
        private readonly SparqlExpression _castedExpression;

        internal SparqlCastBuilder(SparqlExpression castedExpression)
        {
            _castedExpression = castedExpression;
        }

        /// <summary>
        /// Creates a cast to xsd:integer
        /// </summary>
        public NumericExpression<int> AsInteger()
        {
            return new NumericExpression<int>(new IntegerCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:double
        /// </summary>
        public NumericExpression<double> AsDouble()
        {
            return new NumericExpression<double>(new DoubleCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:decimal
        /// </summary>
        public NumericExpression<decimal> AsDecimal()
        {
            return new NumericExpression<decimal>(new DecimalCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:dateTime
        /// </summary>
        public LiteralExpression AsDateTime()
        {
            return new LiteralExpression(new DateTimeCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:float
        /// </summary>
        public NumericExpression<float> AsFloat()
        {
            return new NumericExpression<float>(new FloatCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:boolean
        /// </summary>
        public BooleanExpression AsBoolean()
        {
            return new BooleanExpression(new BooleanCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:string
        /// </summary>
        public LiteralExpression AsString()
        {
            return new LiteralExpression(new StringCast(_castedExpression.Expression));
        }
    }
}