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
    /// Represents the SPARQL Update CREATE command
    /// </summary>
    public class CreateCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;
        private bool _silent = false;

        /// <summary>
        /// Creates a new CREATE command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to create</param>
        /// <param name="silent">Whether the create should be done silenty</param>
        public CreateCommand(Uri graphUri, bool silent)
            : base(SparqlUpdateCommandType.Create) 
        {
            if (graphUri == null) throw new ArgumentNullException("graphUri");
            this._graphUri = graphUri;
            this._silent = silent;
        }

        /// <summary>
        /// Creates a new CREATE command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to create</param>
        public CreateCommand(Uri graphUri)
            : this(graphUri, false) { }

        /// <summary>
        /// Gets whether the Command affects a Single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            if (this._graphUri == null)
            {
                return graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri);
            }
            else
            {
                return this._graphUri.ToString().Equals(graphUri.ToSafeString());
            }
        }

        /// <summary>
        /// Gets the URI of the Graph to be created
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return this._graphUri;
            }
        }

        /// <summary>
        /// Gets whether the Create should be done silently
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
        /// <param name="context">Update Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            if (context.Data.HasGraph(this._graphUri))
            {
                if (!this._silent) throw new SparqlUpdateException("Cannot create a Named Graph with URI '" + this._graphUri.ToString() + "' since a Graph with this URI already exists in the Store");
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = this._graphUri;
                context.Data.AddGraph(g);
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessCreateCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("CREATE ");
            if (this._silent) output.Append("SILENT ");
            output.Append("GRAPH <");
            output.Append(this._graphUri.ToString().Replace(">", "\\>"));
            output.Append('>');
            return output.ToString();
        }
    }
}
