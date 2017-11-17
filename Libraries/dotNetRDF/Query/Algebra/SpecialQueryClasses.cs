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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Special Algebra Construct for optimising queries of the form SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
    /// </summary>
    public class SelectDistinctGraphs : ISparqlAlgebra
    {
        private readonly String _graphVar;

        /// <summary>
        /// Creates a new Select Distinct algebra
        /// </summary>
        /// <param name="graphVar">Graph Variable to bind Graph URIs to</param>
        public SelectDistinctGraphs(String graphVar)
        {
            _graphVar = graphVar;
        }

        /// <summary>
        /// Evaluates the Select Distinct Graphs optimisation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.OutputMultiset = new Multiset();
            string var = context.Query != null ? context.Query.Variables.First(v => v.IsResultVariable).Name : _graphVar;

            foreach (Uri graphUri in context.Data.GraphUris)
            {
                Set s = new Set();
                s.Add(var, graphUri == null ? null : new UriNode(null, graphUri));
                context.OutputMultiset.Add(s);
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return _graphVar.AsEnumerable(); }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return _graphVar.AsEnumerable(); } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return Enumerable.Empty<String>(); } }

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
                return _graphVar;
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
            q.RootGraphPattern = ToGraphPattern();
            q.AddVariable(_graphVar, true);
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
            String subjVar = (!_graphVar.Equals("s")) ? "?s" : "?subj" ;
            String predVar = (!_graphVar.Equals("p")) ? "?p" : "?pred" ;
            String objVar = (!_graphVar.Equals("o")) ? "o" : "?obj" ;

            p.AddTriplePattern(new TriplePattern(new VariablePattern(subjVar), new VariablePattern(predVar), new VariablePattern(objVar)));
            p.IsGraph = true;
            p.GraphSpecifier = new VariableToken("?" + _graphVar, 0, 0, 0);
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
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return Enumerable.Empty<String>(); } }

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
            q.RootGraphPattern = ToGraphPattern();
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
