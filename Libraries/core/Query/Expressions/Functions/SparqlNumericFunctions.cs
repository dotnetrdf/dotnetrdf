using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions
{
    public class AbsFunction : XPathAbsoluteFunction
    {
        public AbsFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordAbs;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordAbs + "(" + this._expr.ToString() + ")";
        }
    }

    public class CeilFunction : XPathCeilingFunction
    {
        public CeilFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCeil;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordCeil + "(" + this._expr.ToString() + ")";
        }
    }

    public class FloorFunction : XPathFloorFunction
    {
        public FloorFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordFloor;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordFloor + "(" + this._expr.ToString() + ")";
        }
    }

    public class RoundFunction : XPathRoundFunction
    {
        public RoundFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRound;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRound + "(" + this._expr.ToString() + ")";
        }
    }
}
