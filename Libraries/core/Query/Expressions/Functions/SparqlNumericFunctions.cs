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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
