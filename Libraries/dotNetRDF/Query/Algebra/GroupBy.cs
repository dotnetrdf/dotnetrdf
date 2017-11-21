/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
        private readonly ISparqlAlgebra _pattern;
        private readonly ISparqlGroupBy _grouping;
        private readonly List<SparqlVariable> _aggregates = new List<SparqlVariable>();

        /// <summary>
        /// Creates a new Group By
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="grouping">Grouping to use</param>
        /// <param name="aggregates">Aggregates to calculate</param>
        public GroupBy(ISparqlAlgebra pattern, ISparqlGroupBy grouping, IEnumerable<SparqlVariable> aggregates)
        {
            _pattern = pattern;
            _grouping = grouping;
            _aggregates.AddRange(aggregates.Where(var => var.IsAggregate));
        }

        /// <summary>
        /// Evaluates a Group By by generating a <see cref="GroupMultiset">GroupMultiset</see> from the Input Multiset
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset results = context.Evaluate(_pattern);
            context.InputMultiset = results;

            // Identity/Null yields an empty multiset
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                results = new Multiset();
            }
            GroupMultiset groupSet = new GroupMultiset(results);
            List<BindingGroup> groups;

            // Calculate Groups
            if (context.Query.GroupBy != null)
            {
                groups = context.Query.GroupBy.Apply(context);
            }
            else if (_grouping != null)
            {
                groups = _grouping.Apply(context);
            }
            else
            {
                groups = new List<BindingGroup>() { new BindingGroup(results.SetIDs) };
            }

            // Add Groups to the GroupMultiset
            HashSet<String> vars = new HashSet<String>();
            foreach (BindingGroup group in groups)
            {
                foreach (KeyValuePair<String, INode> assignment in group.Assignments)
                {
                    if (vars.Contains(assignment.Key)) continue;

                    groupSet.AddVariable(assignment.Key);
                    vars.Add(assignment.Key);
                }
                groupSet.AddGroup(group);
            }
            // If grouping produced no groups and there are aggregates present
            // then an implicit group is created
            if (groups.Count == 0 && _aggregates.Count > 0) groupSet.AddGroup(new BindingGroup());

            // Apply the aggregates
            context.InputMultiset = groupSet;
            context.Binder.SetGroupContext(true);
            foreach (SparqlVariable var in _aggregates)
            {
                if (!vars.Contains(var.Name))
                {
                    groupSet.AddVariable(var.Name);
                    vars.Add(var.Name);
                }

                foreach (ISet s in groupSet.Sets)
                {
                    try
                    {
                        INode value = var.Aggregate.Apply(context, groupSet.GroupSetIDs(s.ID));
                        s.Add(var.Name, value);
                    }
                    catch (RdfQueryException)
                    {
                        s.Add(var.Name, null);
                    }
                }
            }
            context.Binder.SetGroupContext(false);

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
                return _pattern.Variables.Concat(_aggregates.Select(v => v.Name)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those floating in the inner algebra plus aggregates
                return _pattern.FloatingVariables.Concat(_aggregates.Select(v => v.Name)).Distinct();
            }
            
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _pattern.FixedVariables; } }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return _pattern;
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
                return _grouping;
            }
        }

        /// <summary>
        /// Gets the Aggregates that will be applied
        /// </summary>
        public IEnumerable<SparqlVariable> Aggregates
        {
            get
            {
                return _aggregates;
            }
        }

        /// <summary>
        /// Gets the String representation of the 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GroupBy(" + _pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = _pattern.ToQuery();
            q.GroupBy = _grouping;
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
            return new GroupBy(optimiser.Optimise(_pattern), _grouping, _aggregates);
        }
    }
}
