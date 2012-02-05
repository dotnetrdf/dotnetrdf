using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:ln() function
    /// </summary>
    public class LeviathanNaturalLogFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Natural Logarithm Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanNaturalLogFunction(ISparqlExpression expr)
            : base(expr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot square root a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                case SparqlNumericType.Decimal:
                case SparqlNumericType.Float:
                case SparqlNumericType.Double:
                    return new DoubleNode(null, Math.Log(temp.AsDouble(), Math.E));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ln + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of this expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Ln;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanNaturalLogFunction(transformer.Transform(this._expr));
        }
    }
}
