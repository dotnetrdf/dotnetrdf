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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Filter
    /// </summary>
    public class Filter 
        : IFilter
    {
        private readonly ISparqlAlgebra _pattern;
        private readonly ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Filter
        /// </summary>
        /// <param name="pattern">Algebra the Filter applies over</param>
        /// <param name="filter">Filter to apply</param>
        public Filter(ISparqlAlgebra pattern, ISparqlFilter filter)
        {
            _pattern = pattern;
            _filter = filter;
        }

        /// <summary>
        /// Applies the Filter over the results of evaluating the inner pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Apply the Pattern first
            context.InputMultiset = context.Evaluate(_pattern);

            if (context.InputMultiset is NullMultiset)
            {
                // If we get a NullMultiset then the FILTER has no effect since there are already no results
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                if (_filter.Variables.Any())
                {
                    // If we get an IdentityMultiset then the FILTER only has an effect if there are no
                    // variables - otherwise it is not in scope and causes the Output to become Null
                    context.InputMultiset = new NullMultiset();
                }
                else
                {
                    try
                    {
                        if (!_filter.Expression.Evaluate(context, 0).AsSafeBoolean())
                        {
                            context.OutputMultiset = new NullMultiset();
                            return context.OutputMultiset;
                        }
                    }
                    catch
                    {
                        context.OutputMultiset = new NullMultiset();
                        return context.OutputMultiset;
                    }
                }
            }
            else
            {
                _filter.Evaluate(context);
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
            return "Filter(" + _pattern.ToString() + ", " + filter + ")";
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
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new Filter(optimiser.Optimise(_pattern), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(_filter.Expression)));
            }
            else
            {
                return new Filter(optimiser.Optimise(_pattern), _filter);
            }
        }
    }
}
