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
            this._deletePattern = deletions;
            this._wherePattern = where;
            this._graphUri = graphUri;

            //Optimise the WHERE
            this._wherePattern.Optimise(Enumerable.Empty<String>());
        }

        /// <summary>
        /// Creates a new DELETE command which operates on the Default Graph
        /// </summary>
        /// <param name="deletions">Pattern to construct Triples to delete</param>
        /// <param name="where">Pattern to select data which is then used in evaluating the deletions pattern</param>
        public DeleteCommand(GraphPattern deletions, GraphPattern where)
            : base(SparqlUpdateCommandType.Delete)
        {
            this._deletePattern = deletions;
            this._wherePattern = where;
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
        /// Gets the URI of the Graph the deletions are made from
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return this._graphUri;
            }
        }
       
        /// <summary>
        /// Gets the pattern used for Deletions
        /// </summary>
        public GraphPattern DeletePattern
        {
            get
            {
                return this._deletePattern;
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

            //Get the Graph from which we are deleting
            IGraph g = context.Data.GetModifiableGraph(this._graphUri);

            //Delet ethe Triples for each Solution
            foreach (Set s in queryContext.OutputMultiset.Sets)
            {
                List<Triple> deletedTriples = new List<Triple>();

                //Triples from raw Triple Patterns
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
            if (this._graphUri != null)
            {
                output.Append("WITH <");
                output.Append(this._graphUri.ToString().Replace(">", "\\>"));
                output.AppendLine(">");
            }
            output.AppendLine("DELETE");
            if (!ReferenceEquals(this._deletePattern, this._wherePattern)) output.AppendLine(this._deletePattern.ToString());
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
