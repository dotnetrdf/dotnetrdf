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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update DELETE DATA command
    /// </summary>
    public class DeleteDataCommand : SparqlUpdateCommand
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new DELETE DATA command
        /// </summary>
        /// <param name="pattern">Pattern composed of concrete Triples to delete</param>
        public DeleteDataCommand(GraphPattern pattern)
            : base(SparqlUpdateCommandType.DeleteData) 
        {
            if (!pattern.TriplePatterns.All(p => p is TriplePattern && p.IndexType == TripleIndexType.NoVariables)) throw new SparqlUpdateException("Cannot create a DELETE DATA command where any of the Triple Patterns are not complete triples");
            this._pattern = pattern;
        }

        /// <summary>
        /// Gets the Data Pattern containing Triples to delete
        /// </summary>
        public GraphPattern DataPattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            if (!this._pattern.TriplePatterns.All(p => p is TriplePattern && p.IndexType == TripleIndexType.NoVariables)) throw new SparqlUpdateException("Cannot evaluate a DELETE DATA command where any of the Triple Patterns are not complete triples");

            //Get the Target Graph
            IGraph target;
            Uri graphUri;
            if (this._pattern.IsGraph)
            {
                switch (this._pattern.GraphSpecifier.TokenType)
                {
                    case Token.QNAME:
                        throw new NotImplementedException("Graph Specifiers as QNames for DELETE DATA Commands are not supported - please specify an absolute URI instead");
                    case Token.URI:
                        graphUri = new Uri(this._pattern.GraphSpecifier.Value);
                        break;
                    default:
                        throw new SparqlUpdateException("Cannot evaluate an DELETE DATA Command as the Graph Specifier is not a QName/URI");
                }
            }
            else
            {
                graphUri = null;
            }
            if (context.Data.HasGraph(graphUri))
            {
                target = context.Data.Graph(graphUri);
            }
            else
            {
                throw new SparqlUpdateException("Cannot evaluate an DELETE DATA Command since the target Graph does not exist");
            }

            //Delete the actual Triples
            INode subj, pred, obj;
            foreach (ITriplePattern p in this._pattern.TriplePatterns)
            {
                TriplePattern tp = (TriplePattern)p;
                subj = ((NodeMatchPattern)tp.Subject).Node.CopyNode(target);
                pred = ((NodeMatchPattern)tp.Predicate).Node.CopyNode(target);
                obj = ((NodeMatchPattern)tp.Object).Node.CopyNode(target);

                target.Retract(new Triple(subj, pred, obj));
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDeleteDataCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("DELETE DATA {");
            output.AppendLine(this._pattern.ToString());
            output.AppendLine("}");
            return output.ToString();
        }
    }
}
