/*

Copyright Robert Vesse 2009-10
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

#if !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Represents an out of memory dataset which is the data stored in a SQL Database using the dotNetRDF Store format
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class SqlDataset : BaseTransactionalDataset
    {
        private IDotNetRDFStoreManager _manager;
        private GraphFactory _factory = new GraphFactory();
        private SqlReader _reader;

        /// <summary>
        /// Creates a new SQL Dataset
        /// </summary>
        /// <param name="manager">Manager for a dotNetRDF format SQL Store</param>
        public SqlDataset(IDotNetRDFStoreManager manager)
        {
            this._manager = manager;
            if (manager is IThreadedSqlIOManager)
            {
                ((IThreadedSqlIOManager)this._manager).DisableTransactions = true;
            }
            this._reader = new SqlReader(this._manager);
        }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        protected override void AddGraphInternal(IGraph g)
        {
            SqlWriter writer = new SqlWriter(this._manager);
            writer.Save(g, false);
        }

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected override void RemoveGraphInternal(Uri graphUri)
        {
            if (graphUri == null)
            {
                this._manager.ClearGraph(this._manager.GetGraphID(null));
            }
            else
            {
                this._manager.RemoveGraph(this._manager.GetGraphID(graphUri));
            }
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraphInternal(Uri graphUri)
        {
            return this._manager.Exists(graphUri);
        }

        /// <summary>
        /// Gets all the Graphs in the Dataset
        /// </summary>
        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this._manager.GetGraphUris()
                        select this[u]);
            }
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._manager.GetGraphUris();
            }
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// For SQL datasets the Graph returned from this property is no different from the Graph returned by the <see cref="InMemoryDataset.GetModifiableGraph">GetModifiableGraph()</see> method
        /// </para>
        /// </remarks>
        protected override IGraph GetGraphInternal(Uri graphUri)
        {
            return this._reader.Load(graphUri);
        }

        /// <summary>
        /// Gets a modifiable wrapper around the Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override ITransactionalGraph GetModifiableGraphInternal(Uri graphUri)
        {
            return new StoreGraphPersistenceWrapper(this._manager, this.GetGraphInternal(graphUri));
        }

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool ContainsTripleInternal(Triple t)
        {
            try
            {
                this._manager.Open(true);
                String subjID = this._manager.SaveNode(t.Subject);
                String predID = this._manager.SaveNode(t.Predicate);
                String objID = this._manager.SaveNode(t.Object);

                String query = "SELECT tripleID FROM TRIPLES WHERE tripleSubject=" + subjID + " AND triplePredicate=" + predID + " AND tripleObject=" + objID;
                bool contains = false;
                Object tripleID = this._manager.ExecuteScalar(query);
                if (tripleID != null) contains = true;
                this._manager.Close(true);

                return contains;
            }
            catch
            {
                this._manager.Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Gets all the Triples in the underlying SQL store
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetAllTriples()
        {
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID";
            return new SqlTripleEnumerable(this._manager, this._factory, query);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE triplePredicate=" + this._manager.SaveNode(pred);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;

        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj) + " AND triplePredicate=" + this._manager.SaveNode(pred);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj) + " AND tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE triplePredicate=" + this._manager.SaveNode(pred) + " AND tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        /// <summary>
        /// Flushes outstanding changes to the store
        /// </summary>
        protected override void FlushInternal()
        {
            this._manager.Flush();
        }
    }

    #region Enumerator Classes

    class SqlTripleEnumerable : IEnumerable<Triple>
    {
        private IDotNetRDFStoreManager _manager;
        private String _query;
        private GraphFactory _factory;

        public SqlTripleEnumerable(IDotNetRDFStoreManager manager, GraphFactory factory, String query)
        {
            this._manager = manager;
            this._factory = factory;
            this._query = query;
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            return new SqlTripleEnumerator(this._manager, this._factory, this._query);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class SqlTripleEnumerator : IEnumerator<Triple>
    {
        private IDotNetRDFStoreManager _manager;
        private GraphFactory _factory;
        private String _query;
        private Triple _current;
        private DbDataReader _reader;

        public SqlTripleEnumerator(IDotNetRDFStoreManager manager, GraphFactory factory, String query)
        {
            this._manager = manager;
            this._factory = factory;
            this._query = query;
        }

        public Triple Current
        {
            get 
            {
                if (this._reader == null) throw new InvalidOperationException("Enumerator is positioned before the start of the collection");
                if (this._reader.IsClosed) throw new InvalidOperationException("Enumerator is positioned after the end of the collection");
                if (this._current == null) throw new InvalidOperationException("No element at the current position");
                return this._current;
            }
        }

        public void Dispose()
        {
            if (this._reader != null)
            {
                if (!this._reader.IsClosed) this._reader.Close();
                this._reader = null;
            }
            this._manager.Close(true);
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            {
                return this.Current; 
            }
        }

        public bool MoveNext()
        {
            this._manager.Open(true);

            if (this._reader == null)
            {
                //Open the Data Reader
                this._reader = this._manager.ExecuteStreamingQuery(this._query);
            }

            if (this._reader.IsClosed) return false;

            if (this._reader.Read())
            {
                //First need to get the Graph URI
                Uri graphUri = this._manager.GetGraphUri(this._reader.GetInt32(this._reader.GetOrdinal("graphID")).ToString());
                
                //Then get the component parts of the Triple
                IGraph g = this._factory[graphUri];
                INode subj = this._manager.LoadNode(g, this._reader.GetInt32(this._reader.GetOrdinal("tripleSubject")).ToString());
                INode pred = this._manager.LoadNode(g, this._reader.GetInt32(this._reader.GetOrdinal("triplePredicate")).ToString());
                INode obj = this._manager.LoadNode(g, this._reader.GetInt32(this._reader.GetOrdinal("tripleObject")).ToString());
                this._current = new Triple(subj, pred, obj);

                return true;
            }
            else
            {
                if (!this._reader.IsClosed) this._reader.Close();
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("The Reset() operation is not supported by the SqlTripleEnumerator");
        }
    }

    #endregion
}

#endif