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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Projection step of Query Evaluation
    /// </summary>
    public class Project 
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private List<SparqlVariable> _variables = new List<SparqlVariable>();

        /// <summary>
        /// Creates a new Projection
        /// </summary>
        /// <param name="pattern">Inner pattern</param>
        /// <param name="variables">Variables that should be Projected</param>
        public Project(ISparqlAlgebra pattern, IEnumerable<SparqlVariable> variables)
        {
            this._pattern = pattern;
            this._variables.AddRange(variables);
        }

        /// <summary>
        /// Applies the Projection to the results of Evaluating the Inner Pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);
            }
            catch (RdfQueryTimeoutException)
            {
                //If not partial results throw the error
                if (!context.Query.PartialResultsOnTimeout) throw;
            }

            IEnumerable<SparqlVariable> vars;
            if (context.Query != null)
            {
                vars = context.Query.Variables;
            }
            else
            {
                vars = this._variables;
            }

            //For Null and Identity Multisets this is just a simple selection
            if (context.InputMultiset is NullMultiset)
            {
                context.InputMultiset = new Multiset(vars.Select(v => v.Name));
                context.OutputMultiset = context.InputMultiset;
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                context.InputMultiset = new Multiset(vars.Select(v => v.Name));
                Set s = new Set();
                context.InputMultiset.Add(s);
                context.OutputMultiset = context.InputMultiset;
            }

            //If we have a Group Multiset then Projection is more complex
            GroupMultiset groupSet = null;
            if (context.InputMultiset is GroupMultiset)
            {
                groupSet = (GroupMultiset)context.InputMultiset;

                //Project all simple variables for the Groups here
                foreach (SparqlVariable v in vars.Where(v => v.IsResultVariable && !v.IsProjection && !v.IsAggregate))
                {
                    //Can only project a variable if it's used in the GROUP OR if it was assigned by a GROUP BY expression
                    if (context.Query != null)
                    {
                        if (!groupSet.ContainsVariable(v.Name) && !context.Query.GroupBy.Variables.Contains(v.Name))
                        {
                            throw new RdfQueryException("Cannot project the variable ?" + v.Name + " since this Query contains Grouping(s) but the given Variable is not in the GROUP BY - use the SAMPLE aggregate if you need to sample this Variable");
                        }
                    }

                    //Project the value for each variable
                    if (!groupSet.ContainsVariable(v.Name))
                    {
                        //Simple Variable Projection used in GROUP BY so grab first value as all should be same
                        //for the group
                        context.OutputMultiset.AddVariable(v.Name);
                        foreach (int id in groupSet.SetIDs)
                        {
                            INode value = groupSet.Contents[groupSet.GroupSetIDs(id).First()][v.Name];
                            context.OutputMultiset[id].Add(v.Name, value);
                        }
                    }
                }
            }
            else if (context.Query.IsAggregate)
            {
                context.OutputMultiset = new Multiset();
            }

            //Project the rest of the Variables
            Set aggSet = new Set();
            foreach (SparqlVariable v in vars.Where(v => v.IsResultVariable))
            {
                if (groupSet == null)
                {
                    context.InputMultiset.AddVariable(v.Name);
                }
                else
                {
                    context.OutputMultiset.AddVariable(v.Name);
                }

                if (v.IsAggregate)
                {
                    //Compute the Aggregate
                    if (groupSet != null)
                    {
                        context.InputMultiset = groupSet.Contents;
                        foreach (int id in groupSet.SetIDs)
                        {
                            INode aggValue = v.Aggregate.Apply(context, groupSet.GroupSetIDs(id));
                            context.OutputMultiset[id].Add(v.Name, aggValue);
                        }
                        context.InputMultiset = groupSet;
                    }
                    else
                    {
                        INode aggValue = v.Aggregate.Apply(context, context.InputMultiset.SetIDs);
                        aggSet.Add(v.Name, aggValue);
                    }
                }
                else if (v.IsProjection)
                {
                    if (context.Query != null && context.Query.IsAggregate && context.Query.GroupBy == null)
                    {
                        throw new RdfQueryException("Cannot project an expression since this Query contains Aggregates and no GROUP BY");
                    }
                    else
                    {
                        //Compute the Value of the Projection Expression for each Set
                        foreach (int id in context.InputMultiset.SetIDs)
                        {
                            ISet s = context.InputMultiset[id];
                            try
                            {
                                INode temp = v.Projection.Value(context, id);
                                s.Add(v.Name, temp);
                            }
                            catch (RdfQueryException)
                            {
                                s.Add(v.Name, null);
                            }
                        }
                    }
                }
                else
                {
                    if (context.Query != null && context.Query.IsAggregate && context.Query.GroupBy == null)
                    {
                        //If this is an Aggregate without a GROUP BY projected variables are invalid
                        throw new RdfQueryException("Cannot project the variable ?" + v.Name + " since this Query contains Aggregates and no GROUP BY");
                    }
                    else if (context.Query != null && context.Query.IsAggregate && !context.Query.GroupBy.ProjectableVariables.Contains(v.Name))
                    {
                        //If this is an Aggregate with a GROUP BY projected variables are only valid if they occur in the GROUP BY
                        throw new RdfQueryException("Cannot project the variable ?" + v.Name + " since this Query contains Aggregates but the given Variable is not in the GROUP BY - use the SAMPLE aggregate if you need to access this variable");
                    }

                    //Otherwise we don't need to do anything with the Variable 
                }
            }

            if (context.Query != null && context.Query.IsAggregate && context.Query.GroupBy == null)
            {
                context.OutputMultiset.Add(aggSet);
            }
            else
            {
                context.OutputMultiset = context.InputMultiset;
            }

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
        /// Gets the SPARQL Variables used
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null then it's Variables are used rather than these
        /// </remarks>
        public IEnumerable<SparqlVariable> SparqlVariables
        {
            get
            {
                return this._variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the Projection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Project(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            return this._pattern.ToQuery();
        }

        /// <summary>
        /// Converts the Algebra to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown as this Algebra cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Project() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Project(this._pattern, this._variables);
        }
    }

    /// <summary>
    /// Represents the Selection step of Query Evaluation
    /// </summary>
    /// <remarks>
    /// Selection trims variables from the Multiset that are not needed in the final output.  This is separate from <see cref="Project">Project</see> so that all Variables are available for Ordering and Having clauses
    /// </remarks>
    public class Select
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private List<SparqlVariable> _variables = new List<SparqlVariable>();

        /// <summary>
        /// Creates a new Select
        /// </summary>
        /// <param name="pattern">Inner Pattern</param>
        /// <param name="variables">Variables to Select</param>
        public Select(ISparqlAlgebra pattern, IEnumerable<SparqlVariable> variables)
        {
            this._pattern = pattern;
            this._variables.AddRange(variables);
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
        /// Trims the Results of evaluating the inner pattern to remove Variables which are not Result Variables
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

            HashSet<SparqlVariable> vars;
            bool selectAll = false;
            if (context.Query != null)
            {
                vars = new HashSet<SparqlVariable>(context.Query.Variables);
                switch (context.Query.QueryType)
                {
                    case SparqlQueryType.DescribeAll:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                        selectAll = true;
                        break;
                }
            }
            else
            {
                vars = new HashSet<SparqlVariable>(this._variables);
            }

            if (!selectAll)
            {
                //Trim Variables that aren't being SELECTed
                foreach (String var in context.InputMultiset.Variables.ToList())
                {
                    if (!vars.Any(v => v.Name.Equals(var) && v.IsResultVariable))
                    {
                        //If not a Result variable then trim from results
                        context.InputMultiset.Trim(var);
                    }
                }
            }

            context.OutputMultiset = context.InputMultiset;
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
        /// Gets the SPARQL Variables used
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null then it's Variables are used rather than these
        /// </remarks>
        public IEnumerable<SparqlVariable> SparqlVariables
        {
            get
            {
                return this._variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Select(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            foreach (SparqlVariable var in this._variables)
            {
                q.AddVariable(var);
            }
            if (this._variables.All(v => v.IsResultVariable))
            {
                q.QueryType = SparqlQueryType.SelectAll;
            }
            else
            {
                q.QueryType = SparqlQueryType.Select;
            }
            return q;
        }

        /// <summary>
        /// Throws an error as a Select() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Select() cannot be converted back to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Select() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Select(this._pattern, this._variables);
        }
    }

    /// <summary>
    /// Represents the Ask step of Query Evaluation
    /// </summary>
    /// <remarks>
    /// Used only for ASK queries.  Turns the final Multiset into either an <see cref="IdentityMultiset">IdentityMultiset</see> if the ASK succeeds or a <see cref="NullMultiset">NullMultiset</see> if the ASK fails
    /// </remarks>
    public class Ask 
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new ASK
        /// </summary>
        /// <param name="pattern">Inner Pattern</param>
        public Ask(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the ASK by turning the Results of evaluating the Inner Pattern to either an Identity/Null Multiset depending on whether there were any Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);
            }
            catch (RdfQueryTimeoutException)
            {
                if (!context.Query.PartialResultsOnTimeout) throw;
            }

            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else
            {
                if (context.InputMultiset.IsEmpty)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            return context.OutputMultiset;
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
        /// Gets the String representation of the Ask
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Ask(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            q.QueryType = SparqlQueryType.Ask;
            return q;
        }

        /// <summary>
        /// Throws an exception since an Ask() cannot be converted to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since an Ask() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("An Ask() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Ask(this._pattern);
        }
    }
}
