using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;
using VDS.Alexandria.WideTable.ColumnSchema;

namespace VDS.Alexandria.WideTable
{
    public abstract class BaseWideTableAdaptor<TColumn> : IWideTableAdaptor<String, TColumn>
    {
        private SHA256Managed _hash;
        private NQuadsFormatter _formatter = new NQuadsFormatter();

        public abstract IColumnSchema<TColumn> ColumnSchema
        {
            get;
        }

        #region Public Implementation

        public bool AddGraph(IGraph g)
        {
            try
            {
                foreach (Triple t in g.Triples)
                {
                    this.InsertData(this.GetRowKey(t), this.ColumnSchema.ToColumns(t));
                }
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while adding a Graph to the Store", ex);
            }
        }

        public bool RemoveGraph(Uri graphUri)
        {
            try
            {
                foreach (String rowKey in this.GetRowsForGraph(graphUri))
                {
                    this.DeleteRow(rowKey);
                }
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while removing a Graph from the Store", ex);
            }
        }

        public bool HasGraph(Uri graphUri)
        {
            try 
            {
                return this.GetRowsForGraph(graphUri).Any();
            } 
            catch 
            {
                return false;
            }
        }

        public bool GetGraph(IGraph g, Uri graphUri)
        {
            try
            {
                IEnumerable<String> columns = this.ColumnSchema.ColumnNames;
                foreach (String rowKey in this.GetRowsForGraph(graphUri))
                {
                    g.Assert(this.ColumnSchema.FromColumns(g, this.GetColumnsForRow(rowKey, columns)));
                }
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while retrieving a Graph from the Store", ex);
            }
        }

        public bool AppendToGraph(Uri graphUri, IEnumerable<Triple> ts)
        {
            try
            {
                Graph g = new Graph();
                g.BaseUri = graphUri;
                foreach (Triple t in ts)
                {
                    if (!g.BaseUri.ToSafeString().Equals(t.GraphUri.ToSafeString()))
                    {
                        Triple t2 = t.CopyTriple(g);
                        this.InsertData(this.GetRowKey(t2), this.ColumnSchema.ToColumns(t2));
                    }
                    else
                    {
                        this.InsertData(this.GetRowKey(t), this.ColumnSchema.ToColumns(t));
                    }
                }
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occurred while appending Triples to a Graph in the Store", ex);
            }

        }

        public bool RemoveFromGraph(Uri graphUri, IEnumerable<Triple> ts)
        {
            try
            {
                Graph g = new Graph();
                g.BaseUri = graphUri;
                foreach (Triple t in ts)
                {
                    if (!g.BaseUri.ToSafeString().Equals(t.GraphUri.ToSafeString()))
                    {
                        Triple t2 = t.CopyTriple(g);
                        this.DeleteRow(this.GetRowKey(t2));
                    }
                    else
                    {
                        this.DeleteRow(this.GetRowKey(t));
                    }
                }
                return true;
            }
            catch (AlexandriaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AlexandriaException("An error occured while removing Triples from a Graph in the Store", ex);
            }
        }

        #endregion

        #region Abstract Internal Implementation for Data Insert/Delete

        protected String GetRowKey(Triple t)
        {
            //Only instantiate the SHA256 class when we first use it
            if (_hash == null) _hash = new SHA256Managed();

            Byte[] input = Encoding.UTF8.GetBytes(this._formatter.Format(t));
            Byte[] output = _hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        protected abstract IEnumerable<String> GetRowsForGraph(Uri graphUri);

        protected abstract IEnumerable<TColumn> GetColumnsForRow(String rowKey, IEnumerable<String> columNames);

        protected abstract bool InsertData(String rowKey, TColumn column);

        protected abstract bool InsertData(String rowKey, IEnumerable<TColumn> columns);

        protected abstract bool DeleteData(String rowKey, TColumn column);

        protected abstract bool DeleteData(String rowKey, IEnumerable<TColumn> columns);

        protected abstract bool DeleteRow(String rowKey);

        #endregion

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}
