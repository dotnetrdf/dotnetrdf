using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HashLib.Crypto;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
    /// <summary>
    /// Represents the SPARQL SHA224() Function
    /// </summary>
    public class Sha224HashFunction
        : BaseHashLibFunction
    {
        /// <summary>
        /// Creates a new SHA224() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha224HashFunction(ISparqlExpression expr)
            : base(expr, new SHA224()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha224;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha224 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha224HashFunction(transformer.Transform(this._expr));
        }
    }
}
