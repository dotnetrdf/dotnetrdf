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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Abstract Base Class for specialised Filters which restrict the value of a variable to some values
    /// </summary>
    public abstract class VariableRestrictionFilter 
        : IFilter
    {
        private ISparqlAlgebra _pattern;
        private String _var;
        private ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Variable Restriction Filter
        /// </summary>
        /// <param name="pattern">Algebra the filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="filter">Filter to use</param>
        public VariableRestrictionFilter(ISparqlAlgebra pattern, String var, ISparqlFilter filter)
        {
            this._pattern = pattern;
            this._var = var;
            this._filter = filter;
        }

        /// <summary>
        /// Evalutes the algebra for the given evaluation context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public abstract BaseMultiset Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the Variable that this filter restricts the value of
        /// </summary>
        public String RestrictionVariable
        {
            get
            {
                return this._var;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._pattern.Variables.Concat(this._filter.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the Filter to be used
        /// </summary>
        public ISparqlFilter SparqlFilter
        {
            get
            {
                return this._filter;
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
        /// Gets the String representation of the FILTER
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String filter = this._filter.ToString();
            filter = filter.Substring(7, filter.Length - 8);
            return this.GetType().Name + "(" + this._pattern.ToString() + ", " + filter + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = this._pattern.ToGraphPattern();
            GraphPattern f = new GraphPattern();
            f.AddFilter(this._filter);
            p.AddGraphPattern(f);
            return p;
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public abstract ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);
    }

    /// <summary>
    /// Abstract Base Class for specialised Filters which restrict the value of a variable to a single value
    /// </summary>
    public abstract class SingleValueRestrictionFilter 
        : VariableRestrictionFilter
    {
        private NodeExpressionTerm _term;

        /// <summary>
        /// Creates a new Single Value Restriction Filter
        /// </summary>
        /// <param name="pattern">Algebra the filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="term">Value to restrict to</param>
        /// <param name="filter">Filter to use</param>
        public SingleValueRestrictionFilter(ISparqlAlgebra pattern, String var, NodeExpressionTerm term, ISparqlFilter filter)
            : base(pattern, var, filter)
        {
            this._term = term;
        }

        /// <summary>
        /// Gets the Value Restriction which this filter applies
        /// </summary>
        public NodeExpressionTerm RestrictionValue
        {
            get
            {
                return this._term;
            }
        }

        /// <summary>
        /// Applies the Filter over the results of evaluating the inner pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public sealed override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            INode term = this._term.Value(null, 0);

            //First take appropriate pre-filtering actions
            if (context.InputMultiset is IdentityMultiset)
            {
                //If the Input is Identity switch the input to be a Multiset containing a single Set
                //where the variable is bound to the term
                context.InputMultiset = new Multiset();
                Set s = new Set();
                s.Add(this.RestrictionVariable, term);
                context.InputMultiset.Add(s);
            }
            else if (context.InputMultiset is NullMultiset)
            {
                //If Input is Null then output is Null
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
            else
            {
                if (context.InputMultiset.ContainsVariable(this.RestrictionVariable))
                {
                    //If the Input Multiset contains the variable then pre-filter
                    foreach (int id in context.InputMultiset.SetIDs.ToList())
                    {
                        ISet x = context.InputMultiset[id];
                        try
                        {
                            if (x.ContainsVariable(this.RestrictionVariable))
                            {
                                //If does exist check it has appropriate value and if not remove it
                                if (!term.Equals(x[this.RestrictionVariable])) context.InputMultiset.Remove(id);
                            }
                            else
                            {
                                //If doesn't exist for this set then bind it to the term
                                x.Add(this.RestrictionVariable, term);
                            }
                        }
                        catch (RdfQueryException)
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }
                else
                {
                    //If it doesn't contain the variable then bind for each existing set
                    foreach (ISet x in context.InputMultiset.Sets)
                    {
                        x.Add(this.RestrictionVariable, term);
                    }
                }
            }

            //Then evaluate the inner algebra
            BaseMultiset results = context.Evaluate(this.InnerAlgebra);
            if (results is NullMultiset || results is IdentityMultiset) return results;

            //Filter the results to ensure that the variable is indeed bound to the term
            foreach (int id in results.SetIDs.ToList())
            {
                ISet x = results[id];
                try
                {
                    if (!term.Equals(x[this.RestrictionVariable]))
                    {
                        results.Remove(id);
                    }
                }
                catch (RdfQueryException)
                {
                    results.Remove(id);
                }
            }

            if (results.Count > 0)
            {
                context.OutputMultiset = results;
            }
            else
            {
                context.OutputMultiset = new NullMultiset();
            }
            return context.OutputMultiset;
        }
    }

    /// <summary>
    /// Represents a special case Filter where the Filter restricts a variable to just one value i.e. FILTER(?x = &lt;value&gt;)
    /// </summary>
    public class IdentityFilter 
        : SingleValueRestrictionFilter
    {
        /// <summary>
        /// Creates a new Identity Filter
        /// </summary>
        /// <param name="pattern">Algebra the Filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="term">Expression Term</param>
        public IdentityFilter(ISparqlAlgebra pattern, String var, NodeExpressionTerm term)
            : base(pattern, var, term, new UnaryExpressionFilter(new EqualsExpression(new VariableExpressionTerm(var), term))) { }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new IdentityFilter(optimiser.Optimise(this.InnerAlgebra), this.RestrictionVariable, (NodeExpressionTerm)((IExpressionTransformer)optimiser).Transform(this.RestrictionValue));
            }
            else
            {
                return new IdentityFilter(optimiser.Optimise(this.InnerAlgebra), this.RestrictionVariable, this.RestrictionValue);
            }
        }
    }

    /// <summary>
    /// Represents a special case Filter where the Filter is supposed to restrict a variable to just one value i.e. FILTER(SAMETERM(?x, &lt;value&gt;))
    /// </summary>
    public class SameTermFilter
        : SingleValueRestrictionFilter
    {
        /// <summary>
        /// Creates a new Same Term Filter
        /// </summary>
        /// <param name="pattern">Algebra the Filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="term">Expression Term</param>
        public SameTermFilter(ISparqlAlgebra pattern, String var, NodeExpressionTerm term)
            : base(pattern, var, term, new UnaryExpressionFilter(new SameTermFunction(new VariableExpressionTerm(var), term))) { }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new SameTermFilter(optimiser.Optimise(this.InnerAlgebra), this.RestrictionVariable, (NodeExpressionTerm)((IExpressionTransformer)optimiser).Transform(this.RestrictionValue));
            }
            else
            {
                return new SameTermFilter(optimiser.Optimise(this.InnerAlgebra), this.RestrictionVariable, this.RestrictionValue);
            }
        }
    }
}
