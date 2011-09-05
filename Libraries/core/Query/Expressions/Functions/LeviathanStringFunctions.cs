/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using HashLib.Crypto;

namespace VDS.RDF.Query.Expressions.Functions
{

#if !SILVERLIGHT

    /// <summary>
    /// Represents the Leviathan lfn:md5hash() function
    /// </summary>
    public class LeviathanMD5HashFunction 
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new Leviathan MD5 Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanMD5HashFunction(ISparqlExpression expr)
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
            return new LeviathanMD5HashFunction(transformer.Transform(this._expr));
        }
    }
#else
    public class LeviathanMD5HashFunction 
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

    /// <summary>
    /// Represents the Leviathan lfn:sha256hash() function
    /// </summary>
    public class LeviathanSha256HashFunction 
        : BaseHashFunction
    {
        /// <summary>
        /// Creates a new Leviathan SHA 256 Hash function
        /// </summary>
        /// <param name="expr">Expression</param>
        public LeviathanSha256HashFunction(ISparqlExpression expr)
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
            return new LeviathanSha256HashFunction(transformer.Transform(this._expr));
        }
    }
}
