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
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Grouping
    /// </summary>
    public class GroupBy
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private ISparqlGroupBy _grouping;

        /// <summary>
        /// Creates a new Group By
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="grouping">Grouping to use</param>
        public GroupBy(ISparqlAlgebra pattern, ISparqlGroupBy grouping)
        {
            this._pattern = pattern;
            this._grouping = grouping;
        }

        /// <summary>
        /// Evaluates a Group By by generating a <see cref="GroupMultiset">GroupMultiset</see> from the Input Multiset
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset results = context.Evaluate(this._pattern);

            //Identity/Null yields
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                results = new Multiset();
            }
            GroupMultiset groupSet = new GroupMultiset(results);
            List<BindingGroup> groups;

            //Calculate Groups
            if (context.Query.GroupBy != null)
            {
                groups = context.Query.GroupBy.Apply(context);
            }
            else if (this._grouping != null)
            {
                groups = this._grouping.Apply(context);
            }
            else
            {
                groups = new List<BindingGroup>() { new BindingGroup(results.SetIDs) };
            }

            //Add Groups to the GroupMultiset
            HashSet<String> vars = new HashSet<String>();
            foreach (BindingGroup group in groups)
            {
                foreach (KeyValuePair<String, INode> assignment in group.Assignments)
                {
                    if (!vars.Contains(assignment.Key)) groupSet.AddVariable(assignment.Key);
                }
                groupSet.AddGroup(group);
            }

            context.OutputMultiset = groupSet;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the Grouping that is used
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null and has a GROUP BY clause then that is applied rather than the clause with which the GroupBy algebra is instantiated
        /// </remarks>
        public ISparqlGroupBy Grouping
        {
            get
            {
                return this._grouping;
            }
        }

        /// <summary>
        /// Gets the String representation of the 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GroupBy(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            q.GroupBy = this._grouping;
            return q;
        }

        /// <summary>
        /// Throws an exception since GroupBy() cannot be converted to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since GroupBy() cannot be converted to a GraphPattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("GroupBy() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new GroupBy(optimiser.Optimise(this._pattern), this._grouping);
        }
    }
}
