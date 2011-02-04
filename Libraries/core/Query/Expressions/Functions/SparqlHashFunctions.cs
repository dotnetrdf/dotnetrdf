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

#if !SILVERLIGHT && !COMPACT

    public class MD5HashFunction : LeviathanMD5HashFunction
    {
        public MD5HashFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMD5;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMD5 + "(" + this._expr.ToString() + ")";
        }
    }

#else

    public class MD5HashFunction : BaseHashLibFunction
    {
        public MD5HashFunction(ISparqlExpression expr)
            : base(expr, new MD5()) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMD5;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMD5 + "(" + this._expr.ToString() + ")";
        }
    }

#endif

    public class Sha1HashFunction : BaseHashFunction
    {
        public Sha1HashFunction(ISparqlExpression expr)
            : base(expr, new SHA1Managed()) { }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha1;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha1 + "(" + this._expr.ToString() + ")";
        }
    }

    public class Sha224HashFunction : BaseHashLibFunction
    {
        public Sha224HashFunction(ISparqlExpression expr)
            : base(expr, new SHA224()) { }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha224;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha224 + "(" + this._expr.ToString() + ")";
        }
    }

    public class Sha256HashFunction : LeviathanSha256HashFunction
    {
        public Sha256HashFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha256;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha256 + "(" + this._expr.ToString() + ")";
        }
    }

#if !SILVERLIGHT && !COMPACT

    public class Sha384HashFunction : BaseHashFunction
    {
        public Sha384HashFunction(ISparqlExpression expr)
            : base(expr, new SHA384Managed()) { }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha384; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha384 + "(" + this._expr.ToString() + ")";
        }
    }

    public class Sha512HashFunction : BaseHashFunction
    {
        public Sha512HashFunction(ISparqlExpression expr)
            : base(expr, new SHA512Managed()) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha512;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha512 + "(" + this._expr.ToString() + ")";
        }
    }

#else

    public class Sha384HashFunction : BaseHashLibFunction
    {
        public Sha384HashFunction(ISparqlExpression expr)
            : base(expr, new SHA384()) { }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordSha384; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha384 + "(" + this._expr.ToString() + ")";
        }
    }

    public class Sha512HashFunction : BaseHashLibFunction
    {
        public Sha512HashFunction(ISparqlExpression expr)
            : base(expr, new SHA512()) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSha512;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSha512 + "(" + this._expr.ToString() + ")";
        }
    }

#endif

}
