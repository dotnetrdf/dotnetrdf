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
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update DELETE command
    /// </summary>
    public class DeleteCommand : BaseModificationCommand
    {
        private GraphPattern _deletePattern, _wherePattern;

        /// <summary>
        /// Creates a new DELETE command
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        public DeleteCommand(GraphPattern deletions, GraphPattern where, Uri graphUri)
            : base(SparqlUpdateCommandType.Delete)
        {
            if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            _deletePattern = deletions;
            _wherePattern = where;
            _graphUri = graphUri;
        }

        /// <summary>
        /// Creates a new DELETE command which operates on the Default Graph
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern</param>
        public DeleteCommand(GraphPattern deletions, GraphPattern where)
            : base(SparqlUpdateCommandType.Delete)
        {
            if (!IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            _deletePattern = deletions;
            _wherePattern = where;
        }

        /// <summary>
        /// Creates a new DELETE command 
        /// </summary>
        /// <param name="where">Pattern to construct Triples to delete</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        public DeleteCommand(GraphPattern where, Uri graphUri)
            : this(where, where, graphUri) { }

        /// <summary>
        /// Createa a new DELETE command which operates on the Default Graph
        /// </summary>
        /// <param name="where">Pattern to construct Triples to delete</param>
        public DeleteCommand(GraphPattern where)
            : this(where, where, null) { }

        /// <summary>
        /// Gets whether the Command affects a single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                List<String> affectedUris = new List<string>();
                if (TargetUri != null)
                {
                    affectedUris.Add(TargetUri.AbsoluteUri);
                }
                if (_deletePattern.IsGraph) affectedUris.Add(_deletePattern.GraphSpecifier.Value);
                if (_deletePattern.HasChildGraphPatterns)
                {
                    affectedUris.AddRange(from p in _deletePattern.ChildGraphPatterns
                                          where p.IsGraph
                                          select p.GraphSpecifier.Value);
                }

                return affectedUris.Distinct().Count() <= 1;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            List<String> affectedUris = new List<string>();
            if (TargetUri != null)
            {
                affectedUris.Add(TargetUri.AbsoluteUri);
            }
            else
            {
                affectedUris.Add(String.Empty);
            }
            if (_deletePattern.IsGraph) affectedUris.Add(_deletePattern.GraphSpecifier.Value);
            if (_deletePattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in _deletePattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null)) affectedUris.Add(String.Empty);

            return affectedUris.Contains(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets the URI of the Graph the deletions are made from
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return _graphUri;
            }
        }
       
        /// <summary>
        /// Gets the pattern used for Deletions
        /// </summary>
        public GraphPattern DeletePattern
        {
            get
            {
                return _deletePattern;
            }
        }

        /// <summary>
        /// Gets the pattern used for the WHERE clause
        /// </summary>
        public GraphPattern WherePattern
        {
            get
            {
                return _wherePattern;
            }
        }

        /// <summary>
        /// Optimises the Commands WHERE pattern
        /// </summary>
        public override void Optimise(IQueryOptimiser optimiser)
        {
            _wherePattern.Optimise(optimiser);
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            bool defGraphOk = false;
            bool datasetOk = false;

            try
            {
                // If there is a WITH clause and no matching graph, and the delete pattern doesn't contain child graph patterns then there is nothing to do
                if (_graphUri != null && !context.Data.HasGraph(_graphUri) && !_deletePattern.HasChildGraphPatterns)
                {
                    return;
                }

                // First evaluate the WHERE pattern to get the affected bindings
                ISparqlAlgebra where = _wherePattern.ToAlgebra();
                if (context.Commands != null)
                {
                    where = context.Commands.ApplyAlgebraOptimisers(where);
                }

                // Set Active Graph for the WHERE based upon the WITH clause
                // Don't bother if there are USING URIs as these would override any Active Graph we set here
                // so we can save ourselves the effort of doing this
                if (!UsingUris.Any())
                {
                    if (_graphUri != null)
                    {
                        context.Data.SetActiveGraph(_graphUri);
                        defGraphOk = true;
                    }
                    else
                    {
                        context.Data.SetActiveGraph((Uri)null);
                        defGraphOk = true;
                    }
                }

                // We need to make a dummy SparqlQuery object since if the Command has used any 
                // USING/USING NAMEDs along with GRAPH clauses then the algebra needs to have the
                // URIs available to it which it gets from the Query property of the Context
                // object
                SparqlQuery query = new SparqlQuery();
                foreach (Uri u in UsingUris)
                {
                    query.AddDefaultGraph(u);
                }
                foreach (Uri u in UsingNamedUris)
                {
                    query.AddNamedGraph(u);
                }
                SparqlEvaluationContext queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor);
                if (UsingUris.Any())
                {
                    // If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                    context.Data.SetActiveGraph(_usingUris);
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
                IGraph g = context.Data.HasGraph(_graphUri) ? context.Data.GetModifiableGraph(_graphUri) : null;

                // Delete the Triples for each Solution
                foreach (ISet s in results.Sets)
                {
                    List<Triple> deletedTriples = new List<Triple>();

                    if (g != null)
                    {
                        // Triples from raw Triple Patterns
                        try
                        {
                            ConstructContext constructContext = new ConstructContext(g, s, true);
                            foreach (IConstructTriplePattern p in _deletePattern.TriplePatterns
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
                    foreach (GraphPattern gp in _deletePattern.ChildGraphPatterns)
                    {
                        deletedTriples.Clear();
                        try
                        {
                            String graphUri;
                            switch (gp.GraphSpecifier.TokenType)
                            {
                                case Token.URI:
                                    graphUri = gp.GraphSpecifier.Value;
                                    break;
                                case Token.VARIABLE:
                                    String graphVar = gp.GraphSpecifier.Value.Substring(1);
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
                                            graphUri = temp.ToSafeString();
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
                            if (!context.Data.HasGraph(UriFactory.Create(graphUri))) continue;

                            // Do the actual Deletions
                            IGraph h = context.Data.GetModifiableGraph(UriFactory.Create(graphUri));
                            ConstructContext constructContext = new ConstructContext(h, s, true);
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
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDeleteCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (_graphUri != null)
            {
                output.Append("WITH <");
                output.Append(_graphUri.AbsoluteUri.Replace(">", "\\>"));
                output.AppendLine(">");
            }
            output.AppendLine("DELETE");
            if (!ReferenceEquals(_deletePattern, _wherePattern)) output.AppendLine(_deletePattern.ToString());
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
            output.AppendLine(_wherePattern.ToString());
            return output.ToString();
        }
    }
}
