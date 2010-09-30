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
    /// <remarks>
    /// <para>
    /// <strong>Important: </strong> You must explicitly <see cref="Dispose">Dispose()</see> of the Manager when you have finished with it otherwise your data and indexes may be inconsistent
    /// </para>
    /// </remarks>
    public abstract class AlexandriaManager : IGenericIOManager
    {
        private IDocumentManager _docManager;
        private IIndexManager _indexManager;

        public AlexandriaManager(IDocumentManager documentManager, IIndexManager indexManager)
        {
            this._docManager = documentManager;
            this._indexManager = indexManager;
        }

        protected internal IDocumentManager DocumentManager
        {
            get
            {
                return this._docManager;
            }
        }

        protected internal IIndexManager IndexManager
        {
            get
            {
                return this._indexManager;
            }
        }

        protected internal abstract String GetDocumentName(String graphUri);

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
                    this._docManager.DataAdaptor.ToGraph(g, doc);
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
            IDocument doc;

            try
            {
                //Create Document if necessary
                if (!this._docManager.HasDocument(name))
                {
                    if (!this._docManager.CreateDocument(name))
                    {
                        throw new AlexandriaException("Unable to save a Graph to the Store as the Document Manager was unable to create a Document for this Graph");
                    }
                }
                else
                {
                    //If the Document already exists then we must first remove from the index any Triples it currently contains
                    try
                    {
                        Graph temp = new Graph();
                        doc = this._docManager.GetDocument(name);
                        this._docManager.DataAdaptor.ToGraph(temp, doc);
                        this._docManager.ReleaseDocument(name);
                        this._indexManager.RemoveFromIndex(temp.Triples);
                    }
                    catch (AlexandriaException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new AlexandriaException("Unable to save a Graph to the Store since this operation requires removing a previous Graph from the Store and this operation failed", ex);
                    }
                }

                doc = this._docManager.GetDocument(name);
                this._docManager.DataAdaptor.ToDocument(g, doc);
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
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            String name = this.GetDocumentName(graphUri);

            if (this._docManager.HasDocument(name))
            {

                try
                {
                    IDocument doc = this._docManager.GetDocument(name);

                    if (additions != null)
                    {
                        this._docManager.DataAdaptor.AppendTriples(additions, doc);
                        this._indexManager.AddToIndex(additions);
                    }
                    if (removals != null)
                    {
                        this._docManager.DataAdaptor.DeleteTriples(removals, doc);
                        this._indexManager.RemoveFromIndex(removals);
                    }
                }
                catch (AlexandriaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AlexandriaException("An error occurred while attempting to update a Graph in the Store", ex);
                }
                finally
                {
                    this._docManager.ReleaseDocument(name);
                }
            }
            else
            {
                //If we don't have a document for this Graph this is actually a SaveGraph() operation on the Graph generated
                //by first asserting the additions and then retracting the removals
                //If that results in a non-empty Graph then SaveGraph() will be invoked
                Graph g = new Graph();
                if (graphUri != null && !graphUri.Equals(String.Empty))
                {
                    g.BaseUri = new Uri(graphUri);
                }
                if (additions != null) g.Assert(additions);
                if (removals != null) g.Retract(removals);
                if (!g.IsEmpty) this.SaveGraph(g);
            }
        }

        public virtual bool UpdateSupported
        {
            get 
            {
                return true;
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
            this._indexManager.Dispose();
            this._docManager.Dispose();
        }
    }
}
