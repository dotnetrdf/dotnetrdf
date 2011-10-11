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
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update INSERT/DELETE command
    /// </summary>
    public class ModifyCommand : BaseModificationCommand
    {
        private GraphPattern _deletePattern, _insertPattern, _wherePattern;

        /// <summary>
        /// Creates a new INSERT/DELETE command
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete</param>
        /// <param name="insertions">Pattern to construct Triples to insert</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the insertions and deletions</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        public ModifyCommand(GraphPattern deletions, GraphPattern insertions, GraphPattern where, Uri graphUri)
            : base(SparqlUpdateCommandType.Modify)
        {
            if (!this.IsValidDeletePattern(deletions, true)) throw new SparqlUpdateException("Cannot create a DELETE command where any of the Triple Patterns are not constructable triple patterns (Blank Node Variables are not permitted) or a GRAPH clause has nested Graph Patterns");

            this._deletePattern = deletions;
            this._insertPattern = insertions;
            this._wherePattern = where;
            this._graphUri = graphUri;

            //Optimise the WHERE
            this._wherePattern.Optimise();
        }

        /// <summary>
        /// Creates a new INSERT/DELETE command which operates on the Default Graph
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete</param>
        /// <param name="insertions">Pattern to construct Triples to insert</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the insertions and deletions</param>
        public ModifyCommand(GraphPattern deletions, GraphPattern insertions, GraphPattern where)
            : this(deletions, insertions, where, null) { }

        /// <summary>
        /// Gets whether the Command affects a Single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                List<String> affectedUris = new List<string>();
                if (this.TargetUri != null)
                {
                    affectedUris.Add(this.TargetUri.ToString());
                }
                if (this._deletePattern.IsGraph) affectedUris.Add(this._deletePattern.GraphSpecifier.Value);
                if (this._deletePattern.HasChildGraphPatterns)
                {
                    affectedUris.AddRange(from p in this._deletePattern.ChildGraphPatterns
                                          where p.IsGraph
                                          select p.GraphSpecifier.Value);
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
            if (graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri)) graphUri = null;

            List<String> affectedUris = new List<string>();
            if (this.TargetUri != null)
            {
                affectedUris.Add(this.TargetUri.ToString());
            }
            else
            {
                affectedUris.Add(String.Empty);
            }
            if (this._deletePattern.IsGraph) affectedUris.Add(this._deletePattern.GraphSpecifier.Value);
            if (this._deletePattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in this._deletePattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (this._insertPattern.IsGraph) affectedUris.Add(this._insertPattern.GraphSpecifier.Value);
            if (this._insertPattern.HasChildGraphPatterns)
            {
                affectedUris.AddRange(from p in this._insertPattern.ChildGraphPatterns
                                      where p.IsGraph
                                      select p.GraphSpecifier.Value);
            }
            if (affectedUris.Any(u => u != null && u.Equals(GraphCollection.DefaultGraphUri))) affectedUris.Add(String.Empty);

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
        /// Gets the pattern used for deletions
        /// </summary>
        public GraphPattern DeletePattern
        {
            get
            {
                return this._deletePattern;
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

                //Get the Graph to which we are deleting and inserting
                IGraph g;
                if (context.Data.HasGraph(this._graphUri))
                {
                    g = context.Data.GetModifiableGraph(this._graphUri);
                }
                else
                {
                    g = null;
                }

                //Delete the Triples for each Solution
                List<Triple> deletedTriples = new List<Triple>();
                foreach (ISet s in queryContext.OutputMultiset.Sets)
                {
                    try
                    {
                        //If the Default Graph is non-existent then Deletions have no effect on it
                        if (g != null)
                        {
                            ConstructContext constructContext = new ConstructContext(g, s, true);
                            foreach (IConstructTriplePattern p in this._deletePattern.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    deletedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    //If we get an error here then we couldn't construct a specific
                                    //triple so we continue anyway
                                }
                            }
                            g.Retract(deletedTriples);
                        }
                    }
                    catch (RdfQueryException)
                    {
                        //If we get an error here this means we couldn't construct for this solution so the
                        //solution is ignored for this graph
                    }

                    //Triples from GRAPH clauses
                    foreach (GraphPattern gp in this._deletePattern.ChildGraphPatterns)
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

                            //If the Dataset doesn't contain the Graph then no need to do the Deletions
                            if (!context.Data.HasGraph(new Uri(graphUri))) continue;

                            //Do the actual Deletions
                            IGraph h = context.Data.GetModifiableGraph(new Uri(graphUri));
                            ConstructContext constructContext = new ConstructContext(h, s, true);
                            foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    deletedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    //If we get an error here then we couldn't construct a specific triple
                                    //so we continue anyway
                                }
                            }
                            h.Retract(deletedTriples);
                        }
                        catch (RdfQueryException)
                        {
                            //If we get an error here this means we couldn't construct for this solution so the
                            //solution is ignore for this graph
                        }
                    }
                }

                //Insert the Triples for each Solution
                foreach (ISet s in queryContext.OutputMultiset.Sets)
                {
                    List<Triple> insertedTriples = new List<Triple>();
                    try
                    {
                        ConstructContext constructContext = new ConstructContext(g, s, true);
                        foreach (IConstructTriplePattern p in this._insertPattern.TriplePatterns.OfType<IConstructTriplePattern>())
                        {
                            try
                            {
                                insertedTriples.Add(p.Construct(constructContext));
                            }
                            catch (RdfQueryException)
                            {
                                //If we get an error here then we couldn't construct a specific triple
                                //so we continue anyway
                            }
                        }
                        g.Assert(insertedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        //If we get an error here this means we couldn't construct for this solution so the
                        //solution is ignored for this graph
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
                            Uri destUri = new Uri(graphUri);
                            if (context.Data.HasGraph(destUri))
                            {
                                h = context.Data.GetModifiableGraph(destUri);
                            }
                            else
                            {
                                h = new Graph();
                                h.BaseUri = destUri;
                                context.Data.AddGraph(h);
                                h = context.Data.GetModifiableGraph(destUri);
                            }

                            //Do the actual Insertions
                            ConstructContext constructContext = new ConstructContext(h, s, true);
                            foreach (IConstructTriplePattern p in gp.TriplePatterns.OfType<IConstructTriplePattern>())
                            {
                                try
                                {
                                    insertedTriples.Add(p.Construct(constructContext));
                                }
                                catch (RdfQueryException)
                                {
                                    //If we get an error here this means we couldn't construct a specific
                                    //triple so we continue anyway
                                }
                            }
                            h.Assert(insertedTriples);
                        }
                        catch (RdfQueryException)
                        {
                            //If we get an error here this means we couldn't construct for this solution so the
                            //solution is ignored for this graph
                        }
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
            processor.ProcessModifyCommand(this);
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
                output.Append(this._graphUri.ToString().Replace(">", "\\>"));
                output.AppendLine(">");
            }
            output.AppendLine("DELETE");
            output.AppendLine(this._deletePattern.ToString());
            output.AppendLine("INSERT");
            output.AppendLine(this._insertPattern.ToString());
            if (this._usingUris != null)
            {
                foreach (Uri u in this._usingUris)
                {
                    output.AppendLine("USING <" + u.ToString().Replace(">", "\\>") + ">");
                }
            }
            if (this._usingNamedUris != null)
            {
                foreach (Uri u in this._usingNamedUris)
                {
                    output.AppendLine("USING NAMED <" + u.ToString().Replace(">", "\\>") + ">");
                }
            }
            output.AppendLine("WHERE");
            output.AppendLine(this._wherePattern.ToString());
            return output.ToString();
        }
    }
}
