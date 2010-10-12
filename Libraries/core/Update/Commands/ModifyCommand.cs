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
            this._deletePattern = deletions;
            this._insertPattern = insertions;
            this._wherePattern = where;
            this._graphUri = graphUri;
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
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            //First evaluate the WHERE pattern to get the affected bindings
            ISparqlAlgebra where = this._wherePattern.ToAlgebra();
            SparqlEvaluationContext queryContext = new SparqlEvaluationContext(null, context.Data);
            if (this.UsingUris.Any()) context.Data.SetActiveGraph(this._usingUris);
            BaseMultiset results = where.Evaluate(queryContext);
            if (this.UsingUris.Any()) context.Data.ResetActiveGraph();

            //Get the Graph to which we are deleting and inserting
            IGraph g = context.Data.GetModifiableGraph(this._graphUri);

            //Delete the Triples for each Solution
            List<Triple> deletedTriples = new List<Triple>();
            foreach (Set s in queryContext.OutputMultiset.Sets)
            {
                try
                {
                    ConstructContext constructContext = new ConstructContext(g, s, true);
                    foreach (ITriplePattern p in this._deletePattern.TriplePatterns)
                    {
                        deletedTriples.Add(((IConstructTriplePattern)p).Construct(constructContext));
                    }
                    g.Retract(deletedTriples);
                }
                catch (RdfQueryException)
                {
                    //If we throw an error this means we couldn't construct for this solution so the
                    //solution is discarded
                    continue;
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
                                    if (temp.NodeType == NodeType.Uri)
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
                        IGraph h = context.Data.GetModifiableGraph(new Uri(graphUri));
                        ConstructContext constructContext = new ConstructContext(h, s, true);
                        foreach (ITriplePattern p in gp.TriplePatterns)
                        {
                            deletedTriples.Add(((IConstructTriplePattern)p).Construct(constructContext));
                        }
                        h.Retract(deletedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        //If we throw an error this means we couldn't construct for this solution so the
                        //solution is discarded
                        continue;
                    }
                }
            }

            //Insert the Triples for each Solution
            foreach (Set s in queryContext.OutputMultiset.Sets)
            {
                List<Triple> insertedTriples = new List<Triple>();
                try
                {
                    ConstructContext constructContext = new ConstructContext(g, s, true);
                    foreach (ITriplePattern p in this._insertPattern.TriplePatterns)
                    {
                        insertedTriples.Add(((IConstructTriplePattern)p).Construct(constructContext));
                    }
                    g.Assert(insertedTriples);
                }
                catch (RdfQueryException)
                {
                    //If we throw an error this means we couldn't construct for this solution so the
                    //solution is discarded
                    continue;
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
                                    if (temp.NodeType == NodeType.Uri)
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
                        IGraph h = context.Data.GetModifiableGraph(new Uri(graphUri));
                        ConstructContext constructContext = new ConstructContext(h, s, true);
                        foreach (ITriplePattern p in gp.TriplePatterns)
                        {
                            insertedTriples.Add(((IConstructTriplePattern)p).Construct(constructContext));
                        }
                        h.Assert(insertedTriples);
                    }
                    catch (RdfQueryException)
                    {
                        //If we throw an error this means we couldn't construct for this solution so the
                        //solution is discarded
                        continue;
                    }
                }
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
            output.AppendLine("WHERE");
            output.AppendLine(this._wherePattern.ToString());
            return output.ToString();
        }
    }
}
