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

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update CLEAR command
    /// </summary>
    public class ClearCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;

        /// <summary>
        /// Creates a Command which clears the given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public ClearCommand(Uri graphUri)
            : base(SparqlUpdateCommandType.Clear)
        {
            this._graphUri = graphUri;
        }

        /// <summary>
        /// Creates a Command which clears the Default Graph (if any)
        /// </summary>
        public ClearCommand()
            : this(null) { }

        /// <summary>
        /// Gets the URI of the Graph to be cleared (or null if the default graph should be cleared)
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return this._graphUri;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context"></param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            //Q: Throw an error if the Graph doesn't exist?
            if (context.Data.HasGraph(this._graphUri))
            {
                context.Data.Graph(this._graphUri).Clear();
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessClearCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._graphUri == null)
            {
                return "CLEAR GRAPH DEFAULT";
            }
            else
            {
                return "CLEAR GRAPH <" + this._graphUri.ToString().Replace(">", "\\>") + ">";
            }
        }
    }
}
