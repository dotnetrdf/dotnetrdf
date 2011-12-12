using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Hash
{
    /// <summary>
    /// Represents the Leviathan lfn:sha256hash() function
    /// </summary>
    public class Sha256HashFunction
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new Leviathan SHA 256 Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        public Sha256HashFunction(ISparqlExpression expr)
            : base(expr, new SHA256Managed()) { }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Sha256Hash + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Sha256Hash;
            }
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
