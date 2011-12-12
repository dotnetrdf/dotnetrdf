using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
#if !SILVERLIGHT

    /// <summary>
    /// Represents the SPARQL MD5() Function
    /// </summary>
    public class MD5HashFunction
        : Leviathan.Hash.MD5HashFunction
    {
        /// <summary>
        /// Creates a new MD5() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public MD5HashFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMD5;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMD5 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MD5HashFunction(transformer.Transform(this._expr));
        }
    }

#else

    /// <summary>
    /// Represents the SPARQL MD5() Function
    /// </summary>
    public class MD5HashFunction : BaseHashLibFunction
    {
        /// <summary>
        /// Creates a new MD5() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public MD5HashFunction(ISparqlExpression expr)
            : base(expr, new MD5()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMD5;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMD5 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MD5HashFunction(transformer.Transform(this._expr));
        }
    }

#endif
}
