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

namespace VDS.RDF
{
    /// <summary>
    /// Possible Triple Index types
    /// </summary>
    /// <remarks>
    /// <para>
    /// Index types are given Integer values with the lowest being the least useful index and the highest being most useful index.  Non-Index based Patterns are given arbitrary high values since these will typically never be used as these items are usually inserted into a Graph Pattern after the ordering step
    /// </para>
    /// <para>
    /// When used to sort Patterns as part of query optimisation the patterns are partially ordered on the usefullness of their index since more useful indexes are considered more likely to return fewer results which will help restrict the query space earlier in the execution process.
    /// </para>
    /// </remarks>
    public enum TripleIndexType : int
    {
        /// <summary>
        /// No Index should be used as the Pattern does not use Variables
        /// </summary>
        NoVariables = -2,
        /// <summary>
        /// No Index should be used as the Pattern is three Variables
        /// </summary>
        None = -1,
        /// <summary>
        /// Subject Index should be used
        /// </summary>
        Subject = 2,
        /// <summary>
        /// Predicate Index should be used
        /// </summary>
        Predicate = 1,
        /// <summary>
        /// Object Index should be used
        /// </summary>
        Object = 0,
        /// <summary>
        /// Subject-Predicate Index should be used
        /// </summary>
        SubjectPredicate = 5,
        /// <summary>
        /// Predicate-Object Index should be used
        /// </summary>
        PredicateObject = 3,
        /// <summary>
        /// Subject-Object Index should be used
        /// </summary>
        SubjectObject = 4,
        /// <summary>
        /// The Pattern is actually a FILTER
        /// </summary>
        SpecialFilter = 10,
        /// <summary>
        /// The Pattern is actually a LET/BIND assignment
        /// </summary>
        SpecialAssignment = 11,
        /// <summary>
        /// The Pattern is actually a Sub-query
        /// </summary>
        SpecialSubQuery = 12,
        /// <summary>
        /// The Pattern is actually a Property Path
        /// </summary>
        SpecialPropertyPath = 9
    }

    /// <summary>
    /// A Comparer which sorts based on Triple Index Type
    /// </summary>
    class TripleIndexSorter : IComparer<TripleIndexType>
    {
        /// <summary>
        /// Compares two Triple Index types to see which is greater
        /// </summary>
        /// <param name="x">First Index type</param>
        /// <param name="y">Second Index type</param>
        /// <returns></returns>
        /// <remarks>
        /// Implemented by converting to Integers and then using the Integer comparison function
        /// </remarks>
        public int Compare(TripleIndexType x, TripleIndexType y)
        {
            int a, b;
            a = (int)x;
            b = (int)y;

            return a.CompareTo(b);
        }
    }
}