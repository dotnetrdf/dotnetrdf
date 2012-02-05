using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Represents the Leviathan lfn:radians-to-degrees() function
    /// </summary>
    public class RadiansToDegreesFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Radians to Degrees Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public RadiansToDegreesFunction(ISparqlExpression expr)
            : base(expr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

            return new DoubleNode(null, temp.AsDouble() * (180d / Math.PI));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.RadiansToDegrees + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.RadiansToDegrees;
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
            return new RadiansToDegreesFunction(transformer.Transform(this._expr));
        }
    }
}
