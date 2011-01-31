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
    /// Abstract Base Class for classes representing Sparql GROUP BY clauses
    /// </summary>
    public abstract class BaseGroupBy : ISparqlGroupBy
    {
        /// <summary>
        /// Child Grouping
        /// </summary>
        protected ISparqlGroupBy _child = null;

        /// <summary>
        /// Gets/Sets the Child GROUP BY Clause
        /// </summary>
        public ISparqlGroupBy Child
        {
            get
            {
                return this._child;
            }
            set
            {
                this._child = value;
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
        /// Gets the Fixed Variables involved in this Group By
        /// </summary>
        public abstract IEnumerable<String> Variables
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
    }

    /// <summary>
    /// Represents a Grouping on a given Variable
    /// </summary>
    public class GroupByVariable : BaseGroupBy
    {
        private String _name;

        /// <summary>
        /// Creates a new Group By which groups by a given Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        public GroupByVariable(String name)
        {
            this._name = name;
        }

        /// <summary>
        /// Applies a Grouping on a given Variable to the Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override List<BindingGroup> Apply(SparqlEvaluationContext context)
        {
            Dictionary<INode, BindingGroup> groups = new Dictionary<INode, BindingGroup>();

            foreach (int id in context.Binder.BindingIDs)
            {
                INode value = context.Binder.Value(this._name, id);

                if (!groups.ContainsKey(value))
                {
                    groups.Add(value, new BindingGroup());
                }

                groups[value].Add(id);
            }

            if (this._child == null)
            {
                return (from g in groups.Values
                        select g).ToList();
            }
            else
            {
                List<BindingGroup> parentGroups = (from g in groups.Values select g).ToList();
                return this._child.Apply(context, parentGroups);
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

                foreach (int id in group.BindingIDs)
                {
                    INode value = context.Binder.Value(this._name, id);

                    if (!subgroups.ContainsKey(value))
                    {
                        subgroups.Add(value, new BindingGroup());
                    }

                    subgroups[value].Add(id);
                }

                foreach (BindingGroup g in subgroups.Values)
                {
                    outgroups.Add(g);
                }
            }

            if (this._child == null)
            {
                return outgroups;
            }
            else
            {
                return this._child.Apply(context, outgroups);
            }
        }

        /// <summary>
        /// Gets the Variables used in the GROUP BY
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                if (this._child == null)
                {
                    return this._name.AsEnumerable<String>();
                }
                else
                {
                    return this._child.Variables.Concat(this._name.AsEnumerable<String>());
                }
            }
        }

        /// <summary>
        /// Gets the Variable Expression Term used by this GROUP BY
        /// </summary>
        public override ISparqlExpression Expression
        {
            get 
            {
                return new VariableExpressionTerm(this._name); 
            }
        }

        /// <summary>
        /// Gets the String representation of the GROUP BY
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._child == null)
            {
                return "?" + this._name;
            }
            else
            {
                return "?" + this._name + " " + this._child.ToString();
            }
        }
    }

    /// <summary>
    /// Represents a Grouping on a given Expression
    /// </summary>
    public class GroupByExpression : BaseGroupBy
    {
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Group By which groups by a given Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        public GroupByExpression(ISparqlExpression expr)
        {
            this._expr = expr;
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
                    INode value = this._expr.Value(context, id);

                    if (value != null)
                    {
                        if (!groups.ContainsKey(value))
                        {
                            groups.Add(value, new BindingGroup());
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

            List<BindingGroup> parentGroups = (from g in groups.Values select g).ToList();
            if (error.BindingIDs.Any()) parentGroups.Add(error);
            if (nulls.BindingIDs.Any()) parentGroups.Add(nulls);

            if (this._child != null)
            {
                return this._child.Apply(context, parentGroups);
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
                        INode value = this._expr.Value(context, id);

                        if (value != null)
                        {
                            if (!subgroups.ContainsKey(value))
                            {
                                subgroups.Add(value, new BindingGroup());
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

                foreach (BindingGroup g in subgroups.Values)
                {
                    outgroups.Add(g);
                }
                if (error.BindingIDs.Any()) outgroups.Add(error);
                if (nulls.BindingIDs.Any()) outgroups.Add(nulls);
            }

            if (this._child == null)
            {
                return outgroups;
            }
            else
            {
                return this._child.Apply(context, outgroups);
            }
        }

        /// <summary>
        /// Gets the Fixed Variables used in the Grouping
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (this._child == null)
                {
                    if (this._expr is VariableExpressionTerm)
                    {
                        return this._expr.Variables;
                    }
                    else
                    {
                        return Enumerable.Empty<String>();
                    }
                }
                else
                {
                    if (this._expr is VariableExpressionTerm)
                    {
                        return this._child.Variables.Concat(this._expr.Variables);
                    }
                    else
                    {
                        return this._child.Variables;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Expression used to GROUP BY
        /// </summary>
        public override ISparqlExpression Expression
        {
            get 
            {
                return this._expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the GROUP BY
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._child == null)
            {
                return this._expr.ToString();
            }
            else
            {
                return this._expr.ToString() + " " + this._child.ToString();
            }
        }
    }
}