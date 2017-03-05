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
using System.Text;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Abstract Base Class for SPARQL Update Commands which move data between Graphs
    /// </summary>
    public abstract class BaseTransferCommand : SparqlUpdateCommand
    {
        /// <summary>
        /// Source Graph URI
        /// </summary>
        protected Uri _sourceUri;
        /// <summary>
        /// Destination Graph URI
        /// </summary>
        protected Uri _destUri;
        /// <summary>
        /// Whether errors should be suppressed
        /// </summary>
        protected bool _silent = false;

        /// <summary>
        /// Creates a new Transfer Command
        /// </summary>
        /// <param name="type">Command Type</param>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public BaseTransferCommand(SparqlUpdateCommandType type, Uri sourceUri, Uri destUri, bool silent)
            : this(type, sourceUri, destUri)
        {
            this._silent = silent;
        }

        /// <summary>
        /// Creates a new Transfer Command
        /// </summary>
        /// <param name="type">Command Type</param>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        public BaseTransferCommand(SparqlUpdateCommandType type, Uri sourceUri, Uri destUri)
            : base(type)
        {
            this._sourceUri = sourceUri;
            this._destUri = destUri;
        }

        /// <summary>
        /// URI of the Source Graph
        /// </summary>
        public Uri SourceUri
        {
            get
            {
                return this._sourceUri;
            }
        }

        /// <summary>
        /// URI of the Destination Graph
        /// </summary>
        public Uri DestinationUri
        {
            get
            {
                return this._destUri;
            }
        }

        /// <summary>
        /// Whether errors during evaluation should be suppressed
        /// </summary>
        public bool Silent
        {
            get
            {
                return this._silent;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a Single Graph
        /// </summary>
        public override bool AffectsSingleGraph
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the Command affects a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                return (this._destUri == null || this._sourceUri == null);
            }
            else
            {
                return graphUri.AbsoluteUri.Equals(this._sourceUri.ToSafeString()) || graphUri.AbsoluteUri.Equals(this._destUri.ToSafeString());
            }
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            switch (this.CommandType)
            {
                case SparqlUpdateCommandType.Add:
                    output.Append("ADD");
                    break;
                case SparqlUpdateCommandType.Copy:
                    output.Append("COPY");
                    break;
                case SparqlUpdateCommandType.Move:
                    output.Append("MOVE");
                    break;
                default:
                    throw new RdfException("Cannot display the String for this Transfer command as it is not one of the valid transfer commands (ADD/COPY/MOVE)");
            }

            if (this._silent) output.Append(" SILENT");

            if (this._sourceUri == null)
            {
                output.Append(" DEFAULT");
            }
            else
            {
                output.Append(" GRAPH <" + this._sourceUri.AbsoluteUri.Replace(">", "\\>") + ">");
            }
            output.Append(" TO ");
            if (this._destUri == null)
            {
                output.Append(" DEFAULT");
            }
            else
            {
                output.Append(" GRAPH <" + this._destUri.AbsoluteUri.Replace(">", "\\>") + ">");
            }

            return output.ToString();
        }
    }
}
