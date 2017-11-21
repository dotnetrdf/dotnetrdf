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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Sub-queries which occur as part of a SPARQL query
    /// </summary>
    public class SubQueryPattern 
        : BaseTriplePattern, ISubQueryPattern, IComparable<SubQueryPattern>
    {
        private readonly SparqlQuery _subquery;

        /// <summary>
        /// Creates a new Sub-query pattern which represents the given sub-query
        /// </summary>
        /// <param name="subquery">Sub-query</param>
        public SubQueryPattern(SparqlQuery subquery)
        {
            _subquery = subquery;
            
            // Get the Variables this query projects out
            foreach (SparqlVariable var in _subquery.Variables)
            {
                if (var.IsResultVariable)
                {
                    _vars.Add(var.Name);
                }
            }
            _vars.Sort();
        }

        /// <summary>
        /// Gets the Sub-Query
        /// </summary>
        public SparqlQuery SubQuery
        {
            get
            {
                return _subquery;
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public override IEnumerable<String> FloatingVariables { get { return _subquery.ToAlgebra().FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public override IEnumerable<String> FixedVariables { get { return _subquery.ToAlgebra().FixedVariables; } }

        /// <summary>
        /// Gets the pattern type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get 
            {
                return TriplePatternType.SubQuery; 
            }
        }

        /// <summary>
        /// Evaluates a Sub-query in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            // Use the same algebra optimisers as the parent query (if any)
            if (context.Query != null)
            {
                _subquery.AlgebraOptimisers = context.Query.AlgebraOptimisers;
            }

            if (context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else if (context.InputMultiset.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                SparqlEvaluationContext subcontext = new SparqlEvaluationContext(_subquery, context.Data, context.Processor);
                subcontext.InputMultiset = context.InputMultiset;

                // Add any Named Graphs to the subquery
                if (context.Query != null)
                {
                    foreach (Uri u in context.Query.NamedGraphs)
                    {
                        _subquery.AddNamedGraph(u);
                    }
                }

                ISparqlAlgebra query = _subquery.ToAlgebra();
                try
                {
                    // Evaluate the Subquery
                    context.OutputMultiset = subcontext.Evaluate(query);

                    // If the Subquery contains a GROUP BY it may return a Group Multiset in which case we must flatten this to a Multiset
                    if (context.OutputMultiset is GroupMultiset)
                    {
                        context.OutputMultiset = new Multiset((GroupMultiset)context.OutputMultiset);
                    }

                    // Strip out any Named Graphs from the subquery
                    if (_subquery.NamedGraphs.Any())
                    {
                        _subquery.ClearNamedGraphs();
                    }
                }
                catch (RdfQueryException queryEx)
                {
                    throw new RdfQueryException("Query failed due to a failure in Subquery Execution:\n" + queryEx.Message, queryEx);
                }
            }
        }

        /// <summary>
        /// Returns that the Pattern is not an accept all since it is a Sub-query
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the Sub-query is Thread Safe
        /// </summary>
        public override bool UsesDefaultDataset
        {
            get
            {
                return _subquery.UsesDefaultDataset;
            }
        }

        /// <summary>
        /// Returns true as while a sub-query may contain blank node variables they will not be in scope here
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Compares a sub-query pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(SubQueryPattern other)
        {
            return CompareTo((ISubQueryPattern)other);
        }

        /// <summary>
        /// Compares a sub-query pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(ISubQueryPattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Gets the string representation of the sub-query
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{" + _subquery.ToString() + "}";
        }
    }
}
