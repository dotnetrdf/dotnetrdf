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

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update DROP command
    /// </summary>
    public class DropCommand : SparqlUpdateCommand
    {
        private Uri _graphUri;
        private ClearMode _mode = ClearMode.Graph;
        private bool _silent = false;

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI ofthe Graph to DROP</param>
        /// <param name="mode">DROP Mode to use</param>
        /// <param name="silent">Whether the DROP should be done silently</param>
        public DropCommand(Uri graphUri, ClearMode mode, bool silent)
            : base(SparqlUpdateCommandType.Drop)
        {
            this._graphUri = graphUri;
            this._mode = mode;
            if (this._graphUri == null && this._mode == ClearMode.Graph) this._mode = ClearMode.Default;
            if (this._mode == ClearMode.Default) this._graphUri = null;
            this._silent = silent;
        }

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to DROP</param>
        /// <param name="mode">DROP Mode to use</param>
        public DropCommand(Uri graphUri, ClearMode mode)
            : this(graphUri, mode, false) { }

        /// <summary>
        /// Creates a new DROP command
        /// </summary>
        /// <param name="graphUri">URI of the Graph to DROP</param>
        public DropCommand(Uri graphUri)
            : this(graphUri, ClearMode.Graph, false) { }

        /// <summary>
        /// Creates a new DROP command which drops the Default Graph
        /// </summary>
        public DropCommand()
            : this(null, ClearMode.Default) { }

        /// <summary>
        /// Creates a new DROP command which performs a specific clear mode drop operation
        /// </summary>
        /// <param name="mode">Clear Mode</param>
        public DropCommand(ClearMode mode)
            : this(mode, false) { }

        /// <summary>
        /// Creates a new DROP command which performs a specific clear mode drop operation
        /// </summary>
        /// <param name="mode">Clear Mode</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public DropCommand(ClearMode mode, bool silent)
            : this(null, mode, silent) { }

        /// <summary>
        /// Gets whether the Command affects a single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                return this._mode == ClearMode.Graph || this._mode == ClearMode.Default;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            switch (this._mode)
            {
                case ClearMode.All:
                    return true;
                case ClearMode.Default:
                    return graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri);
                case ClearMode.Named:
                    return graphUri != null && !graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri);
                case ClearMode.Graph:
                    if (this._graphUri == null)
                    {
                        return graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri);
                    }
                    else
                    {
                        return this._graphUri.ToString().Equals(graphUri.ToSafeString());
                    }
                default:
                    //No Other Clear Modes but have to keep the compiler happy
                    return true;
            }
        }

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
        /// Gets the type of DROP operation to perform
        /// </summary>
        public ClearMode Mode
        {
            get
            {
                return this._mode;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            try
            {
                switch (this._mode)
                {
                    case ClearMode.Default:
                    case ClearMode.Graph:
                        if (!context.Data.HasGraph(this._graphUri))
                        {
                            if (!this._silent) throw new SparqlUpdateException("Cannot remove a Named Graph with URI '" + this._graphUri.ToString() + "' since a Graph with this URI does not exist in the Store");
                        }
                        else
                        {
                            if (this._mode == ClearMode.Graph)
                            {
                                context.Data.RemoveGraph(this._graphUri);
                            }
                            else
                            {
                                //DROPing the DEFAULT graph only results in clearing it
                                //This is because removing the default graph may cause errors in later commands/queries
                                //which rely on it existing
                                context.Data.GetModifiableGraph(this._graphUri).Clear();
                            }
                        }
                        break;

                    case ClearMode.Named:
                        foreach (Uri u in context.Data.GraphUris.ToList())
                        {
                            if (u != null)
                            {
                                context.Data.RemoveGraph(u);
                            }
                        }
                        break;
                    case ClearMode.All:
                        foreach (Uri u in context.Data.GraphUris.ToList())
                        {
                            if (u != null)
                            {
                                context.Data.RemoveGraph(u);
                            }
                            else
                            {
                                //DROPing the DEFAULT graph only results in clearing it
                                //This is because removing the default graph may cause errors in later commands/queries
                                //which rely on it existing
                                context.Data.GetModifiableGraph(u).Clear();
                            }
                        }
                        break;
                }
            }
            catch
            {
                if (!this._silent) throw;
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
            switch (this._mode)
            {
                case ClearMode.All:
                    output.Append("ALL");
                    break;
                case ClearMode.Default:
                    output.Append("DEFAULT");
                    break;
                case ClearMode.Named:
                    output.Append("NAMED");
                    break;
                case ClearMode.Graph:
                    output.Append("GRAPH <");
                    output.Append(this._graphUri.ToString().Replace(">", "\\>"));
                    output.Append('>');
                    break;
            }
            return output.ToString();
        }

    }
}
