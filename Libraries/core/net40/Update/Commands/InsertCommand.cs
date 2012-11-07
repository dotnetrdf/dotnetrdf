/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update INSERT command
    /// </summary>
    public class InsertCommand 
        : BaseModificationCommand
    {
        private GraphPattern _insertPattern, _wherePattern;

        /// <summary>
        /// Creates a new INSERT command
        /// </summary>
        /// <param name="insertions">Pattern to construct Triples to insert</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the insertions</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        public InsertCommand(GraphPattern insertions, GraphPattern where, Uri graphUri)
            : base(SparqlUpdateCommandType.Insert) 
        {
            this._insertPattern = insertions;
            this._wherePattern = where;
            this._graphUri = graphUri;
        }

        /// <summary>
        /// Creates a new INSERT command which operates on the Default Graph
        /// </summary>
        /// <param name="insertions">Pattern to construct Triples to insert</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the insertions</param>
        public InsertCommand(GraphPattern insertions, GraphPattern where)
            : this(insertions, where, null) { }

        /// <summary>
        /// Gets whether the Command affects a single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                List<String> affectedUris = new List<string>();
                if (this.TargetUri != null)
                {
                    affectedUris.Add(this.TargetUri.AbsoluteUri);
                }
                if (this._insertPattern.IsGraph) affectedUris.Add(this._insertPattern.GraphSpecifier.Value);
                if (this._insertPattern.HasChildGraphPatterns)
                {
                    affectedUris.AddRange(from p in this._insertPattern.ChildGraphPatterns
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
            if (this.TargetUri != null)
            {
                affectedUris.Add(this.TargetUri.AbsoluteUri);
            }
            else
            {
                affectedUris.Add(String.Empty);
            }
            if (this._insertPattern.IsGraph) affectedUris.Add(this._insertPattern.GraphSpecifier.Value);
            if (this._insertPattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in this._insertPattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null)) affectedUris.Add(String.Empty);

            return affectedUris.Contains(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets the URI of the Graph the insertions are made to
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return this._graphUri;
            }
        }

        /// <summary>
        /// Gets the pattern used for insertions
        /// </summary>
        public GraphPattern InsertPattern
        {
            get
            {
                return this._insertPattern;
            }
        }

        /// <summary>
        /// Gets the pattern used for the WHERE clause
        /// </summary>
        public GraphPattern WherePattern
        {
            get
            {
                return this._wherePattern;
            }
        }

        /// <summary>
        /// Optimises the Commands WHERE pattern
        /// </summary>
        public override void Optimise(IQueryOptimiser optimiser)
        {
            this._wherePattern.Optimise(optimiser);
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            bool datasetOk = false;
            bool defGraphOk = false;

            try
            {
                //First evaluate the WHERE pattern to get the affected bindings
                ISparqlAlgebra where = this._wherePattern.ToAlgebra();
                if (context.Commands != null)
                {
                    where = context.Commands.ApplyAlgebraOptimisers(where);
                }

                //Set Active Graph for the WHERE
                //Don't bother if there are USING URIs as these would override any Active Graph we set here
                //so we can save ourselves the effort of doing this
                if (!this.UsingUris.Any())
                {
                    if (this._graphUri != null)
                    {
                        context.Data.SetActiveGraph(this._graphUri);
                        defGraphOk = true;
                    }
                    else
                    {
                        context.Data.SetActiveGraph((Uri)null);
                        defGraphOk = true;
                    }
                }

                //We need to make a dummy SparqlQuery object since if the Command has used any 
                //USING NAMEDs along with GRAPH clauses then the algebra needs to have the
                //URIs available to it which it gets from the Query property of the Context
                //object
                SparqlQuery query = new SparqlQuery();
                foreach (Uri u in this.UsingUris)
                {
                    query.AddDefaultGraph(u);
                }
                foreach (Uri u in this.UsingNamedUris)
                {
                    query.AddNamedGraph(u);
                }
                SparqlEvaluationContext queryContext = new SparqlEvaluationContext(query, context.Data, context.QueryProcessor);
                if (this.UsingUris.Any())
                {
                    //If there are USING URIs set the Active Graph to be formed of the Graphs with those URIs
                    context.Data.SetActiveGraph(this._usingUris);
                    datasetOk = true;
                }
                BaseMultiset results = queryContext.Evaluate(where);
                if (results is IdentityMultiset) queryContext.OutputMultiset = new SingletonMultiset(results.Variables);
                if (this.UsingUris.Any())
                {
                    //If there are USING URIs reset the Active Graph afterwards
                    //Also flag the dataset as no longer being OK as this flag is used in the finally 
                    //block to determine whether the Active Graph needs resetting which it may do if the
                    //evaluation of the 
                    context.Data.ResetActiveGraph();
                    datasetOk = false;
                }

                //Reset Active Graph for the WHERE
                if (defGraphOk)
                {
                    context.Data.ResetActiveGraph();
                    defGraphOk = false;
                }

                //TODO: Need to detect when we create a Graph for Insertion but then fail to insert anything since in this case the Inserted Graph should be removed

                //Get the Graph to which we are inserting Triples with no explicit Graph clause
                IGraph g = null;
                if (this._insertPattern.TriplePatterns.Count > 0)
                {
                    if (context.Data.HasGraph(this._graphUri))
                    {
                        g = context.Data.GetModifiableGraph(this._graphUri);
                    }
                    else
                    {
                        //insertedGraphs.Add(this._graphUri);
                        g = new Graph();
                        g.BaseUri = this._graphUri;
                        context.Data.AddGraph(g);
                        g = context.Data.GetModifiableGraph(this._graphUri);
                    }
                }

                //Keep a record of graphs to which we insert
                MultiDictionary<Uri, IGraph> graphs = new MultiDictionary<Uri, IGraph>(u => u.GetEnhancedHashCode(), new UriComparer(), MultiDictionaryMode.AVL);

                //Insert the Triples for each Solution
                foreach (ISet s in queryContext.OutputMultiset.Sets)
                {
                    List<Triple> insertedTriples = new List<Triple>();

                    try
                    {
                        //Create a new Construct Context for each Solution
                        ConstructContext constructContext = new ConstructContext(null, s, true);

                        //Triples from raw Triple Patterns
                        if (this._insertPattern.TriplePatterns.Count > 0)
                        {
                            foreach (IConstructTriplePattern p in this._insertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    insertedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    //If we throw an error this means we couldn't construct a specific Triple
                                    //so we continue anyway
                                }
                            }
                            g.Assert(insertedTriples);
                        }

                        //Triples from GRAPH clauses
                        foreach (GraphPattern gp in this._insertPattern.ChildGraphPatterns)
                        {
                            insertedTriples.Clear();
                            try
                            {
                                String graphUri;
                                switch (gp.GraphSpecifier.TokenType)
                                {
                                    case Token.URI:
                                        graphUri = gp.GraphSpecifier.Value;
                                        break;
                                    case Token.VARIABLE:
                                        if (s.ContainsVariable(gp.GraphSpecifier.Value))
                                        {
                                            INode temp = s[gp.GraphSpecifier.Value.Substring(1)];
                                            if (temp == null)
                                            {
                                                //If the Variable is not bound then skip
                                                continue;
                                            }
                                            else if (temp.NodeType == NodeType.Uri)
                                            {
                                                graphUri = temp.ToSafeString();
                                            }
                                            else
                                            {
                                                //If the Variable is not bound to a URI then skip
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            //If the Variable is not bound for this solution then skip
                                            continue;
                                        }
                                        break;
                                    default:
                                        //Any other Graph Specifier we have to ignore this solution
                                        continue;
                                }

                                //Ensure the Graph we're inserting to exists in the dataset creating it if necessary
                                IGraph h;
                                Uri destUri = UriFactory.Create(graphUri);
                                if (graphs.ContainsKey(destUri))
                                {
                                    h = graphs[destUri];
                                }
                                else
                                {
                                    if (context.Data.HasGraph(destUri))
                                    {
                                        h = context.Data.GetModifiableGraph(destUri);
                                    }
                                    else
                                    {
                                        //insertedGraphs.Add(destUri);
                                        h = new Graph();
                                        h.BaseUri = destUri;
                                        context.Data.AddGraph(h);
                                        h = context.Data.GetModifiableGraph(destUri);
                                    }
                                    graphs.Add(destUri, h);
                                }

                                //Do the actual Insertions
                                foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                                {
                                    try
                                    {
                                        insertedTriples.Add(p.Construct(constructContext));
                                    }
                                    catch (RdfQueryException)
                                    {
                                        //If we throw an error this means we couldn't construct a specific Triple
                                        //so we continue anyway
                                    }
                                }
                                h.Assert(insertedTriples);
                            }
                            catch (RdfQueryException)
                            {
                                //If we throw an error this means we couldn't construct for this solution so the
                                //solution is ignored for this Graph
                            }
                        }
                    }
                    catch (RdfQueryException)
                    {
                        //If we throw an error this means we couldn't construct for this solution so the
                        //solution is ignored for this graph
                    }
                }
            }
            finally
            {
                //If the Dataset was set and an error occurred in doing the WHERE clause then
                //we'll need to Reset the Active Graph
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
            processor.ProcessInsertCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._graphUri != null)
            {
                output.Append("WITH <");
                output.Append(this._graphUri.AbsoluteUri.Replace(">", "\\>"));
                output.AppendLine(">");
            }
            output.AppendLine("INSERT");
            output.AppendLine(this._insertPattern.ToString());
            if (this._usingUris != null)
            {
                foreach (Uri u in this._usingUris)
                {
                    output.AppendLine("USING <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
                }
            }
            if (this._usingNamedUris != null)
            {
                foreach (Uri u in this._usingNamedUris)
                {
                    output.AppendLine("USING NAMED <" + u.AbsoluteUri.Replace(">", "\\>") + ">");
                }
            }
            output.AppendLine("WHERE");
            output.AppendLine(this._wherePattern.ToString());
            return output.ToString();
        }
    }
}
