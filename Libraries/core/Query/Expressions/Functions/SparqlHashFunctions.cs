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
using System.Security.Cryptography;
using System.Text;
using HashLib.Crypto;

namespace VDS.RDF.Query.Expressions.Functions
{

#if !SILVERLIGHT

    /// <summary>
    /// Represents the SPARQL MD5() Function
    /// </summary>
    public class MD5HashFunction 
        : LeviathanMD5HashFunction
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

    /// <summary>
    /// Represents the SPARQL SHA256() Function
    /// </summary>
    public class Sha256HashFunction 
        : LeviathanSha256HashFunction
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

#if !SILVERLIGHT

    /// <summary>
    /// Represents the SPARQL SHA384() Function
    /// </summary>
    public class Sha384HashFunction : BaseHashFunction
    {
        /// <summary>
        /// Creates a new SHA384() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha384HashFunction(ISparqlExpression expr)
            : base(expr, new SHA384Managed()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha384; 
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha384 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha384HashFunction(transformer.Transform(this._expr));
        }
    }

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
    /// Represents the SPARQL SHA384() Function
    /// </summary>
    public class Sha384HashFunction 
        : BaseHashLibFunction
    {
        /// <summary>
        /// Creates a new SHA384() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public Sha384HashFunction(ISparqlExpression expr)
            : base(expr, new SHA384()) { }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha384; 
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha384 + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new Sha384HashFunction(transformer.Transform(this._expr));
        }
    }

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
