using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:pythagoras() function
    /// </summary>
    public class PythagoreanDistanceFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Pythagorean Distance Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public PythagoreanDistanceFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode x = this._leftExpr.Evaluate(context, bindingID);
            if (x == null) throw new RdfQueryException("Cannot calculate distance of a null");
            IValuedNode y = this._rightExpr.Evaluate(context, bindingID);
            if (y == null) throw new RdfQueryException("Cannot calculate distance of a null");

            if (x.NumericType == SparqlNumericType.NaN || y.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot calculate distance when one/both arguments are non-numeric");

            return new DoubleNode(null, Math.Sqrt(Math.Pow(x.AsDouble(), 2) + Math.Pow(y.AsDouble(), 2)));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras;
            }
        }

        /// <summary>
        /// Gets the type of the expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new PythagoreanDistanceFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
