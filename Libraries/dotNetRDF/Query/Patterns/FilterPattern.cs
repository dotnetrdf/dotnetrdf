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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Filter Patterns in SPARQL Queries
    /// </summary>
    /// <remarks>
    /// A Filter Pattern is any FILTER clause that can be executed during the process of executing Triple Patterns rather than after all the Triple Patterns and Child Graph Patterns have been executed
    /// </remarks>
    public class FilterPattern 
        : BaseTriplePattern, IFilterPattern, IComparable<FilterPattern>
    {
        private readonly ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Filter Pattern with the given Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        public FilterPattern(ISparqlFilter filter)
        {
            _filter = filter;
            _vars = filter.Variables.Distinct().ToList();
            _vars.Sort();
        }

        /// <summary>
        /// Evaluates a Filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset)
            {
                // If we get a NullMultiset then the FILTER has no effect since there are already no results
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                if (!_filter.Variables.Any())
                {
                    // If we get an IdentityMultiset then the FILTER only has an effect if there are no
                    // variables - otherwise it is not in scope and is ignored

                    try
                    {
                        if (!_filter.Expression.Evaluate(context, 0).AsSafeBoolean())
                        {
                            context.OutputMultiset = new NullMultiset();
                            return;
                        }
                    }
                    catch
                    {
                        context.OutputMultiset = new NullMultiset();
                        return;
                    }
                }
            }
            else
            {
                _filter.Evaluate(context);
            }
            context.OutputMultiset = new IdentityMultiset();
        }

        /// <summary>
        /// Gets the Pattern Type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get
            {
                return TriplePatternType.Filter;
            }
        }

        /// <summary>
        /// Returns that the Pattern is not an accept all (since it's a Filter)
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Filter that this Pattern uses
        /// </summary>
        public ISparqlFilter Filter
        {
            get
            {
                return _filter;
            }
        }

        /// <summary>
        /// Returns the empty enumerable as don't know which variables will be bound
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        /// Returns the empty enumerable as don't know which variables will be bound
        /// </summary>
        public override IEnumerable<string> FloatingVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Gets whether the Pattern uses the Default Dataset
        /// </summary>
        public override bool UsesDefaultDataset
        {
            get
            {
                return _filter.Expression.UsesDefaultDataset();
            }
        }

        /// <summary>
        /// Returns true as a FILTER cannot contain blank variables
        /// </summary>
        /// <remarks>
        /// Technically blank nodes may appear in a FILTER as part of an EXISTS/NOT EXISTS clause but in that case they would not be visible outside of the FILTER and so are not relevant
        /// </remarks>
        public override bool HasNoBlankVariables
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Compares a filter pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(FilterPattern other)
        {
            return CompareTo((IFilterPattern)other);
        }

        /// <summary>
        /// Compares a filter pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(IFilterPattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Returns the string representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _filter.ToString();
        }
    }
}
