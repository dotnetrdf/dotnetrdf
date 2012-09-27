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

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Possible Types of Triple Pattern
    /// </summary>
    public enum TriplePatternType
    {
        /// <summary>
        /// Simple pattern matching
        /// </summary>
        Match,
        /// <summary>
        /// FILTER application
        /// </summary>
        Filter,
        /// <summary>
        /// BIND assignment
        /// </summary>
        BindAssignment,
        /// <summary>
        /// LET assignment
        /// </summary>
        LetAssignment,
        /// <summary>
        /// Sub-query
        /// </summary>
        SubQuery,
        /// <summary>
        /// Property Path
        /// </summary>
        Path,
        /// <summary>
        /// Property Function
        /// </summary>
        PropertyFunction
    }

    /// <summary>
    /// Comparer for Triple Pattern Types
    /// </summary>
    public class TriplePatternTypeComparer
        : IComparer<TriplePatternType>
    {
        /// <summary>
        /// Compares two triple pattern types
        /// </summary>
        /// <param name="x">Pattern Type</param>
        /// <param name="y">Pattern Type</param>
        /// <returns></returns>
        public int Compare(TriplePatternType x, TriplePatternType y)
        {
            return ((int)x).CompareTo((int)y);
        }
    }
}
