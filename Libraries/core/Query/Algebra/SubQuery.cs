/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a sub-query as an Algebra operator (only used when strict algebra is generated)
    /// </summary>
    public class SubQuery : ITerminalOperator
    {
        private SparqlQuery _subquery;

        /// <summary>
        /// Creates a new subquery operator
        /// </summary>
        /// <param name="q">Subquery</param>
        public SubQuery(SparqlQuery q)
        {
            this._subquery = q;
        }

        /// <summary>
        /// Evaluates the subquery in the given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Use the same algebra optimisers as the parent query (if any)
            if (context.Query != null)
            {
                this._subquery.AlgebraOptimisers = context.Query.AlgebraOptimisers;
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
                SparqlEvaluationContext subcontext = new SparqlEvaluationContext(this._subquery, context.Data, context.Processor);
                subcontext.InputMultiset = context.InputMultiset;

                //Add any Named Graphs to the subquery
                if (context.Query != null)
                {
                    foreach (Uri u in context.Query.NamedGraphs)
                    {
                        this._subquery.AddNamedGraph(u);
                    }
                }

                ISparqlAlgebra query = this._subquery.ToAlgebra();
                try
                {
                    //Evaluate the Subquery
                    context.OutputMultiset = subcontext.Evaluate(query);

                    //If the Subquery contains a GROUP BY it may return a Group Multiset in which case we must flatten this to a Multiset
                    if (context.OutputMultiset is GroupMultiset)
                    {
                        context.OutputMultiset = new Multiset((GroupMultiset)context.OutputMultiset);
                    }

                    //Strip out any Named Graphs from the subquery
                    if (this._subquery.NamedGraphs.Any())
                    {
                        this._subquery.ClearNamedGraphs();
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
                return this._subquery.Variables.Where(v => v.IsResultVariable).Select(v => v.Name); 
            }
        }

        /// <summary>
        /// Converts the algebra back into a Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            return q;
        }

        /// <summary>
        /// Converts the algebra back into a Subquery
        /// </summary>
        /// <returns></returns>
        public VDS.RDF.Query.Patterns.GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.TriplePatterns.Add(new SubQueryPattern(this._subquery));
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
