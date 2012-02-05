using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:factorial() function
    /// </summary>
    public class FactorialFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Factorial Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public FactorialFunction(ISparqlExpression expr)
            : base(expr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot evaluate factorial of a null");
            long l = temp.AsInteger();

            if (l == 0) return new LongNode(null, 0);
            long fac = 1;
            if (l > 0)
            {
                for (long i = l; i > 1; i--)
                {
                    fac = fac * i;
                }
            }
            else
            {
                for (long i = l; i < -1; i++)
                {
                    fac = fac * i;
                }
            }
            return new LongNode(null, fac);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Factorial + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Factorial;
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
            return new FactorialFunction(transformer.Transform(this._expr));
        }
    }
}
