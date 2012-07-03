/*

Copyright Robert Vesse 2009-12
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
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class ExportTask
        : CancellableTask<TaskResult>
    {
        private String _file;
        private IStorageProvider _manager;

        public ExportTask(IStorageProvider manager, String file)
            : base("Export Store") 
        {
            if (file == null) throw new ArgumentNullException("file", "Cannot Export the Store to a null File");
            this._file = file;
            this._manager = manager;
        }

        protected override TaskResult RunTaskInternal()
        {
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeType(MimeTypesHelper.GetTrueFileExtension(this._file))).FirstOrDefault(d => d.CanWriteRdfDatasets);
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

            if (writer is TriXWriter)
            {
                //For TriX must load all into memory and then write out all at once
                foreach (Uri u in this.ListGraphs())
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
                foreach (Uri u in this.ListGraphs())
                {
                    using (FileStream stream = new FileStream(this._file, FileMode.Append))
                    {
                        if (writer is IFormatterBasedWriter)
                        {
                            //Stream via a WriteThroughHandler
                            this.Information = "Stream Exporting Graph " + (u != null ? u.ToString() : "Default");
                            WriteThroughHandler handler = new WriteThroughHandler(((IFormatterBasedWriter)writer).TripleFormatterType, new StreamWriter(stream), true);
                            ExportProgressHandler progHandler = new ExportProgressHandler(handler, this, tripleCount);
                            this._manager.LoadGraph(progHandler, u);
                            graphCount++;
                            tripleCount = progHandler.TripleCount;

                            this.Information = "Finished Stream Exporting Graph " + (u != null ? u.ToString() : "Default") + ", exported " + tripleCount + " Triple(s) in " + graphCount + " Graph(s) so far...";
                        }
                        else
                        {
                            //Load Graph into memory
                            Graph g = new Graph();
                            g.BaseUri = u;
                            this.Information = "Loading Graph " + (u != null ? u.ToString() : "Default");
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
            else if (this._manager is IQueryableStorage)
            {
                List<Uri> uris = new List<Uri>();
                Object results = ((IQueryableStorage)this._manager).Query("SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}");
                if (results is SparqlResultSet)
                {
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
                else
                {
                    throw new RdfStorageException("Store failed to list graphs so unable to do a Graph by Graph export");
                }
            }
            else
            {
                throw new RdfStorageException("Store does not support listing Graphs so unable to do a Graph by Graph export");
            }
        }
    }

    class ExportProgressHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private ExportTask _task;
        private int _count = 0;

        public ExportProgressHandler(IRdfHandler handler, ExportTask task, int initCount)
        {
            this._handler = handler;
            this._task = task;
            this._count = initCount;
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        public override bool AcceptsAll
        {
            get 
            {
                return false; 
            }
        }

        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._count++;
            if (this._count % 1000 == 0)
            {
                if (this._task.HasBeenCancelled) return false;
                this._task.Information = "Exported " + this._count + " triples so far...";
            }
            return this._handler.HandleTriple(t);
        }

        public int TripleCount
        {
            get
            {
                return this._count;
            }
        }
    }
}
