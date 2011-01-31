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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Grouping
{
    /// <summary>
    /// Interface for Classes that represent SPARQL GROUP BY clauses
    /// </summary>
    public interface ISparqlGroupBy
    {
        /// <summary>
        /// Applies the Grouping to a Result Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        List<BindingGroup> Apply(SparqlEvaluationContext context);

        /// <summary>
        /// Applies the Grouping to a Result Binder subdividing the Groups from the previous Group By clause into further Groups
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="groups">Groups</param>
        /// <returns></returns>
        List<BindingGroup> Apply(SparqlEvaluationContext context, List<BindingGroup> groups);

        /// <summary>
        /// Gets/Sets the child GROUP BY Clause
        /// </summary>
        ISparqlGroupBy Child
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Fixed Variables used in the GROUP BY
        /// </summary>
        /// <remarks>
        /// Should only return variables whose raw values are grouped upon not those which are used in expressions
        /// </remarks>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used to GROUP BY
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }
    }
}
