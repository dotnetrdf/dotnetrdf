using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:reciprocal() function
    /// </summary>
    public class ReciprocalFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Reciprocal Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public ReciprocalFunction(ISparqlExpression expr)
            : base(expr) { }


        public override IValuedNode  Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot evaluate reciprocal of a null");
            double d = temp.AsDouble();
            if (d == 0) throw new RdfQueryException("Cannot evaluate reciprocal of zero");

            return new DoubleNode(null, 1d / d);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Reciprocal + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Reciprocal;
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
            return new ReciprocalFunction(transformer.Transform(this._expr));
        }
    }
}
