using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:pow() function
    /// </summary>
    public class PowerFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Power Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public PowerFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode arg = this._leftExpr.Evaluate(context, bindingID);
            if (arg == null) throw new RdfQueryException("Cannot raise a null to a power");
            IValuedNode pow = this._rightExpr.Evaluate(context, bindingID);
            if (pow == null) throw new RdfQueryException("Cannot raise to a null power");

            if (arg.NumericType == SparqlNumericType.NaN || pow.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot raise to a power when one/both arguments are non-numeric");

            return new DoubleNode(null, Math.Pow(arg.AsDouble(), pow.AsDouble()));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Power + ">(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Power;
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
            return new PowerFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
