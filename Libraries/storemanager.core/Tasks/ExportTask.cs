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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for exporting data from a Store
    /// </summary>
    public class ExportTask
        : CancellableTask<TaskResult>
    {
        private readonly String _file;
        private readonly IStorageProvider _manager;

        /// <summary>
        /// Creates a new Export Task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="file">File to export to</param>
        public ExportTask(IStorageProvider manager, String file)
            : base("Export Store") 
        {
            if (file == null) throw new ArgumentNullException("file", "Cannot Export the Store to a null File");
            this._file = file;
            this._manager = manager;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override TaskResult RunTaskInternal()
        {
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.GetTrueFileExtension(this._file)).FirstOrDefault(d => d.CanWriteRdfDatasets);
            if (def == null)
            {
                throw new RdfOutputException("Cannot Export the Store to the selected File since dotNetRDF was unable to select a writer to use based on the File Extension");
            }

            IStoreWriter writer = def.GetRdfDatasetWriter();
            if (writer is IMultiThreadedWriter)
            {
                ((IMultiThreadedWriter)writer).UseMultiThreadedWriting = false;
            }

            TripleStore store = new TripleStore();
            List<Uri> graphUris = this.ListGraphs().ToList();

            if (writer is TriXWriter)
            {
                //For TriX must load all into memory and then write out all at once

                // Make sure we always include the default graph in the export
                if (!graphUris.Contains(null)) graphUris.Add(null);

                foreach (Uri u in graphUris)
                {
                    Graph g = new Graph();
                    this._manager.LoadGraph(g, u);
                    g.BaseUri = u;
                    store.Add(g);
                    this.Information = "Loading into memory prior to export, loaded " + store.Graphs.Sum(x => x.Triples.Count) + " Triple(s) in " + store.Graphs.Count + " Graph(s) so far...";
                    if (this.HasBeenCancelled)
                    {
                        this.Information = "Export Cancelled";
                        return new TaskResult(true);
                    }
                }
                this.Information = "Exporting Data all at once, have " + store.Graphs.Sum(x => x.Triples.Count) + " Triple(s) in " + store.Graphs.Count + " Graph(s) to export...";
                writer.Save(store, new StreamWriter(this._file));
                this.Information = "Exported " + store.Graphs.Sum(x => x.Triples.Count) + " Triple(s) in " + store.Graphs.Count + " Graph(s)";
            }
            else
            {
                if (File.Exists(this._file)) File.Delete(this._file);

                //For non-TriX formats assume it is safe to append one Graph at a time to the file
                int graphCount = 0, tripleCount = 0;
                
                // Make sure we always include the default graph in the export
                if (!graphUris.Contains(null)) graphUris.Add(null);

                // Write each graph out
                foreach (Uri u in graphUris)
                {
                    using (FileStream stream = new FileStream(this._file, FileMode.Append))
                    {
                        if (writer is IFormatterBasedWriter)
                        {
                            //Stream via a WriteThroughHandler
                            this.Information = "Stream Exporting Graph " + (u != null ? u.AbsoluteUri : "Default");
                            IRdfHandler handler = new WriteThroughHandler(((IFormatterBasedWriter)writer).TripleFormatterType, new StreamWriter(stream), true);
                            if (u != null) handler = new GraphUriRewriteHandler(handler, u);
                            ExportProgressHandler progHandler = new ExportProgressHandler(handler, this, tripleCount);
                            this._manager.LoadGraph(progHandler, u);
                            graphCount++;
                            tripleCount = progHandler.TripleCount;

                            this.Information = "Finished Stream Exporting Graph " + (u != null ? u.AbsoluteUri : "Default") + ", exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s) so far...";
                        }
                        else
                        {
                            //Load Graph into memory
                            Graph g = new Graph();
                            g.BaseUri = u;
                            this.Information = "Loading Graph " + (u != null ? u.AbsoluteUri : "Default");
                            this._manager.LoadGraph(g, u);
                            g.BaseUri = u;

                            if (this.HasBeenCancelled)
                            {
                                stream.Close();
                                this.Information = "Export Cancelled, exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s)";
                                return new TaskResult(true);
                            }

                            graphCount++;
                            tripleCount += g.Triples.Count;

                            //Save it
                            store.Add(g);
                            writer.Save(store, new StreamWriter(stream, def.Encoding));
                            store.Remove(u);

                            this.Information = "Exporting Data graph by graph, exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s) so far...";
                        }

                        //Check for cancellation
                        if (this.HasBeenCancelled)
                        {
                            stream.Close();
                            this.Information = "Export Cancelled, exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s)";
                            return new TaskResult(true);
                        }
                    }
                }
                this.Information = "Exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s)";
            }
 
            return new TaskResult(true);
        }

        private IEnumerable<Uri> ListGraphs()
        {
            if (this._manager.ListGraphsSupported)
            {
                return this._manager.ListGraphs();
            }
            if (!(this._manager is IQueryableStorage)) throw new RdfStorageException("Store does not support listing Graphs so unable to do a Graph by Graph export");

            List<Uri> uris = new List<Uri>();
            Object results = ((IQueryableStorage)this._manager).Query("SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}");
            if (!(results is SparqlResultSet)) throw new RdfStorageException("Store failed to list graphs so unable to do a Graph by Graph export");
            SparqlResultSet rset = (SparqlResultSet)results;
            foreach (SparqlResult res in rset)
            {
                if (res["g"] != null && res["g"].NodeType == NodeType.Uri)
                {
                    uris.Add(((IUriNode)res["g"]).Uri);
                }
            }
            return uris;
        }
    }

    /// <summary>
    /// Handler for monitoring the progress of export operations
    /// </summary>
    class ExportProgressHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private readonly IRdfHandler _handler;
        private readonly ExportTask _task;

        /// <summary>
        /// Creates a new Export Progress handler
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <param name="task">Export Task</param>
        /// <param name="initCount">Initial Count</param>
        public ExportProgressHandler(IRdfHandler handler, ExportTask task, int initCount)
        {
            this._handler = handler;
            this._task = task;
            this.TripleCount = initCount;
        }

        /// <summary>
        /// Gets the Inner Handlers
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets whether the Handler accepts all
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Starts handling RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
        }

        /// <summary>
        /// Ends handling RDF
        /// </summary>
        /// <param name="ok">Whether parsing completed OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles Base URI
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles Namespace declarations
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles Triples
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this.TripleCount++;
            if (this.TripleCount % 1000 == 0)
            {
                if (this._task.HasBeenCancelled) return false;
                this._task.Information = "Exported " + this.TripleCount + " triples so far...";
            }
            return this._handler.HandleTriple(t);
        }

        /// <summary>
        /// Gets the count of triples seen so far
        /// </summary>
        public int TripleCount { get; private set; }
    }
}
