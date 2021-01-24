/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a GRAPH clause.
    /// </summary>
    public class Graph
        : IUnaryOperator
    {
        private readonly ISparqlAlgebra _pattern;
        private readonly IToken _graphSpecifier;

        /// <summary>
        /// Creates a new Graph clause.
        /// </summary>
        /// <param name="pattern">Pattern.</param>
        /// <param name="graphSpecifier">Graph Specifier.</param>
        public Graph(ISparqlAlgebra pattern, IToken graphSpecifier)
        {
            _pattern = pattern;
            _graphSpecifier = graphSpecifier;
        }

        /// <summary>
        /// Evaluates the Graph Clause by setting up the dataset, applying the pattern and then generating additional bindings if necessary.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Q: Can we optimise GRAPH when the input is the Null Multiset to just return the Null Multiset?

            var datasetOk = false;
            try
            {
                var activeGraphs = new List<string>();

                // Get the URIs of Graphs that should be evaluated over
                if (_graphSpecifier.TokenType != Token.VARIABLE)
                {
                    switch (_graphSpecifier.TokenType)
                    {
                        case Token.URI:
                        case Token.QNAME:
                            IRefNode activeGraphName = new UriNode(context.UriFactory.Create(Tools.ResolveUriOrQName(_graphSpecifier, context.Query.NamespaceMap, context.Query.BaseUri)));
                            if (context.Data.HasGraph(activeGraphName))
                            {
                                // If the Graph is explicitly specified and there are FROM/FROM NAMED present then the Graph 
                                // URI must be in the graphs specified by a FROM/FROM NAMED or the result is null
                                if (context.Query == null ||
                                    (!context.Query.DefaultGraphNames.Any() && !context.Query.NamedGraphNames.Any()) || 
                                    context.Query.NamedGraphNames.Contains(activeGraphName))
                                {
                                    // Either there was no Query 
                                    // OR there were no Default/Named Graphs (hence any Graph URI is permitted) 
                                    // OR the specified URI was a Named Graph URI
                                    // In any case we can go ahead and set the active Graph
                                    activeGraphs.Add(activeGraphName.ToSafeString());
                                }
                                else
                                {
                                    // The specified URI was not present in the Named Graphs so return null
                                    context.OutputMultiset = new NullMultiset();
                                    return context.OutputMultiset;
                                }
                            }
                            else
                            {
                                // If specifies a specific Graph and not in the Dataset result is a null multiset
                                context.OutputMultiset = new NullMultiset();
                                return context.OutputMultiset;
                            }
                            break;
                        default:
                            throw new RdfQueryException("Cannot use a '" + _graphSpecifier.GetType() + "' Token to specify the Graph for a GRAPH clause");
                    }
                }
                else
                {
                    var gvar = _graphSpecifier.Value.Substring(1);

                    // Watch out for the case in which the Graph Variable is not bound for all Sets in which case
                    // we still need to operate over all Graphs
                    if (context.InputMultiset.ContainsVariable(gvar) && context.InputMultiset.Sets.All(s => s[gvar] != null))
                    {
                        // If there are already values bound to the Graph variable for all Input Solutions then we limit the Query to those Graphs
                        var graphUris = new List<Uri>();
                        foreach (ISet s in context.InputMultiset.Sets)
                        {
                            INode temp = s[gvar];
                            if (temp == null) continue;
                            if (temp.NodeType != NodeType.Uri) continue;
                            activeGraphs.Add(temp.ToString());
                            graphUris.Add(((IUriNode) temp).Uri);
                        }
                    }
                    else
                    {
                        // Nothing yet bound to the Graph Variable so the Query is over all the named Graphs
                        if (context.Query != null && context.Query.NamedGraphNames.Any())
                        {
                            // Query specifies one/more named Graphs
                            activeGraphs.AddRange(context.Query.NamedGraphNames.Select(u => u.ToSafeString()));
                        }
                        else if (context.Query != null && context.Query.DefaultGraphNames.Any() && !context.Query.NamedGraphNames.Any())
                        {
                            // Gives null since the query dataset does not include any named graphs
                            context.OutputMultiset = new NullMultiset();
                            return context.OutputMultiset;
                        }
                        else
                        {
                            // Query is over entire dataset/default Graph since no named Graphs are explicitly specified
                            activeGraphs.AddRange(context.Data.GraphNames.Select(u => u.ToSafeString()));
                        }
                    }
                }

                // Remove all duplicates from Active Graphs to avoid duplicate results
                activeGraphs = activeGraphs.Distinct().ToList();

                // Evaluate the inner pattern
                BaseMultiset initialInput = context.InputMultiset;
                BaseMultiset finalResult = new Multiset();

                // Evaluate for each Graph URI and union the results
                foreach (var uri in activeGraphs)
                {
                    // Always use the same Input for each Graph URI and set that Graph to be the Active Graph
                    // Be sure to translate String.Empty back to the null URI to select the default graph
                    // correctly
                    context.InputMultiset = initialInput;
                    IRefNode currGraphName = (uri.Equals(string.Empty)) ? null : 
                        uri.StartsWith("_:") ? new BlankNode(uri) : new UriNode(context.UriFactory.Create(uri));

                    // Set Active Graph
                    if (currGraphName == null)
                    {
                        // GRAPH operates over named graphs only so default graph gets skipped
                        continue;
                    }
                    // The result of the HasGraph() call is ignored we just make it so datasets with any kind of 
                    // load on demand behaviour work properly
                    context.Data.HasGraph(currGraphName);
                    // All we actually care about is setting the active graph
                    context.Data.SetActiveGraph(currGraphName);
                    datasetOk = true;

                    // Evaluate for the current Active Graph
                    BaseMultiset result = context.Evaluate(_pattern);

                    // Merge the Results into our overall Results
                    if (result is NullMultiset)
                    {
                        // Don't do anything, adds nothing to the results
                    }
                    else if (result is IdentityMultiset)
                    {
                        // Adds a single row to the results
                        if (_graphSpecifier.TokenType == Token.VARIABLE)
                        {
                            // Include graph variable if not yet bound
                            INode currGraph = currGraphName;
                            var s = new Set();
                            s.Add(_graphSpecifier.Value.Substring(1), currGraph);
                            finalResult.Add(s);
                        }
                        else
                        {
                            finalResult.Add(new Set());
                        }
                    }
                    else
                    {
                        // If the Graph Specifier is a Variable then we must either bind the
                        // variable or eliminate solutions which have an incorrect value for it
                        if (_graphSpecifier.TokenType == Token.VARIABLE)
                        {
                            var gvar = _graphSpecifier.Value.Substring(1);
                            foreach (var id in result.SetIDs.ToList())
                            {
                                ISet s = result[id];
                                if (s[gvar] == null)
                                {
                                    // If Graph Variable is not yet bound for solution bind it
                                    s.Add(gvar, currGraphName);
                                }
                                else if (!s[gvar].Equals(currGraphName))
                                {
                                    // If Graph Variable is bound for solution and doesn't match
                                    // current Graph then we have to remove the solution
                                    result.Remove(id);
                                }
                            }
                        }
                        // Union solutions into the Results
                        finalResult.Union(result);
                    }

                    // Reset the Active Graph after each pass
                    context.Data.ResetActiveGraph();
                    datasetOk = false;
                }

                // Return the final result
                if (finalResult.IsEmpty) finalResult = new NullMultiset();
                context.OutputMultiset = finalResult;
            }
            finally
            {
                if (datasetOk) context.Data.ResetActiveGraph();
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (_graphSpecifier.TokenType != Token.VARIABLE) return _pattern.Variables.Distinct();

                // Include graph variable
                var graphVar = ((VariableToken) _graphSpecifier).Value.Substring(1);
                return _pattern.Variables.Concat(graphVar.AsEnumerable()).Distinct();
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> FloatingVariables
        {
            get
            {
                if (_graphSpecifier.TokenType != Token.VARIABLE) return _pattern.FloatingVariables;

                // May need to add graph variable to floating variables if it isn't fixed
                // Strictly speaking the graph variable should always be fixed but non-standard implementations may treat the default graph as a named graph with a null URI
                var graphVar = ((VariableToken) _graphSpecifier).Value.Substring(1);
                var fixedVars = new HashSet<string>(FixedVariables);
                return fixedVars.Contains(graphVar) ? _pattern.FloatingVariables : _pattern.FloatingVariables.Concat(graphVar.AsEnumerable()).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<string> FixedVariables
        {
            get { return _pattern.FixedVariables; }
        }

        /// <summary>
        /// Gets the Graph Specifier.
        /// </summary>
        public IToken GraphSpecifier
        {
            get { return _graphSpecifier; }
        }

        /// <summary>
        /// Gets the Inner Algebra.
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get { return _pattern; }
        }

        /// <summary>
        /// Gets the String representation of the Algebra.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Graph({_graphSpecifier.Value}, {_pattern})";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query.
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            var q = new SparqlQuery {RootGraphPattern = ToGraphPattern()};
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern.
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            var p = _pattern.ToGraphPattern();
            if (!p.IsGraph)
            {
                p.IsGraph = true;
                p.GraphSpecifier = _graphSpecifier;
            }
            return p;
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser.
        /// </summary>
        /// <param name="optimiser">Optimiser.</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Graph(optimiser.Optimise(_pattern), _graphSpecifier);
        }

        /// <summary>
        /// Creates a Graph instance by applying a graph specifier to an algebra.
        /// </summary>
        /// <param name="algebra">The algebra to be constrained.</param>
        /// <param name="graphSpecifier">A token specifying the graph constraint.</param>
        /// <returns>A Graph instance representing the application of the graph constraint to the algebra.</returns>
        public static ISparqlAlgebra ApplyGraph(ISparqlAlgebra algebra, IToken graphSpecifier)
        {
            if (!(algebra is Graph)) return new Graph(algebra, graphSpecifier);

            var other = (Graph) algebra;
            if (other.GraphSpecifier.TokenType == graphSpecifier.TokenType && other.GraphSpecifier.Value.Equals(graphSpecifier.Value))
            {
                // We already have the appropriate graph specifier applied to us so reapplying it is unnecessary
                return algebra;
            }
            return new Graph(algebra, graphSpecifier);
        }
    }
}