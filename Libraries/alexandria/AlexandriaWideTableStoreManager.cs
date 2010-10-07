using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using VDS.Alexandria.WideTable;

namespace VDS.Alexandria
{
    /// <summary>
    /// Abstract Base Class for Alexandria Wide Table Store Managers
    /// </summary>
    public abstract class AlexandriaWideTableStoreManager<TKey, TColumn> : IGenericIOManager
    {
        public IWideTableAdaptor<TKey, TColumn> _adaptor;

        public AlexandriaWideTableStoreManager(IWideTableAdaptor<TKey, TColumn> adaptor)
        {
            this._adaptor = adaptor;
        }

        ~AlexandriaWideTableStoreManager()
        {
            this.Dispose(false);
        }

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            try
            {
                if (g.IsEmpty)
                {
                    //If Graph is Empty load directly into it
                    if (g.BaseUri == null) g.BaseUri = graphUri;
                    if (!this._adaptor.GetGraph(g, graphUri))
                    {
                        throw new AlexandriaException("The underlying Store failed to return the specified Graph");
                    }
                }
                else
                {
                    //If Graph is non-empty load into another Graph then merge
                    Graph h = new Graph();
                    h.BaseUri = graphUri;
                    if (!this._adaptor.GetGraph(h, graphUri))
                    {
                        throw new AlexandriaException("The underlying Store failed to return the specified Graph");
                    }
                    else
                    {
                        g.Merge(h);
                    }
                }
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Load a Graph from the Store", ex);
            }
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri.ToSafeString().Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        public void SaveGraph(IGraph g)
        {
            try
            {
                //If Graph exists then must delete old one
                if (this._adaptor.HasGraph(g.BaseUri))
                {
                    //Try to delete the Old Graph
                    try
                    {
                        this.DeleteGraph(g.BaseUri);
                    }
                    catch (Exception ex)
                    {
                        throw new AlexandriaException("Unable to save a Graph to the Store since this operation requires removing a previous Graph from the Store and this operation failed", ex);
                    }
                }

                //Then can Save the Graph
                this._adaptor.AddGraph(g);
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Save a Graph to the Store", ex);
            }
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                if (this._adaptor.HasGraph(graphUri))
                {
                    if (additions != null)
                    {
                        this._adaptor.AppendToGraph(graphUri, additions);
                    }
                    if (removals != null)
                    {
                        this._adaptor.RemoveFromGraph(graphUri, removals);
                    }
                }
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Update a Graph in the Store", ex);
            }
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri.ToSafeString().Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        public bool UpdateSupported
        {
            get 
            {
                return true; 
            }
        }

        public virtual void DeleteGraph(Uri graphUri)
        {
            try
            {
                this._adaptor.RemoveGraph(graphUri);
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while trying to Delete a Graph from the Store", ex);
            }
        }

        public virtual void DeleteGraph(String graphUri)
        {
            if (graphUri.ToSafeString().Equals(String.Empty))
            {
                this.DeleteGraph((Uri)null);
            }
            else
            {
                this.DeleteGraph(new Uri(graphUri));
            }
        }

        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this._adaptor.Dispose();
        }
    }
}
