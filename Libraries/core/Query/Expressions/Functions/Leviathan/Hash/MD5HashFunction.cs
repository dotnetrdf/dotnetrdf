using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;
using HashLib;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Hash
{
#if !SILVERLIGHT

    /// <summary>
    /// Represents the Leviathan lfn:md5hash() function
    /// </summary>
    public class MD5HashFunction
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new Leviathan MD5 Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        public MD5HashFunction(ISparqlExpression expr)
            : base(expr, new MD5Cng()) { }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.MD5Hash + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.MD5Hash;
            }
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
    public class MD5HashFunction 
    : BaseHashLibFunction
    {
        /// <summary>
        /// Creates a new Leviathan MD5() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public LeviathanMD5HashFunction(ISparqlExpression expr)
            : base(expr, new MD5()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.MD5Hash;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.MD5Hash + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LeviathanMD5HashFunction(transformer.Transform(this._expr));
        }
    }
#endif
}
