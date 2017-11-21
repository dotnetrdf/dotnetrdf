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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
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
        private readonly ISparqlAlgebra _pattern;
        private readonly String _var;
        private readonly ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Variable Restriction Filter
        /// </summary>
        /// <param name="pattern">Algebra the filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="filter">Filter to use</param>
        public VariableRestrictionFilter(ISparqlAlgebra pattern, String var, ISparqlFilter filter)
        {
            _pattern = pattern;
            _var = var;
            _filter = filter;
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
                return _var;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (_pattern.Variables.Concat(_filter.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return _pattern.FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _pattern.FixedVariables; } }

        /// <summary>
        /// Gets the Filter to be used
        /// </summary>
        public ISparqlFilter SparqlFilter
        {
            get
            {
                return _filter;
            }
        }

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
        /// Gets the String representation of the FILTER
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String filter = _filter.ToString();
            filter = filter.Substring(7, filter.Length - 8);
            return GetType().Name + "(" + _pattern.ToString() + ", " + filter + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = _pattern.ToGraphPattern();
            GraphPattern f = new GraphPattern();
            f.AddFilter(_filter);
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
        private ConstantTerm _term;

        /// <summary>
        /// Creates a new Single Value Restriction Filter
        /// </summary>
        /// <param name="pattern">Algebra the filter applies over</param>
        /// <param name="var">Variable to restrict on</param>
        /// <param name="term">Value to restrict to</param>
        /// <param name="filter">Filter to use</param>
        public SingleValueRestrictionFilter(ISparqlAlgebra pattern, String var, ConstantTerm term, ISparqlFilter filter)
            : base(pattern, var, filter)
        {
            _term = term;
        }

        /// <summary>
        /// Gets the Value Restriction which this filter applies
        /// </summary>
        public ConstantTerm RestrictionValue
        {
            get
            {
                return _term;
            }
        }

        /// <summary>
        /// Applies the Filter over the results of evaluating the inner pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public sealed override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            INode term = _term.Evaluate(null, 0);

            // First take appropriate pre-filtering actions
            if (context.InputMultiset is IdentityMultiset)
            {
                // If the Input is Identity switch the input to be a Multiset containing a single Set
                // where the variable is bound to the term
                context.InputMultiset = new Multiset();
                Set s = new Set();
                s.Add(RestrictionVariable, term);
                context.InputMultiset.Add(s);
            }
            else if (context.InputMultiset is NullMultiset)
            {
                // If Input is Null then output is Null
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
            else
            {
                if (context.InputMultiset.ContainsVariable(RestrictionVariable))
                {
                    // If the Input Multiset contains the variable then pre-filter
                    foreach (int id in context.InputMultiset.SetIDs.ToList())
                    {
                        ISet x = context.InputMultiset[id];
                        try
                        {
                            if (x.ContainsVariable(RestrictionVariable))
                            {
                                // If does exist check it has appropriate value and if not remove it
                                if (!term.Equals(x[RestrictionVariable])) context.InputMultiset.Remove(id);
                            }
                            else
                            {
                                // If doesn't exist for this set then bind it to the term
                                x.Add(RestrictionVariable, term);
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
                    // If it doesn't contain the variable then bind for each existing set
                    foreach (ISet x in context.InputMultiset.Sets)
                    {
                        x.Add(RestrictionVariable, term);
                    }
                }
            }

            // Then evaluate the inner algebra
            BaseMultiset results = context.Evaluate(InnerAlgebra);
            if (results is NullMultiset || results is IdentityMultiset) return results;

            // Filter the results to ensure that the variable is indeed bound to the term
            foreach (int id in results.SetIDs.ToList())
            {
                ISet x = results[id];
                try
                {
                    if (!term.Equals(x[RestrictionVariable]))
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
        public IdentityFilter(ISparqlAlgebra pattern, String var, ConstantTerm term)
            : base(pattern, var, term, new UnaryExpressionFilter(new EqualsExpression(new VariableTerm(var), term))) { }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new IdentityFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, (ConstantTerm)((IExpressionTransformer)optimiser).Transform(RestrictionValue));
            }
            else
            {
                return new IdentityFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, RestrictionValue);
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
        public SameTermFilter(ISparqlAlgebra pattern, String var, ConstantTerm term)
            : base(pattern, var, term, new UnaryExpressionFilter(new SameTermFunction(new VariableTerm(var), term))) { }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new SameTermFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, (ConstantTerm)((IExpressionTransformer)optimiser).Transform(RestrictionValue));
            }
            else
            {
                return new SameTermFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, RestrictionValue);
            }
        }
    }
}
