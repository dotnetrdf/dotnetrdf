using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
    /// <summary>
    /// Represents the SPARQL SHA256() Function
    /// </summary>
    public class Sha256HashFunction
        : Leviathan.Hash.Sha256HashFunction
    {
        /// <summary>
        /// Creates a new SHA256() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha256HashFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha256;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha256 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha256HashFunction(transformer.Transform(this._expr));
        }
    }
}
