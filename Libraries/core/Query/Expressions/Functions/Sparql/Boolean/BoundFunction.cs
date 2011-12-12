using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{

    /// <summary>
    /// Class representing the SPARQL BOUND() function
    /// </summary>
    public class BoundFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Bound() function expression
        /// </summary>
        /// <param name="varExpr">Variable Expression</param>
        public BoundFunction(VariableTerm varExpr)
            : base(varExpr) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return new BooleanNode(null, this._expr.Evaluate(context, bindingID) != null);
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "BOUND(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
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
                return SparqlSpecsHelper.SparqlKeywordBound;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BoundFunction((VariableTerm)transformer.Transform(this._expr));
        }
    }
}
