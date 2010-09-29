using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    /// <summary>
    /// Abstract Base Class for Alexandria Store Managers
    /// </summary>
    public abstract class AlexandriaManager : IGenericIOManager
    {
        private IDocumentManager _docManager;
        private IIndexManager _indexManager;

        public AlexandriaManager(IDocumentManager documentManager, IIndexManager indexManager)
        {
            this._docManager = documentManager;
            this._indexManager = indexManager;
        }

        protected abstract String GetDocumentName(String graphUri);

        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        public virtual void LoadGraph(IGraph g, string graphUri)
        {
            String name = this.GetDocumentName(graphUri);

            //If the Document doesn't exist the Graph doesn't exist in the Store so nothing to do
            if (this._docManager.HasDocument(name))
            {
                try
                {
                    IDocument doc = this._docManager.GetDocument(name);
                    this._docManager.GraphAdaptor.ToGraph(g, doc);
                }
                catch (AlexandriaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AlexandriaException("An error occured while attempting to load a Graph from the Store", ex);
                }
                finally
                {
                    this._docManager.ReleaseDocument(name);
                }
            }
        }

        public virtual void SaveGraph(IGraph g)
        {
            String name = this.GetDocumentName(g.BaseUri.ToSafeString());

            try
            {
                IDocument doc = this._docManager.GetDocument(name);
                this._docManager.GraphAdaptor.ToDocument(g, doc);
                this._indexManager.AddToIndex(g.Triples);
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while attempting to save a Graph to the Store", ex);
            }
            finally
            {
                this._docManager.ReleaseDocument(name);
            }
        }

        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateSupported
        {
            get 
            {
                return false;
            }
        }

        public virtual bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        public virtual bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public virtual void Dispose()
        {
            this._docManager.Dispose();
            this._indexManager.Dispose();
        }
    }
}
