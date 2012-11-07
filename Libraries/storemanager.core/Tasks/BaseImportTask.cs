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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Abstract Base Class for import tasks
    /// </summary>
    public abstract class BaseImportTask 
        : CancellableTask<TaskResult>
    {
        private IStorageProvider _manager;
        private Uri _targetUri;
        private CancellableHandler _canceller;
        private StoreCountHandler _counter = new StoreCountHandler();
        private ImportProgressHandler _progress;
        private int _batchSize;

        /// <summary>
        /// Creates a new Import Task
        /// </summary>
        /// <param name="name">Task Name</param>
        /// <param name="manager">Storage Provider to import to</param>
        /// <param name="targetGraph">Target Graph to import to</param>
        /// <param name="batchSize">Batch Size for imports</param>
        public BaseImportTask(String name, IStorageProvider manager, Uri targetGraph, int batchSize)
            : base(name)
        {
            this._manager = manager;
            this._targetUri = targetGraph;
            this._batchSize = batchSize;
            if (this._batchSize <= 0) this._batchSize = 100;

            this._progress = new ImportProgressHandler(this._counter);
            this._progress.Progress += new ImportProgressEventHandler(_progress_Progress);
        }

        /// <summary>
        /// Progress reporter event handler
        /// </summary>
        void _progress_Progress()
        {
            StringBuilder output = new StringBuilder();
            int readTriples = this._counter.TripleCount;
            int importedTriples = (readTriples / this._batchSize) * this._batchSize;
            this.Information = "Read " + readTriples + " Triple(s), Imported " + importedTriples + " Triple(s) in " + this._counter.GraphCount + " Graph(s) so far...";
        }
        
        /// <summary>
        /// Implementation of the task
        /// </summary>
        /// <returns></returns>
        protected sealed override TaskResult RunTaskInternal()
        {
            if (this._manager.UpdateSupported)
            {
                //Use a WriteToStoreHandler for direct writing
                this._canceller = new CancellableHandler(new WriteToStoreHandler(this._manager, this.GetTargetUri(), this._batchSize));
                if (this.HasBeenCancelled) this._canceller.Cancel();

                //Wrap in a ChainedHandler to ensure we permit cancellation but also count imported triples
                ChainedHandler m = new ChainedHandler(new IRdfHandler[] { this._canceller, this._progress });
                this.ImportUsingHandler(m);
            }
            else
            {
                //Use a StoreHandler to load into memory and will do a SaveGraph() at the end
                TripleStore store = new TripleStore();
                this._canceller = new CancellableHandler(new StoreHandler(store));
                if (this.HasBeenCancelled) this._canceller.Cancel();

                //Wrap in a ChainedHandler to ensure we permit cancellation but also count imported triples
                ChainedHandler m = new ChainedHandler(new IRdfHandler[] { this._canceller, this._progress });
                this.ImportUsingHandler(m);

                //Finally Save to the underlying Store
                foreach (IGraph g in store.Graphs)
                {
                    if (g.BaseUri == null)
                    {
                        g.BaseUri = this.GetTargetUri();
                    }
                    this._manager.SaveGraph(g);
                }
            }
            this.Information = this._counter.TripleCount + " Triple(s) in " + this._counter.GraphCount + " Graph(s) Imported";

            return new TaskResult(true);
        }

        /// <summary>
        /// Abstract method to be implemented by derived classes to perform the actual importing
        /// </summary>
        /// <param name="handler"></param>
        protected abstract void ImportUsingHandler(IRdfHandler handler);

        /// <summary>
        /// Gets the Target URI
        /// </summary>
        /// <returns></returns>
        private Uri GetTargetUri()
        {
            if (this._targetUri != null)
            {
                return this._targetUri;
            }
            else
            {
                return this.GetDefaultTargetUri();
            }
        }

        /// <summary>
        /// Gets the Default Target URI
        /// </summary>
        /// <returns></returns>
        protected virtual Uri GetDefaultTargetUri()
        {
            return null;
        }

        /// <summary>
        /// Cancels the Task
        /// </summary>
        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }

    /// <summary>
    /// Delegate for Import Progress Events
    /// </summary>
    public delegate void ImportProgressEventHandler();

    /// <summary>
    /// RDF Handler which can raise Progress Events
    /// </summary>
    class ImportProgressHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private StoreCountHandler _handler;
        private int _progressCount = 0;

        /// <summary>
        /// Creates a new Handler
        /// </summary>
        /// <param name="handler">Handler</param>
        public ImportProgressHandler(StoreCountHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        /// <summary>
        /// Gets the Inner Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable().OfType<IRdfHandler>();
            }
        }

        /// <summary>
        /// Starts the Handling of RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._progressCount = 0;
            this._handler.StartRdf();
        }

        /// <summary>
        /// Ends the Handling of RDF
        /// </summary>
        /// <param name="ok">Whether parsing finished OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles Triples
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            bool temp = this._handler.HandleTriple(t);
            this._progressCount++;
            if (this._progressCount == 1000)
            {
                this._progressCount = 0;
                this.RaiseImportProgress();
            }
            return temp;
        }

        /// <summary>
        /// Returns that this handler does accept everything
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Progress Event
        /// </summary>
        public event ImportProgressEventHandler Progress;

        /// <summary>
        /// Raises the Progress Event
        /// </summary>
        private void RaiseImportProgress()
        {
            ImportProgressEventHandler d = this.Progress;
            if (d != null)
            {
                d();
            }
        }
    }
}
