using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Represents the Leviathan lfn:tan() or lfn:tan-1 function
    /// </summary>
    public class TangentFunction
        : BaseTrigonometricFunction
    {
        private bool _inverse = false;

        /// <summary>
        /// Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public TangentFunction(ISparqlExpression expr)
            : base(expr, Math.Tan) { }

        /// <summary>
        /// Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public TangentFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            this._inverse = inverse;
            if (this._inverse)
            {
                this._func = Math.Atan;
            }
            else
            {
                this._func = Math.Tan;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTanInv + ">(" + this._expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (this._inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTanInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new TangentFunction(transformer.Transform(this._expr), this._inverse);
        }
    }
}
