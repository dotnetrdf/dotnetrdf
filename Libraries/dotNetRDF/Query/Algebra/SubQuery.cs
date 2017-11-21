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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a sub-query as an Algebra operator (only used when strict algebra is generated)
    /// </summary>
    public class SubQuery : ITerminalOperator
    {
        private readonly SparqlQuery _subquery;

        /// <summary>
        /// Creates a new subquery operator
        /// </summary>
        /// <param name="q">Subquery</param>
        public SubQuery(SparqlQuery q)
        {
            _subquery = q;
        }

        /// <summary>
        /// Evaluates the subquery in the given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
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

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the variables used in the subquery which are projected out of it
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            { 
                return _subquery.Variables.Where(v => v.IsResultVariable).Select(v => v.Name); 
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return _subquery.ToAlgebra().FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _subquery.ToAlgebra().FixedVariables; } }

        /// <summary>
        /// Converts the algebra back into a Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
            return q;
        }

        /// <summary>
        /// Converts the algebra back into a Subquery
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.TriplePatterns.Add(new SubQueryPattern(_subquery));
            return gp;
        }

        /// <summary>
        /// Gets the string representation of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Subquery()";
        }
    }
}
