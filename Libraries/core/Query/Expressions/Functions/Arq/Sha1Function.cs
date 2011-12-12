using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:sha1sum() function
    /// </summary>
    public class Sha1Function 
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new ARQ SHA1 Sum function
        /// </summary>
        /// <param name="expr">Expression</param>
        public Sha1Function(ISparqlExpression expr)
            : base(expr, new SHA1Managed()) { }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Sha1Sum + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Sha1Sum;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha1Function(transformer.Transform(this._expr));
        }
    }
}
