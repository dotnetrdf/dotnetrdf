using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a SPARQL expression (variable, function, operator or term)
    /// </summary>
#pragma warning disable 660,661
    public abstract class SparqlExpression
#pragma warning restore 660,661
    {
        protected SparqlExpression(ISparqlExpression expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// The undelrying expression
        /// </summary>
        public ISparqlExpression Expression { get; set; }

        /// <summary>
        /// Creates a call to the IN function
        /// </summary>
        /// <param name="expressions">the list of SPARQL expressions</param>
        public BooleanExpression In(params SparqlExpression[] expressions)
        {
            var inFunction = new InFunction(Expression, expressions.Select(v => v.Expression));
            return new BooleanExpression(inFunction);
        }

#pragma warning disable 1591
        public static BooleanExpression operator ==(SparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.Expression));
        }

        public static BooleanExpression operator !=(SparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.Expression));
        }
#pragma warning restore 1591

        /// <summary>
        /// Creates a greater than operator usage
        /// </summary>
        protected static BooleanExpression Gt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanExpression(left, right));
        }

        /// <summary>
        /// Creates a less than operator usage
        /// </summary>
        protected static BooleanExpression Lt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanExpression(left, right));
        }

        /// <summary>
        /// Creates a greater than or equal operator usage
        /// </summary>
        protected static BooleanExpression Ge(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanOrEqualToExpression(left, right));
        }

        /// <summary>
        /// Creates a less than or equal operator usage
        /// </summary>
        protected static BooleanExpression Le(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanOrEqualToExpression(left, right));
        }
    }
}