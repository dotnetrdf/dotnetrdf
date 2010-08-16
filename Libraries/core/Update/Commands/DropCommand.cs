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
using System.Text;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update DROP command
    /// </summary>
    public class DropCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;
        private bool _silent = false;

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI ofthe Graph to DROP</param>
        /// <param name="silent">Whether the DROP should be done silently</param>
        public DropCommand(Uri graphUri, bool silent)
            : base(SparqlUpdateCommandType.Drop)
        {
            this._graphUri = graphUri;
            this._silent = silent;
        }

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to DROP</param>
        public DropCommand(Uri graphUri)
            : this(graphUri, false) { }

        /// <summary>
        /// Gets the URI of the Graph to be dropped
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return this._graphUri;
            }
        }

        /// <summary>
        /// Gets whether the Drop should be done silently
        /// </summary>
        public bool Silent
        {
            get
            {
                return this._silent;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            if (!context.Data.HasGraph(this._graphUri))
            {
                if (!this._silent) throw new SparqlUpdateException("Cannot remove a Named Graph with URI '" + this._graphUri.ToString() + "' since a Graph with this URI does not exist in the Store");
            }
            else
            {
                context.Data.Remove(this._graphUri);
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessDropCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("DROP ");
            if (this._silent) output.Append("SILENT ");
            output.Append("GRAPH <");
            output.Append(this._graphUri.ToString().Replace(">", "\\>"));
            output.Append('>');
            return output.ToString();
        }

    }
}
