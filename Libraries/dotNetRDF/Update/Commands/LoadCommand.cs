/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// Represents the SPARQL Update LOAD command.
    /// </summary>
    public class LoadCommand : SparqlUpdateCommand
    {
        private readonly Loader _loader;

        /// <summary>
        /// Creates a new LOAD command.
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from.</param>
        /// <param name="graphUri">Target URI for the Graph to store data in.</param>
        /// <param name="silent">Whether errors loading should be suppressed.</param>
        /// <param name="loader">The loader to use for retrieving and parsing data.</param>
        public LoadCommand(Uri sourceUri, Uri graphUri, bool silent, Loader loader = null)
            : base(SparqlUpdateCommandType.Load) 
        {
            if (sourceUri == null) throw new ArgumentNullException(nameof(sourceUri));
            SourceUri = sourceUri;
            TargetUri = graphUri;
            Silent = silent;
            _loader = loader ?? new Loader();
        }

        /// <summary>
        /// Creates a new LOAD command.
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from.</param>
        /// <param name="silent">Whether errors loading should be suppressed.</param>
        public LoadCommand(Uri sourceUri, bool silent)
            : this(sourceUri, null, silent) { }

        /// <summary>
        /// Creates a new LOAD command.
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from.</param>
        /// <param name="targetUri">Target URI for the Graph to store data in.</param>
        public LoadCommand(Uri sourceUri, Uri targetUri)
            : this(sourceUri, targetUri, false) { }

        /// <summary>
        /// Creates a new LOAD command which operates on the Default Graph.
        /// </summary>
        /// <param name="sourceUri">Source URI to load data from.</param>
        public LoadCommand(Uri sourceUri)
            : this(sourceUri, null) { }

        /// <summary>
        /// Gets whether the Command affects a specific Graph.
        /// </summary>
        public override bool AffectsSingleGraph => true;

        /// <summary>
        /// Gets whether the Command affects a given Graph.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <returns></returns>
        public override bool AffectsGraph(Uri graphUri)
        {
            if (TargetUri == null)
            {
                return true;
            }
            return TargetUri.AbsoluteUri.Equals(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets the URI that data is loaded from.
        /// </summary>
        public Uri SourceUri { get; }

        /// <summary>
        /// Gets the URI of the Graph to load data into.
        /// </summary>
        public Uri TargetUri { get; }

        /// <summary>
        /// Gets whether errors loading the data are suppressed.
        /// </summary>
        public bool Silent { get; }

        /// <summary>
        /// Evaluates the Command in the given Context.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
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
                var g = new Graph();
                _loader.LoadGraph(g, SourceUri);

                if (context.Data.HasGraph(TargetUri))
                {
                    // Merge the Data into the existing Graph
                    context.Data.GetModifiableGraph(TargetUri).Merge(g);
                }
                else
                {
                    // Add New Graph to the Dataset
                    g.BaseUri = TargetUri;
                    context.Data.AddGraph(g);
                }
            }
            catch
            {
                if (!Silent) throw;
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor.
        /// </summary>
        /// <param name="processor">SPARQL Update Processor.</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessLoadCommand(this);
        }

        /// <summary>
        /// Gets the String representation of the Command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var silent = Silent ? "SILENT " : string.Empty;
            if (TargetUri == null)
            {
                return "LOAD " + silent + "<" + SourceUri.AbsoluteUri.Replace(">", "\\>") + ">";
            }
            else
            {
                return "LOAD " + silent + "<" + SourceUri.AbsoluteUri.Replace(">", "\\>") + "> INTO <" + TargetUri.AbsoluteUri.Replace(">", "\\>") + ">";
            }
        }
    }
}
