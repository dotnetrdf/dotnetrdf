using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Represents the SPARQL ROUND() Function
    /// </summary>
    public class RoundFunction
        : XPath.Numeric.RoundFunction
    {
        /// <summary>
        /// Creates a new SPARQL ROUND() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public RoundFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRound;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRound + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new RoundFunction(transformer.Transform(this._expr));
        }
    }
}
