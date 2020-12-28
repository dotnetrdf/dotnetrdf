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
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update DELETE command.
    /// </summary>
    public class DeleteCommand : BaseModificationCommand
    {
        /// <summary>
        /// Creates a new DELETE command.
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete.</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
        /// <param name="graphUri">URI of the affected Graph.</param>
        [Obsolete("Replaced by DeleteCommand(GraphPatten, GraphPatter, IRefNode")]
        public DeleteCommand(GraphPattern deletions, GraphPattern where, Uri graphUri)
            : base(SparqlUpdateCommandType.Delete)
        {
            if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            DeletePattern = deletions;
            WherePattern = where;
            WithGraphName = graphUri == null ? null : new UriNode(graphUri);
        }

        /// <summary>
        /// Creates a new DELETE command.
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete.</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
        /// <param name="graphName">Name of the affected Graph.</param>
        public DeleteCommand(GraphPattern deletions, GraphPattern where, IRefNode graphName)
            : base(SparqlUpdateCommandType.Delete)
        {
            if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            DeletePattern = deletions;
            WherePattern = where;
            WithGraphName = graphName;
        }
        /// <summary>
        /// Creates a new DELETE command which operates on the Default Graph.
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete.</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern.</param>
        public DeleteCommand(GraphPattern deletions, GraphPattern where)
            : base(SparqlUpdateCommandType.Delete)
        {
            if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            DeletePattern = deletions;
            WherePattern = where;
        }

        /// <summary>
        /// Creates a new DELETE command. 
        /// </summary>
        /// <param name="where">Pattern to construct Triples to delete.</param>
        /// <param name="graphUri">URI of the affected Graph.</param>
        [Obsolete("Replaced by DeleteCommand(GraphPattern, IRefNode)")]
        public DeleteCommand(GraphPattern where, Uri graphUri)
            : this(where, where, graphUri) { }

        /// <summary>
        /// Creates a new DELETE command. 
        /// </summary>
        /// <param name="where">Pattern to construct Triples to delete.</param>
        /// <param name="graphName">Name of the affected Graph.</param>
        public DeleteCommand(GraphPattern where, IRefNode graphName)
            : this(where, where, graphName)
        {
        }

        /// <summary>
        /// Createa a new DELETE command which operates on the Default Graph.
        /// </summary>
        /// <param name="where">Pattern to construct Triples to delete.</param>
        public DeleteCommand(GraphPattern where)
            : this(where, where, (IRefNode)null) { }

        /// <summary>
        /// Gets whether the Command affects a single Graph.
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                var affectedUris = new List<string>();
                if (TargetGraph != null)
                {
                    affectedUris.Add(TargetGraph.ToString());
                }
                if (DeletePattern.IsGraph) affectedUris.Add(DeletePattern.GraphSpecifier.Value);
                if (DeletePattern.HasChildGraphPatterns)
                {
                    affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                                          where p.IsGraph
                                          select p.GraphSpecifier.Value);
                }

                return affectedUris.Distinct().Count() <= 1;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <returns></returns>
        [Obsolete("Replaced by AffectsGraph(IRefNode)")]
        public override bool AffectsGraph(Uri graphUri)
        {
            var affectedUris = new List<string>();
            if (TargetUri != null)
            {
                affectedUris.Add(TargetUri.AbsoluteUri);
            }
            else
            {
                affectedUris.Add(string.Empty);
            }
            if (DeletePattern.IsGraph) affectedUris.Add(DeletePattern.GraphSpecifier.Value);
            if (DeletePattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

            return affectedUris.Contains(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets whether the Command will potentially affect the given Graph.
        /// </summary>
        /// <param name="graphName">Graph name.</param>
        /// <returns></returns>
        public override bool AffectsGraph(IRefNode graphName)
        {
            var affectedUris = new List<string> {TargetGraph.ToSafeString()};

            if (DeletePattern.IsGraph)
            {
                affectedUris.Add(DeletePattern.GraphSpecifier.Value);
            }
            if (DeletePattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in DeletePattern.ChildGraphPatterns
                    where p.IsGraph
                    select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null)) affectedUris.Add(string.Empty);

            return affectedUris.Contains(graphName.ToSafeString());
        }

        /// <summary>
        /// Gets the URI of the Graph the deletions are made from.
        /// </summary>
        [Obsolete("Replaced by TargetGraph")]
        public Uri TargetUri
        {
            get
            {
                return (WithGraphName as UriNode)?.Uri;
            }
        }

        /// <summary>
        /// Gets the name of the graph the deletions are made from.
        /// </summary>
        public IRefNode TargetGraph => WithGraphName;
       
        /// <summary>
        /// Gets the pattern used for Deletions.
        /// </summary>
        public GraphPattern DeletePattern { get; }

        /// <summary>
        /// Gets the pattern used for the WHERE clause.
        /// </summary>
        public GraphPattern WherePattern { get; }

        /// <summary>
        /// Optimises the Commands WHERE pattern.
        /// </summary>
        public override void Optimise(IQueryOptimiser optimiser)
        {
            WherePattern.Optimise(optimiser);
        }

        /// <summary>
        /// Evaluates the Command in the given Context.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            var defGraphOk = false;
            var datasetOk = false;

            try
            {
                // If there is a WITH clause and no matching graph, and the delete pattern doesn't contain child graph patterns then there is nothing to do
                if (WithGraphName != null && !context.Data.HasGraph(WithGraphName) && !DeletePattern.HasChildGraphPatterns)
                {
                    return;
                }

                // First evaluate the WHERE pattern to get the affected bindings
                ISparqlAlgebra where = WherePattern.ToAlgebra();
                if (context.Commands != null)
                {
                    where = context.Commands.ApplyAlgebraOptimisers(where);
                }

                // Set Active Graph for the WHERE based upon the WITH clause
                // Don't bother if there are USING URIs as these would override any Active Graph we set here
                // so we can save ourselves the effort of doing this
                if (!UsingUris.Any())
                {
                    if (WithGraphName != null)
                    {
                        context.Data.SetActiveGraph(WithGraphName);
                        defGraphOk = true;
                    }
                    else
                    {
                        context.Data.SetActiveGraph((IRefNode)null);
                        defGraphOk = true;
                    }
                }

                // We need to make a dummy SparqlQuery object since if the Command has used any 
                // USING/USING NAMEDs along with GRAPH clauses then the algebra needs to have the
                // URIs available to it which it gets from the Query property of the Context
                // object
                var query = new SparqlQuery();
                foreach (Uri u in UsingUris)
                {
                    query.AddDefaultGraph(new UriNode(u));
                }
                foreach (Uri u in UsingNamedUris)
                {
                    query.AddNamedGraph(new UriNode(u));
                }
                var queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor, context.Options);
                if (UsingUris.Any())
                {
                    // If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                    IList<IRefNode> activeGraphs = _usingUris.Select<Uri, IRefNode>(u => new UriNode(u)).ToList();
                    context.Data.SetActiveGraph(activeGraphs);
                    datasetOk = true;
                }
                BaseMultiset results = queryContext.Evaluate(where);
                if (results is IdentityMultiset) results = new SingletonMultiset(results.Variables);
                if (UsingUris.Any())
                {
                    // If there are USING URIs reset the Active Graph afterwards
                    // Also flag the dataset as no longer being OK as this flag is used in the finally 
                    // block to determine whether the Active Graph needs resetting which it may do if the
                    // evaluation of the 
                    context.Data.ResetActiveGraph();
                    datasetOk = false;
                }

                // Reset Active Graph for the WHERE
                if (defGraphOk)
                {
                    context.Data.ResetActiveGraph();
                    defGraphOk = false;
                }

                // Get the Graph from which we are deleting
                IGraph g = context.Data.HasGraph(WithGraphName) ? context.Data.GetModifiableGraph(WithGraphName) : null;

                // Delete the Triples for each Solution
                foreach (ISet s in results.Sets)
                {
                    var deletedTriples = new List<Triple>();

                    if (g != null)
                    {
                        // Triples from raw Triple Patterns
                        try
                        {
                            var constructContext = new ConstructContext(g, s, true);
                            foreach (IConstructTriplePattern p in DeletePattern.TriplePatterns
                                .OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    deletedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    // If we get an error here then we couldn't construct a specific Triple
                                    // so we continue anyway
                                }
                            }

                            g.Retract(deletedTriples);
                        }
                        catch (RdfQueryException)
                        {
                            // If we throw an error this means we couldn't construct for this solution so the
                            // solution is ignored this graph
                        }
                    }

                    // Triples from GRAPH clauses
                    foreach (GraphPattern gp in DeletePattern.ChildGraphPatterns)
                    {
                        deletedTriples.Clear();
                        try
                        {
                            IRefNode graphName;
                            switch (gp.GraphSpecifier.TokenType)
                            {
                                case Token.URI:
                                    graphName = new UriNode(UriFactory.Create(gp.GraphSpecifier.Value));
                                    break;
                                case Token.VARIABLE:
                                    var graphVar = gp.GraphSpecifier.Value.Substring(1);
                                    if (s.ContainsVariable(graphVar))
                                    {
                                        INode temp = s[graphVar];
                                        if (temp == null)
                                        {
                                            // If the Variable is not bound then skip
                                            continue;
                                        }
                                        else if (temp.NodeType == NodeType.Uri)
                                        {
                                            graphName = temp as IUriNode;
                                        }
                                        else if (temp.NodeType == NodeType.Blank)
                                        {
                                            graphName = temp as IBlankNode;
                                        }
                                        else
                                        {
                                            // If the Variable is not bound to a URI then skip
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        // If the Variable is not bound for this solution then skip
                                        continue;
                                    }
                                    break;
                                default:
                                    // Any other Graph Specifier we have to ignore this solution
                                    continue;
                            }
                            
                            // If the Dataset doesn't contain the Graph then no need to do the Deletions
                            if (!context.Data.HasGraph(graphName)) continue;

                            // Do the actual Deletions
                            IGraph h = context.Data.GetModifiableGraph(graphName);
                            var constructContext = new ConstructContext(h, s, true);
                            foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    deletedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    // If we get an error here then we couldn't construct a specific
                                    // triple so we continue anyway
                                }
                            }
                            h.Retract(deletedTriples);
                        }
                        catch (RdfQueryException)
                        {
                            // If we get an error here this means we couldn't construct for this solution so the
                            // solution is ignored for this graph
                        }
                    }
                }
            }
            finally
            {
                // If the Dataset was set and an error occurred in doing the WHERE clause then
                // we'll need to Reset the Active Graph
                if (datasetOk) context.Data.ResetActiveGraph();
                if (defGraphOk) context.Data.ResetActiveGraph();
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor.
        /// </summary>
        /// <param name="processor">SPARQL Update Processor.</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDeleteCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            var formatter = new SparqlFormatter();
            if (WithGraphName != null)
            {
                output.Append("WITH ");
                output.AppendLine(formatter.Format(WithGraphName));
            }
            output.AppendLine("DELETE");
            if (!ReferenceEquals(DeletePattern, WherePattern)) output.AppendLine(DeletePattern.ToString());
            if (_usingUris != null)
            {
                foreach (Uri u in _usingUris)
                {
                    output.AppendLine("USING <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
                }
            }
            if (_usingNamedUris != null)
            {
                foreach (Uri u in _usingNamedUris)
                {
                    output.AppendLine("USING NAMED <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
                }
            }
            output.AppendLine("WHERE");
            output.AppendLine(WherePattern.ToString());
            return output.ToString();
        }
    }
}
