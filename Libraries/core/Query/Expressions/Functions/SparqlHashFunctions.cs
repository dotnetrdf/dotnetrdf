using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

#endif

}
