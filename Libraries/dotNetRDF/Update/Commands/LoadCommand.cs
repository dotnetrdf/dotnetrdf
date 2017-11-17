/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Parsing;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents the SPARQL Update LOAD command
    /// </summary>
    public class LoadCommand : SparqlUpdateCommand
    {
        private readonly Uri _sourceUri, _graphUri;
        private readonly bool _silent = false;

        /// <summary>
        /// Creates a new LOAD command
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from</param>
        /// <param name="graphUri">Target URI for the Graph to store data in</param>
        /// <param name="silent">Whether errors loading should be suppressed</param>
        public LoadCommand(Uri sourceUri, Uri graphUri, bool silent)
            : base(SparqlUpdateCommandType.Load) 
        {
            if (sourceUri == null) throw new ArgumentNullException("sourceUri");
            _sourceUri = sourceUri;
            _graphUri = graphUri;
            _silent = silent;
        }

        /// <summary>
        /// Creates a new LOAD command
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from</param>
        /// <param name="silent">Whether errors loading should be suppressed</param>
        public LoadCommand(Uri sourceUri, bool silent)
            : this(sourceUri, null, silent) { }

        /// <summary>
        /// Creates a new LOAD command
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from</param>
        /// <param name="targetUri">Target URI for the Graph to store data in</param>
        public LoadCommand(Uri sourceUri, Uri targetUri)
            : this(sourceUri, targetUri, false) { }

        /// <summary>
        /// Creates a new LOAD command which operates on the Default Graph
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from</param>
        public LoadCommand(Uri sourceUri)
            : this(sourceUri, null) { }

        /// <summary>
        /// Gets whether the Command affects a specific Graph
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
            if (_graphUri == null)
            {
                return true;
            }
            return _graphUri.AbsoluteUri.Equals(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets the URI that data is loaded from
        /// </summary>
        public Uri SourceUri
        {
            get
            {
                return _sourceUri;
            }
        }

        /// <summary>
        /// Gets the URI of the Graph to load data into
        /// </summary>
        public Uri TargetUri
        {
            get
            {
                return _graphUri;
            }
        }

        /// <summary>
        /// Gets whether errors loading the data are suppressed
        /// </summary>
        public bool Silent
        {
            get
            {
                return _silent;
            }
        }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            // Q: Does LOAD into a named Graph require that Graph to be pre-existing?
            // if (this._graphUri != null)
            // {
            //    //When adding to specific Graph need to ensure that Graph exists
            //    //In the case when we're adding to the default graph we'll create it if it doesn't exist
            //    if (!context.Data.HasGraph(this._graphUri))
            //    {
            //        throw new RdfUpdateException("Cannot LOAD into a Graph that does not exist in the Store");
            //    }
            // }

            try
            {
                // Load from the URI
                Graph g = new Graph();
                UriLoader.Load(g, _sourceUri);

                if (context.Data.HasGraph(_graphUri))
                {
                    // Merge the Data into the existing Graph
                    context.Data.GetModifiableGraph(_graphUri).Merge(g);
                }
                else
                {
                    // Add New Graph to the Dataset
                    g.BaseUri = _graphUri;
                    context.Data.AddGraph(g);
                }
            }
            catch
            {
                if (!_silent) throw;
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessLoadCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String silent = (_silent) ? "SILENT " : String.Empty;
            if (_graphUri == null)
            {
                return "LOAD " + silent + "<" + _sourceUri.AbsoluteUri.Replace(">", "\\>") + ">";
            }
            else
            {
                return "LOAD " + silent + "<" + _sourceUri.AbsoluteUri.Replace(">", "\\>") + "> INTO <" + _graphUri.AbsoluteUri.Replace(">", "\\>") + ">";
            }
        }
    }
}
