using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Indexing;

namespace VDS.Alexandria
{
    /// <summary>
    /// Abstract Base Class for Alexandria Document Store Managers
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important: </strong> You must explicitly <see cref="AlexandriaDocumentStoreManager.Dispose">Dispose()</see> of the Manager when you have finished with it otherwise your data and indexes may be inconsistent.  While the Manager has been designed to force a <see cref="AlexandriaManager.Dispose">Dispose()</see> call if you fail to do so this is very inefficient and may cause your program to consume memory/hang unecessarily due to the vagaries of the .Net Garbage Collector
    /// </para>
    /// </remarks>
    public abstract class AlexandriaDocumentStoreManager<TReader,TWriter> : BaseAlexandriaManager
    {
        private IDocumentManager<TReader,TWriter> _docManager;
        private IIndexManager _indexManager;

        public AlexandriaDocumentStoreManager(IDocumentManager<TReader,TWriter> documentManager, IIndexManager indexManager)
        {
            this._docManager = documentManager;
            this._indexManager = indexManager;
        }

        ~AlexandriaDocumentStoreManager()
        {
            this.Dispose(false);
        }

        protected internal IDocumentManager<TReader,TWriter> DocumentManager
        {
            get
            {
                return this._docManager;
            }
        }

        protected internal override IIndexManager IndexManager
        {
            get
            {
                return this._indexManager;
            }
        }

        public override void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        public override void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        public override void LoadGraph(IGraph g, string graphUri)
        {
            String name = this._docManager.GraphRegistry.GetDocumentName(graphUri);

            //If the Document doesn't exist the Graph doesn't exist in the Store so nothing to do
            if (this._docManager.HasDocument(name))
            {
                try
                {
                    IDocument<TReader,TWriter> doc = this._docManager.GetDocument(name);
                    if (g.IsEmpty)
                    {
                        //If Graph is Empty load directly into that Graph
                        if (g.BaseUri == null && !String.IsNullOrEmpty(graphUri)) g.BaseUri = new Uri(graphUri);
                        this._docManager.DataAdaptor.ToGraph(g, doc);
                    }
                    else
                    {
                        //If Graph is not Empty load into another Graph then merge
                        Graph h = new Graph();
                        if (!String.IsNullOrEmpty(graphUri)) h.BaseUri = new Uri(graphUri);
                        this._docManager.DataAdaptor.ToGraph(h, doc);
                        g.Merge(h);
                    }
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

        public override void LoadGraph(IRdfHandler handler, string graphUri)
        {
            throw new NotImplementedException();
        }

        public override void SaveGraph(IGraph g)
        {
            String name = this._docManager.GraphRegistry.GetDocumentName(g.BaseUri.ToSafeString());
            IDocument<TReader,TWriter> doc;

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
                    //If the Document already exists then we must first delete it then replace it with a new Document
                    try
                    {
                        this.DeleteGraph(g.BaseUri.ToSafeString());
                        if (!this._docManager.CreateDocument(name))
                        {
                            throw new AlexandriaException("Unable to save a Graph to the Store as the Document Manager was unable to create a Document for this Graph");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new AlexandriaException("Unable to save a Graph to the Store since this operation requires removing a previous Graph from the Store and this operation failed", ex);
                    }
                }

                doc = this._docManager.GetDocument(name);
                this._docManager.GraphRegistry.RegisterGraph(g.BaseUri.ToSafeString(), name);
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

        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            String name = this._docManager.GraphRegistry.GetDocumentName(graphUri);

            if (this._docManager.HasDocument(name))
            {
                try
                {
                    IDocument<TReader,TWriter> doc = this._docManager.GetDocument(name);

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
                if (String.IsNullOrEmpty(graphUri))
                {
                    g.BaseUri = new Uri(graphUri);
                }
                if (additions != null) g.Assert(additions);
                if (removals != null) g.Retract(removals);
                if (!g.IsEmpty) this.SaveGraph(g);
            }
        }

        public override bool UpdateSupported
        {
            get 
            {
                return true;
            }
        }

        public override void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        public override void DeleteGraph(String graphUri)
        {
            String name = this._docManager.GraphRegistry.GetDocumentName(graphUri);

            //If the Document doesn't exist the Graph doesn't exist in the Store so nothing to do
            if (this._docManager.HasDocument(name))
            {
                //First we need to remove any Triples in this Graph from the Index
                try
                {
                    Graph temp = new Graph();
                    IDocument<TReader,TWriter> doc = this._docManager.GetDocument(name);
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
                    throw new AlexandriaException("Unable to delete a Graph from the Store since this operation requires removing indexed Triples from the Store and this operation failed", ex);
                }

                //Then we can actually delete the Graph
                try
                {
                    this._docManager.DeleteDocument(name);
                    this._docManager.GraphRegistry.UnregisterGraph(graphUri, name);
                }
                catch (AlexandriaException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AlexandriaException("An error occured while attempting to delete a Graph from the Store", ex);
                }
            }
        }

        public override bool DeleteSupported
        {
            get 
            {
                return true;
            }
        }

        public override IEnumerable<Uri> ListGraphs()
        {
            return (from u in this._docManager.GraphRegistry.GraphUris
                    select (String.IsNullOrEmpty(u) ? null : new Uri(u)));
        }

        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        public override bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        public override bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public override void Flush()
        {
            this._indexManager.Flush();
            this._docManager.Flush();
        }

        public override void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (this._indexManager != null) this._indexManager.Dispose();
            if (this._docManager != null) this._docManager.Dispose();
        }
    }
}
