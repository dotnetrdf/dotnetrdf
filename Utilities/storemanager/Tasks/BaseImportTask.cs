using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public abstract class BaseImportTask : CancellableTask<TaskResult>
    {
        private IGenericIOManager _manager;
        private Uri _targetUri;
        private CancellableHandler _canceller;
        private StoreCountHandler _counter = new StoreCountHandler();
        private ImportProgressHandler _progress;

        public BaseImportTask(String name, IGenericIOManager manager, Uri targetGraph)
            : base(name)
        {
            this._manager = manager;
            this._targetUri = targetGraph;
            this._progress = new ImportProgressHandler(this._counter);
            this._progress.Progress += new ImportProgressEventHandler(_progress_Progress);
        }

        void _progress_Progress()
        {
            this.Information = "Imported " + this._counter.TripleCount + " Triple(s) in " + this._counter.GraphCount + " Graph(s) so far...";
        }


        protected sealed override TaskResult RunTaskInternal()
        {
            if (this._manager.UpdateSupported)
            {
                //Use a WriteToStoreHandler for direct writing
                this._canceller = new CancellableHandler(new WriteToStoreHandler(this._manager, this.GetTargetUri()));
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

        protected abstract void ImportUsingHandler(IRdfHandler handler);

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

        protected virtual Uri GetDefaultTargetUri()
        {
            return null;
        }

        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }

    public delegate void ImportProgressEventHandler();

    class ImportProgressHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private StoreCountHandler _handler;
        private int _progressCount = 0;

        public ImportProgressHandler(StoreCountHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable().OfType<IRdfHandler>();
            }
        }

        protected override void StartRdfInternal()
        {
            this._progressCount = 0;
            this._handler.StartRdf();
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

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

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }

        public event ImportProgressEventHandler Progress;

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
