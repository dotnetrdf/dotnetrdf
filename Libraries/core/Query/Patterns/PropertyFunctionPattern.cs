/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Patterns
{
    public class PropertyFunctionPattern
        : BaseTriplePattern, IPropertyFunctionPattern, IComparable<PropertyFunctionPattern>
    {
        private List<ITriplePattern> _patterns;
        private List<PatternItem> _lhsArgs, _rhsArgs;
        private ISparqlPropertyFunction _function;

        public PropertyFunctionPattern(IEnumerable<ITriplePattern> origPatterns, IEnumerable<PatternItem> lhsArgs, IEnumerable<PatternItem> rhsArgs, ISparqlPropertyFunction propertyFunction)
        {
            this._patterns = origPatterns.ToList();
            this._lhsArgs = lhsArgs.ToList();
            this._rhsArgs = rhsArgs.ToList();
            this._function = propertyFunction;
        }

        public override TriplePatternType PatternType
        {
            get
            {
                return TriplePatternType.PropertyFunction;
            }
        }

        public IEnumerable<PatternItem> LhsArgs
        {
            get
            {
                return this._lhsArgs;
            }
        }

        public IEnumerable<PatternItem> RhsArgs
        {
            get
            {
                return this._rhsArgs;
            }
        }

        public IEnumerable<ITriplePattern> OriginalPatterns
        {
            get
            {
                return this._patterns;
            }
        }

        public ISparqlPropertyFunction PropertyFunction
        {
            get
            {
                return this._function;
            }
        }

        public override void Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        public override bool HasNoBlankVariables
        {
            get 
            { 
                throw new NotImplementedException();
            }
        }

        public int CompareTo(PropertyFunctionPattern other)
        {
            return this.CompareTo((IPropertyFunctionPattern)other);
        }

        public int CompareTo(IPropertyFunctionPattern other)
        {
            return base.CompareTo(other);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
