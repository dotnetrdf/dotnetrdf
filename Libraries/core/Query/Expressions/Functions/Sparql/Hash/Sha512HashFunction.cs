using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HashLib;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{

#if !SILVERLIGHT

    /// <summary>
    /// Represents the SPARQL SHA512() Function
    /// </summary>
    public class Sha512HashFunction 
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new SHA512() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha512HashFunction(ISparqlExpression expr)
            : base(expr, new SHA512Managed()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha512;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha512 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha512HashFunction(transformer.Transform(this._expr));
        }
    }

#else



    /// <summary>
    /// Represents the SPARQL SHA512() Function
    /// </summary>
    public class Sha512HashFunction 
        : BaseHashLibFunction
    {
        /// <summary>
        /// Creates a new SHA512() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha512HashFunction(ISparqlExpression expr)
            : base(expr, new SHA512()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha512;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha512 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha512HashFunction(transformer.Transform(this._expr));
        }
    }

#endif
}
