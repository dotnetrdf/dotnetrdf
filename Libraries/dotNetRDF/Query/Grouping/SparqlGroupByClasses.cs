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
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Grouping
{
    /// <summary>
    /// Abstract Base Class for classes representing Sparql GROUP BY clauses
    /// </summary>
    public abstract class BaseGroupBy
        : ISparqlGroupBy
    {
        /// <summary>
        /// Child Grouping
        /// </summary>
        protected ISparqlGroupBy _child = null;

        private String _assignVariable;

        /// <summary>
        /// Gets/Sets the Child GROUP BY Clause
        /// </summary>
        public ISparqlGroupBy Child
        {
            get
            {
                return _child;
            }
            set
            {
                _child = value;
            }
        }

        /// <summary>
        /// Applies the Grouping to the Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public abstract List<BindingGroup> Apply(SparqlEvaluationContext context);

        /// <summary>
        /// Applies the Grouping to the Binder subdividing Groups from a previous Grouping
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="groups">Groups to subdivide</param>
        /// <returns></returns>
        public abstract List<BindingGroup> Apply(SparqlEvaluationContext context, List<BindingGroup> groups);

        /// <summary>
        /// Gets the Variables involved in this Group By
        /// </summary>
        public abstract IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables
        /// </summary>
        public abstract IEnumerable<String> ProjectableVariables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used to GROUP BY
        /// </summary>
        public abstract ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Variable that the grouped upon value should be assigned to
        /// </summary>
        public String AssignVariable
        {
            get
            {
                return _assignVariable;
            }
            set
            {
                _assignVariable = value;
            }
        }
    }

    /// <summary>
    /// Represents a Grouping on a given Variable
    /// </summary>
    public class GroupByVariable
        : BaseGroupBy
    {
        private String _name;

        /// <summary>
        /// Creates a new Group By which groups by a given Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        public GroupByVariable(String name)
        {
            _name = name;
        }

        /// <summary>
        /// Creates a new Group By which groups by a given Variable and assigns to another variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="assignVariable">Assign Variable</param>
        public GroupByVariable(String name, String assignVariable)
            : this(name)
        {
            AssignVariable = assignVariable;
        }

        /// <summary>
        /// Applies a Grouping on a given Variable to the Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override List<BindingGroup> Apply(SparqlEvaluationContext context)
        {
            Dictionary<INode, BindingGroup> groups = new Dictionary<INode, BindingGroup>();
            BindingGroup nulls = new BindingGroup();

            foreach (int id in context.Binder.BindingIDs)
            {
                INode value = context.Binder.Value(_name, id);

                if (value != null)
                {
                    if (!groups.ContainsKey(value))
                    {
                        groups.Add(value, new BindingGroup());
                        if (AssignVariable != null)
                        {
                            groups[value].AddAssignment(AssignVariable, value);
                        }
                    }

                    groups[value].Add(id);
                }
                else
                {
                    nulls.Add(id);
                }
            }

            List<BindingGroup> outGroups = (from g in groups.Values select g).ToList();
            if (nulls.Any())
            {
                outGroups.Add(nulls);
                if (AssignVariable != null) nulls.AddAssignment(AssignVariable, null);
            }
            if (_child == null)
            {
                return outGroups;
            }
            else
            {
                return _child.Apply(context, outGroups);
            }
        }

        /// <summary>
        /// Applies a Grouping on a given Variable to the Binder Groups from a previous Grouping
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="groups">Binder Group to subgroup</param>
        /// <returns></returns>
        public override List<BindingGroup> Apply(SparqlEvaluationContext context, List<BindingGroup> groups)
        {
            List<BindingGroup> outgroups = new List<BindingGroup>();

            foreach (BindingGroup group in groups)
            {
                Dictionary<INode, BindingGroup> subgroups = new Dictionary<INode, BindingGroup>();
                BindingGroup nulls = new BindingGroup(group);

                foreach (int id in group.BindingIDs)
                {
                    INode value = context.Binder.Value(_name, id);

                    if (value != null)
                    {
                        if (!subgroups.ContainsKey(value))
                        {
                            subgroups.Add(value, new BindingGroup(group));
                            if (AssignVariable != null)
                            {
                                subgroups[value].AddAssignment(AssignVariable, value);
                            }
                        }

                        subgroups[value].Add(id);
                    }
                    else
                    {
                        nulls.Add(id);
                    }
                }

                foreach (BindingGroup g in subgroups.Values)
                {
                    outgroups.Add(g);
                }
                if (nulls.Any())
                {
                    outgroups.Add(nulls);
                    if (AssignVariable != null) nulls.AddAssignment(AssignVariable, null);
                }
            }

            if (_child == null)
            {
                return outgroups;
            }
            else
            {
                return _child.Apply(context, outgroups);
            }
        }

        /// <summary>
        /// Gets the Variables used in the GROUP BY
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                if (_child == null)
                {
                    return _name.AsEnumerable<String>();
                }
                else
                {
                    return _child.Variables.Concat(_name.AsEnumerable<String>());
                }
            }
        }

        /// <summary>
        /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables
        /// </summary>
        public override IEnumerable<String> ProjectableVariables
        {
            get
            {
                List<String> vars = new List<string>();
                if (AssignVariable != null) vars.Add(AssignVariable);
                vars.Add(_name);

                if (_child != null) vars.AddRange(_child.ProjectableVariables);
                return vars.Distinct();
            }
        }

        /// <summary>
        /// Gets the Variable Expression Term used by this GROUP BY
        /// </summary>
        public override ISparqlExpression Expression
        {
            get 
            {
                return new VariableTerm(_name); 
            }
        }

        /// <summary>
        /// Gets the String representation of the GROUP BY
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (AssignVariable != null && !AssignVariable.Equals(_name))
            {
                output.Append('(');
            }
            output.Append('?');
            output.Append(_name);
            if (AssignVariable != null && !AssignVariable.Equals(_name))
            {
                output.Append(" AS ?");
                output.Append(AssignVariable);
                output.Append(')');
            }

            if (_child != null)
            {
                output.Append(' ');
                output.Append(_child.ToString());
            }

            return output.ToString();
        }
    }

    /// <summary>
    /// Represents a Grouping on a given Expression
    /// </summary>
    public class GroupByExpression
        : BaseGroupBy
    {
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Group By which groups by a given Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        public GroupByExpression(ISparqlExpression expr)
        {
            _expr = expr;
        }

        /// <summary>
        /// Applies a Grouping on a given Expression to the Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override List<BindingGroup> Apply(SparqlEvaluationContext context)
        {
            Dictionary<INode, BindingGroup> groups = new Dictionary<INode, BindingGroup>();
            BindingGroup error = new BindingGroup();
            BindingGroup nulls = new BindingGroup();

            foreach (int id in context.Binder.BindingIDs)
            {
                try
                {
                    INode value = _expr.Evaluate(context, id);

                    if (value != null)
                    {
                        if (!groups.ContainsKey(value))
                        {
                            groups.Add(value, new BindingGroup());
                            if (AssignVariable != null)
                            {
                                groups[value].AddAssignment(AssignVariable, value);
                            }
                        }

                        groups[value].Add(id);
                    }
                    else
                    {
                        nulls.Add(id);
                    }
                }
                catch (RdfQueryException)
                {
                    error.Add(id);
                }
            }

            // Build the List of Groups
            // Null and Error Group are included if required
            List<BindingGroup> parentGroups = (from g in groups.Values select g).ToList();
            if (error.BindingIDs.Any())
            {
                parentGroups.Add(error);
                if (AssignVariable != null) error.AddAssignment(AssignVariable, null);
            }
            if (nulls.BindingIDs.Any())
            {
                parentGroups.Add(nulls);
                if (AssignVariable != null) nulls.AddAssignment(AssignVariable, null);
            }

            if (_child != null)
            {
                return _child.Apply(context, parentGroups);
            }
            else
            {
                return parentGroups;
            }
        }

        /// <summary>
        /// Applies a Grouping on a given Variable to the Binder Groups from a previous Grouping
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="groups">Binder Group to subgroup</param>
        /// <returns></returns>
        public override List<BindingGroup> Apply(SparqlEvaluationContext context, List<BindingGroup> groups)
        {
            List<BindingGroup> outgroups = new List<BindingGroup>();

            foreach (BindingGroup group in groups)
            {
                Dictionary<INode, BindingGroup> subgroups = new Dictionary<INode, BindingGroup>();
                BindingGroup error = new BindingGroup();
                BindingGroup nulls = new BindingGroup();

                foreach (int id in group.BindingIDs)
                {
                    try
                    {
                        INode value = _expr.Evaluate(context, id);

                        if (value != null)
                        {
                            if (!subgroups.ContainsKey(value))
                            {
                                subgroups.Add(value, new BindingGroup(group));
                                if (AssignVariable != null)
                                {
                                    subgroups[value].AddAssignment(AssignVariable, value);
                                }
                            }

                            subgroups[value].Add(id);
                        }
                        else
                        {
                            nulls.Add(id);
                        }
                    }
                    catch (RdfQueryException)
                    {
                        error.Add(id);
                    }
                }

                // Build the List of Groups
                // Null and Error Group are included if required
                foreach (BindingGroup g in subgroups.Values)
                {
                    outgroups.Add(g);
                }
                if (error.BindingIDs.Any())
                {
                    outgroups.Add(error);
                    if (AssignVariable != null) error.AddAssignment(AssignVariable, null);
                    error = new BindingGroup();
                }
                if (nulls.BindingIDs.Any())
                {
                    outgroups.Add(nulls);
                    if (AssignVariable != null) nulls.AddAssignment(AssignVariable, null);
                    nulls = new BindingGroup();
                }
            }

            if (_child == null)
            {
                return outgroups;
            }
            else
            {
                return _child.Apply(context, outgroups);
            }
        }

        /// <summary>
        /// Gets the Fixed Variables used in the Grouping
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (_child == null)
                {
                    return _expr.Variables;
                }
                else
                {
                    return _expr.Variables.Concat(_child.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables
        /// </summary>
        public override IEnumerable<String> ProjectableVariables
        {
            get
            {
                List<String> vars = new List<string>();
                if (AssignVariable != null) vars.Add(AssignVariable);
                if (_expr is VariableTerm)
                {
                    vars.AddRange(_expr.Variables);
                }

                if (_child != null) vars.AddRange(_child.ProjectableVariables);
                return vars.Distinct();
            }
        }

        /// <summary>
        /// Gets the Expression used to GROUP BY
        /// </summary>
        public override ISparqlExpression Expression
        {
            get 
            {
                return _expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the GROUP BY
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('(');
            output.Append(_expr.ToString());
            if (AssignVariable != null)
            {
                output.Append(" AS ?");
                output.Append(AssignVariable);
            }
            output.Append(')');

            if (_child != null)
            {
                output.Append(' ');
                output.Append(_child.ToString());
            }

            return output.ToString();
        }
    }
}