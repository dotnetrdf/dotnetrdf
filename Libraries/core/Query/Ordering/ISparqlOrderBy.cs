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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering
{
    /// <summary>
    /// Interface for classes that represent SPARQL ORDER BY clauses
    /// </summary>
    /// <remarks>A SPARQL Order By clause provides a list of orderings, when parsed into the dotNetRDF model this is represented as a single <see cref="ISparqlOrderBy">ISparqlOrderBy</see> for the first term in the clause chained to <see cref="ISparqlOrderBy">ISparqlOrderBy</see>'s for each subsequent term via the <see cref="ISparqlOrderBy.Child">Child</see> property.</remarks>
    public interface ISparqlOrderBy : IComparer<ISet>
    {
        /// <summary>
        /// Gets/Sets the Child Ordering that applies if the two Objects are considered equal
        /// </summary>
        ISparqlOrderBy Child
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the Evaluation Context for the Order By
        /// </summary>
        SparqlEvaluationContext Context
        {
            set;
        }

        /// <summary>
        /// Sets whether the Ordering is Descending
        /// </summary>
        bool Descending
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the Ordering is simple (i.e. applies on variables only)
        /// </summary>
        bool IsSimple
        {
            get;
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used to do the Ordering
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern
        /// </summary>
        /// <param name="pattern">Triple Pattern</param>
        /// <returns></returns>
        IComparer<Triple> GetComparer(TriplePattern pattern);
    }
}
