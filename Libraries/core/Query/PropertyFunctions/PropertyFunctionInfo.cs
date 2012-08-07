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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Represents information about a property function
    /// </summary>
    public class PropertyFunctionInfo
    {
        private Uri _funcUri;
        private List<IMatchTriplePattern> _patterns = new List<IMatchTriplePattern>();
        private List<PatternItem> _subjArgs = new List<PatternItem>();
        private List<PatternItem> _objArgs = new List<PatternItem>();

        public PropertyFunctionInfo(Uri u)
        {
            this._funcUri = u;
        }

        public Uri FunctionUri
        {
            get
            {
                return this._funcUri;
            }
        }

        public List<IMatchTriplePattern> Patterns
        {
            get
            {
                return this._patterns;
            }
        }

        public List<PatternItem> SubjectArgs
        {
            get
            {
                return this._subjArgs;
            }
        }

        public List<PatternItem> ObjectArgs
        {
            get
            {
                return this._objArgs;
            }
        }
    }
}
