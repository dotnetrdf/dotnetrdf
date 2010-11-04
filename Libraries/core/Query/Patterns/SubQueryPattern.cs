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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Sub-queries which occur as part of a SPARQL query
    /// </summary>
    public class SubQueryPattern : BaseTriplePattern
    {
        private SparqlQuery _subquery;

        /// <summary>
        /// Creates a new Sub-query pattern which represents the given sub-query
        /// </summary>
        /// <param name="subquery">Sub-query</param>
        public SubQueryPattern(SparqlQuery subquery)
        {
            this._subquery = subquery;
            this._indexType = TripleIndexType.SpecialSubQuery;
            
            //Get the Variables this query projects out
            foreach (SparqlVariable var in this._subquery.Variables)
            {
                if (var.IsResultVariable)
                {
                    this._vars.Add(var.Name);
                }
            }
            this._vars.Sort();
        }

        /// <summary>
        /// Gets the Sub-Query
        /// </summary>
        public SparqlQuery SubQuery
        {
            get
            {
                return this._subquery;
            }
        }

        /// <summary>
        /// Evaluates a Sub-query in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            this._subquery.Optimise();

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
                SparqlEvaluationContext subcontext = new SparqlEvaluationContext(this._subquery, context.Data);
                subcontext.InputMultiset = context.InputMultiset;

                ISparqlAlgebra query = this._subquery.ToAlgebra();
                try
                {
                    context.OutputMultiset = query.Evaluate(subcontext);
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
        /// Gets the string representation of the sub-query
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{" + this._subquery.ToString() + "}";
        }
    }
}
