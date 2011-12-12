using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
    /// <summary>
    /// Represents the SPARQL SHA1() Function
    /// </summary>
    public class Sha1HashFunction
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new SHA1() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha1HashFunction(ISparqlExpression expr)
            : base(expr, new SHA1Managed()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha1;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha1 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha1HashFunction(transformer.Transform(this._expr));
        }
    }
}
