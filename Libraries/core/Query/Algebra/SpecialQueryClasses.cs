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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Special Algebra Construct for optimising queries of the form SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
    /// </summary>
    public class SelectDistinctGraphs : ISparqlAlgebra
    {
        private String _graphVar;

        /// <summary>
        /// Creates a new Select Distinct algebra
        /// </summary>
        /// <param name="graphVar">Graph Variable to bind Graph URIs to</param>
        public SelectDistinctGraphs(String graphVar)
        {
            this._graphVar = graphVar;
        }

        /// <summary>
        /// Evaluates the Select Distinct Graphs optimisation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.OutputMultiset = new Multiset();
            String var;
            if (context.Query != null)
            {
                var = context.Query.Variables.First(v => v.IsResultVariable).Name;
            }
            else
            {
                var = this._graphVar;
            }

            foreach (Uri graphUri in context.Data.GraphUris)
            {
                Set s = new Set();
                if (graphUri == null)
                {
                    s.Add(var, null);
                }
                else
                {
                    s.Add(var, new UriNode(null, graphUri));
                }
                context.OutputMultiset.Add(s);
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
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Graph Variable to which Graph URIs are bound
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null then the Variable Name from the Query is used rather than this
        /// </remarks>
        public String GraphVariable
        {
            get
            {
                return this._graphVar;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SelectDistinctGraphs()";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.AddVariable(this._graphVar, true);
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            String subjVar = (!this._graphVar.Equals("s")) ? "?s" : "?subj" ;
            String predVar = (!this._graphVar.Equals("p")) ? "?p" : "?pred" ;
            String objVar = (!this._graphVar.Equals("o")) ? "o" : "?obj" ;

            p.AddTriplePattern(new TriplePattern(new VariablePattern(subjVar), new VariablePattern(predVar), new VariablePattern(objVar)));
            p.IsGraph = true;
            p.GraphSpecifier = new VariableToken("?" + this._graphVar, 0, 0, 0);
            return p;
        }
    }

    /// <summary>
    /// Special Algebra Construct for optimising queries of the form ASK WHERE {?s ?p ?o}
    /// </summary>
    public class AskAnyTriples : ISparqlAlgebra
    {
        /// <summary>
        /// Evalutes the Ask Any Triples optimisation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (context.Data.HasTriples)
            {
                context.OutputMultiset = new IdentityMultiset();
            }
            else
            {
                context.OutputMultiset = new NullMultiset();
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
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "AskAnyTriples()";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.QueryType = SparqlQueryType.Ask;
            return q;
        }

        /// <summary>
        /// Converts the Algebra to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            p.AddTriplePattern(new TriplePattern(new VariablePattern("?s"), new VariablePattern("?p"), new VariablePattern("?o")));
            return p;
        }
    }
}
