using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Conditional
{
    /// <summary>
    /// Class representing logical Not Expressions
    /// </summary>
    public class NotExpression
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Negation Expression
        /// </summary>
        /// <param name="expr">Expression to Negate</param>
        public NotExpression(ISparqlExpression expr) 
            : base(expr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return new BooleanNode(null, !this._expr.Evaluate(context, bindingID).AsSafeBoolean());
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "!" + this._expr.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.UnaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "!";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new NotExpression(transformer.Transform(this._expr));
        }
    }
}
