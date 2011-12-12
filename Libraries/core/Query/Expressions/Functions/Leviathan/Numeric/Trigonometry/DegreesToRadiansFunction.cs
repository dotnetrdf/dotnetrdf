using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Represents the Leviathan lfn:degrees-to-radians() function
    /// </summary>
    public class DegreesToRadiansFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Degrees to Radians Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public DegreesToRadiansFunction(ISparqlExpression expr)
            : base(expr) { }


        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

            return new DoubleNode(null, Math.PI * (temp.AsDouble() / 180d));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.DegreesToRadians + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.DegreesToRadians;
            }
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
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DegreesToRadiansFunction(transformer.Transform(this._expr));
        }
    }
}
