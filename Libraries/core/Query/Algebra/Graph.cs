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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a GRAPH clause
    /// </summary>
    public class Graph : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private IToken _graphSpecifier;

        /// <summary>
        /// Creates a new Graph clause
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="graphSpecifier">Graph Specifier</param>
        public Graph(ISparqlAlgebra pattern, IToken graphSpecifier)
        {
            this._pattern = pattern;
            this._graphSpecifier = graphSpecifier;
        }

        /// <summary>
        /// Evaluates the Graph Clause by setting up the dataset, applying the pattern and then generating additional bindings if necessary
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset result;

            //Q: Can we optimise GRAPH when the input is the Null Multiset to just return the Null Multiset?

            if (this._pattern is Bgp && ((Bgp)this._pattern).IsEmpty)
            {
                //Optimise the case where we have GRAPH ?g {} by not setting the Graph and just returning
                //a Null Multiset
                result = new NullMultiset();
            }
            else
            {
                bool datasetOk = false;
                try
                {
                    List<String> activeGraphs = new List<string>();

                    //Get the URIs of Graphs that should be evaluated over
                    if (this._graphSpecifier.TokenType != Token.VARIABLE)
                    {
                        switch (this._graphSpecifier.TokenType)
                        {
                            case Token.URI:
                            case Token.QNAME:
                                Uri activeGraphUri = new Uri(Tools.ResolveUriOrQName(this._graphSpecifier, context.Query.NamespaceMap, context.Query.BaseUri));
                                if (context.Data.HasGraph(activeGraphUri))
                                {
                                    //If the Graph is explicitly specified and there are FROM NAMED present then the Graph 
                                    //URI must be in the graphs specified by a FROM NAMED or the result is null
                                    if (context.Query != null &&
                                        ((!context.Query.DefaultGraphs.Any() && !context.Query.NamedGraphs.Any())
                                         || context.Query.DefaultGraphs.Any(u => EqualityHelper.AreUrisEqual(activeGraphUri, u))
                                         || context.Query.NamedGraphs.Any(u => EqualityHelper.AreUrisEqual(activeGraphUri, u)))
                                        )
                                    {
                                        //Either there was no Query OR there were no Default/Named Graphs OR 
                                        //the specified URI was either a Default/Named Graph URI
                                        //In any case we can go ahead and set the active Graph
                                        activeGraphs.Add(activeGraphUri.ToString());
                                    }
                                    else
                                    {
                                        //The specified URI was not present in the Default/Named Graphs so return null
                                        context.OutputMultiset = new NullMultiset();
                                        return context.OutputMultiset;
                                    }
                                }
                                else
                                {
                                    //If specifies a specific Graph and not in the Dataset result is a null multiset
                                    context.OutputMultiset = new NullMultiset();
                                    return context.OutputMultiset;
                                }
                                break;
                            default:
                                throw new RdfQueryException("Cannot use a '" + this._graphSpecifier.GetType().ToString() + "' Token to specify the Graph for a GRAPH clause");
                        }
                    }
                    else
                    {
                        String gvar = this._graphSpecifier.Value.Substring(1);

                        //Watch out for the case in which the Graph Variable is not bound for all Sets in which case
                        //we still need to operate over all Graphs
                        if (context.InputMultiset.ContainsVariable(gvar) && context.InputMultiset.Sets.All(s => s[gvar] != null))
                        {
                            //If there are already values bound to the Graph variable for all Input Solutions then we limit the Query to those Graphs
                            List<Uri> graphUris = new List<Uri>();
                            foreach (ISet s in context.InputMultiset.Sets)
                            {
                                INode temp = s[gvar];
                                if (temp != null)
                                {
                                    if (temp.NodeType == NodeType.Uri)
                                    {
                                        activeGraphs.Add(temp.ToString());
                                        graphUris.Add(((IUriNode)temp).Uri);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Nothing yet bound to the Graph Variable so the Query is over all the named Graphs
                            if (context.Query != null && context.Query.NamedGraphs.Any())
                            {
                                //Query specifies one/more named Graphs
                                activeGraphs.AddRange(context.Query.NamedGraphs.Select(u => u.ToString()));
                            }
                            else
                            {
                                //Query is over entire dataset/default Graph since no named Graphs are explicitly specified
                                activeGraphs.AddRange(context.Data.GraphUris.Select(u => u.ToSafeString()));
                            }
                        }
                    }

                    //Remove all duplicates from Active Graphs to avoid duplicate results
                    activeGraphs = activeGraphs.Distinct().ToList();

                    //Evaluate the inner pattern
                    BaseMultiset initialInput = context.InputMultiset;
                    BaseMultiset finalResult = new Multiset();

                    //Evalute for each Graph URI and union the results
                    foreach (String uri in activeGraphs)
                    {
                        //Always use the same Input for each Graph URI and set that Graph to be the Active Graph
                        //Be sure to translate String.Empty back to the null URI to select the default graph
                        //correctly
                        context.InputMultiset = initialInput;
                        Uri currGraphUri = (uri.Equals(String.Empty)) ? null : new Uri(uri);

                        //This bit of logic takes care of the fact that calling SetActiveGraph((Uri)null) resets the
                        //Active Graph to be the default graph which if the default graph is null is usually the Union of
                        //all Graphs in the Store
                        if (currGraphUri == null && context.Data.DefaultGraph == null && context.Data.UsesUnionDefaultGraph)
                        {
                            if (context.Data.HasGraph(null))
                            {
                                context.Data.SetActiveGraph(context.Data[null]);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            context.Data.SetActiveGraph(currGraphUri);
                        }
                        datasetOk = true;

                        //Evaluate for the current Active Graph
                        result = context.Evaluate(this._pattern);

                        //Merge the Results into our overall Results
                        if (result is NullMultiset || result is IdentityMultiset)
                        {
                            //Don't do anything
                        }
                        else
                        {
                            //If the Graph Specifier is a Variable then we must either bind the
                            //variable or eliminate solutions which have an incorrect value for it
                            if (this._graphSpecifier.TokenType == Token.VARIABLE)
                            {
                                String gvar = this._graphSpecifier.Value.Substring(1);
                                INode currGraph = (currGraphUri == null) ? null : new UriNode(null, currGraphUri);
                                foreach (int id in result.SetIDs.ToList())
                                {
                                    ISet s = result[id];
                                    if (s[gvar] == null)
                                    {
                                        //If Graph Variable is not yet bound for solution bind it
                                        s.Add(gvar, currGraph);
                                    }
                                    else if (!s[gvar].Equals(currGraph))
                                    {
                                        //If Graph Variable is bound for solution and doesn't match
                                        //current Graph then we have to remove the solution
                                        result.Remove(id);
                                    }
                                }
                            } 
                            //Union solutions into the Results
                            finalResult.Union(result);
                        }

                        //Reset the Active Graph after each pass
                        context.Data.ResetActiveGraph();
                        datasetOk = false;
                    }

                    //Return the final result
                    if (finalResult.IsEmpty) finalResult = new NullMultiset();
                    context.OutputMultiset = finalResult;
                }
                finally
                {
                    if (datasetOk) context.Data.ResetActiveGraph();
                }
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
                if (this._graphSpecifier.TokenType == Token.VARIABLE)
                {
                    String graphVar = ((VariableToken)this._graphSpecifier).Value.Substring(1);
                    return this._pattern.Variables.Concat(graphVar.AsEnumerable()).Distinct();
                }
                else
                {
                    return this._pattern.Variables.Distinct();
                }
            }
        }

        /// <summary>
        /// Gets the Graph Specifier
        /// </summary>
        public IToken GraphSpecifier
        {
            get
            {
                return this._graphSpecifier;
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
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Graph(" + this._graphSpecifier.Value + ", " + this._pattern.ToString() + ")";
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
            if (!p.IsGraph)
            {
                p.IsGraph = true;
                p.GraphSpecifier = this._graphSpecifier;
            }
            return p;
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Graph(this._pattern, this._graphSpecifier);
        }
    }
}
