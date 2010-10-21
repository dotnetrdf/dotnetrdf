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
    public class SqlDataset : BaseDataset
    {
        private IDotNetRDFStoreManager _manager;
        private NodeFactory _factory = new NodeFactory();

        public SqlDataset(IDotNetRDFStoreManager manager)
        {
            this._manager = manager;
            if (manager is IThreadedSqlIOManager)
            {
                ((IThreadedSqlIOManager)this._manager).DisableTransactions = true;
            }
        }

        public override void AddGraph(IGraph g)
        {
            SqlWriter writer = new SqlWriter(this._manager);
            writer.Save(g, true);
        }

        public override void RemoveGraph(Uri graphUri)
        {
            this._manager.RemoveGraph(this._manager.GetGraphID(graphUri));
        }

        public override bool HasGraph(Uri graphUri)
        {
            return this._manager.Exists(graphUri);
        }

        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this._manager.GetGraphUris()
                        select this[u]);
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._manager.GetGraphUris();
            }
        }

        public override IGraph this[Uri graphUri]
        {
            get 
            {
                return new SqlGraph(graphUri, this._manager);
            }
        }

        public override bool ContainsTriple(Triple t)
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

        protected override IEnumerable<Triple> GetAllTriples()
        {
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID";
            return new SqlTripleEnumerable(this._manager, this._factory, query);
        }

        public override IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE triplePredicate=" + this._manager.SaveNode(pred);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj) + " AND triplePredicate=" + this._manager.SaveNode(pred);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE tripleSubject=" + this._manager.SaveNode(subj) + " AND tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            this._manager.Open(true);
            String query = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE triplePredicate=" + this._manager.SaveNode(pred) + " AND tripleObject=" + this._manager.SaveNode(obj);
            IEnumerable<Triple> ts = new SqlTripleEnumerable(this._manager, this._factory, query);
            this._manager.Close(true);
            return ts;
        }

        public override void Flush()
        {
            this._manager.Flush();
        }
    }

    #region Enumerator Classes

    class SqlTripleEnumerable : IEnumerable<Triple>
    {
        private IDotNetRDFStoreManager _manager;
        private String _query;
        private NodeFactory _factory;

        public SqlTripleEnumerable(IDotNetRDFStoreManager manager, NodeFactory factory, String query)
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
        private NodeFactory _factory;
        private String _query;
        private Triple _current;
        private DbDataReader _reader;

        public SqlTripleEnumerator(IDotNetRDFStoreManager manager, NodeFactory factory, String query)
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