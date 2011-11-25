using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Virtualisation;

namespace VDS.RDF.Query.Datasets
{
    class AdoTripleEnumerable<TConn,TCommand,TParameter,TAdapter,TException> 
        : IEnumerable<Triple>
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdapter : DbDataAdapter
        where TException : Exception
    {
        private BaseAdoStore<TConn,TCommand,TParameter,TAdapter,TException> _manager;
        private TCommand _cmd;
        private GraphFactory _factory;
        private INode _s, _p, _o;

        public AdoTripleEnumerable(BaseAdoStore<TConn,TCommand,TParameter,TAdapter,TException> manager, GraphFactory factory, TCommand cmd)
        {
            this._manager = manager;
            this._factory = factory;
            this._cmd = cmd;
        }

        public AdoTripleEnumerable(BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> manager, GraphFactory factory, TCommand cmd, INode subj, INode pred, INode obj)
            : this(manager, factory, cmd)
        {
            this._s = subj;
            this._p = pred;
            this._o = obj;
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            switch (this._manager.AccessMode)
            {
                case AdoAccessMode.Batched:
                    return new AdoBatchedTripleEnumerator<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, this._cmd, this._s, this._p, this._o);
                case AdoAccessMode.Streaming:
                default:
                    return new AdoStreamingTripleEnumerator<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, this._cmd, this._s, this._p, this._o);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class AdoStreamingTripleEnumerator<TConn,TCommand,TParameter,TAdapter,TException> 
        : IEnumerator<Triple>
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdapter : DbDataAdapter
        where TException : Exception
    {
        private BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> _manager;
        private GraphFactory _factory;
        private TCommand _cmd;
        private Triple _current;
        private DbDataReader _reader;
        private INode _s, _p, _o;

        public AdoStreamingTripleEnumerator(BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> manager, GraphFactory factory, TCommand cmd, INode subj, INode pred, INode obj)
        {
            this._manager = manager;
            this._factory = factory;
            this._cmd = cmd;
            this._s = subj;
            this._p = pred;
            this._o = obj;
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
            }
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
            if (this._reader == null)
            {
                //Open the Data Reader
                this._reader = this._cmd.ExecuteReader();
            }

            if (this._reader.IsClosed) return false;

            if (this._reader.Read())
            {
                //First need to get the Graph URI
                Uri graphUri = this._manager.GetGraphUri(this._reader.GetInt32(this._reader.GetOrdinal("graphID")));

                //Then get the component parts of the Triple using the fixed parts where known
                IGraph g = this._factory[graphUri];
                INode subj = (this._s != null) ? this.CopyNode(g, this._s) : this._manager.DecodeVirtualNode(g, (byte)this._reader["subjectType"], (int)this._reader["subjectID"]);
                INode pred = (this._p != null) ? this.CopyNode(g, this._p) : this._manager.DecodeVirtualNode(g, (byte)this._reader["predicateType"], (int)this._reader["predicateID"]);
                INode obj = (this._o != null) ? this.CopyNode(g, this._o) : this._manager.DecodeVirtualNode(g, (byte)this._reader["objectType"], (int)this._reader["objectID"]);
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
            throw new NotSupportedException("The Reset() operation is not supported by the AdoStreamingTripleEnumerator");
        }

        private INode CopyNode(IGraph target, INode n)
        {
            if (n is BaseVirtualNode<int, int>)
            {
                return ((BaseVirtualNode<int, int>)n).CopyNode(target);
            }
            else
            {
                return n.CopyNode(target);
            }
        }
    }


    class AdoBatchedTripleEnumerator<TConn, TCommand, TParameter, TAdapter, TException>
        : IEnumerator<Triple>
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdapter : DbDataAdapter
        where TException : Exception
    {
        private BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> _manager;
        private GraphFactory _factory;
        private TCommand _cmd;
        private Triple _current;
        private DataTable _table;
        private int _row = -1;
        private INode _s, _p, _o;

        public AdoBatchedTripleEnumerator(BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> manager, GraphFactory factory, TCommand cmd, INode subj, INode pred, INode obj)
        {
            this._manager = manager;
            this._factory = factory;
            this._cmd = cmd;
            this._s = subj;
            this._p = pred;
            this._o = obj;
        }

        public Triple Current
        {
            get 
            {
                if (this._table == null) throw new InvalidOperationException("Enumerator is positioned before the start of the collection");
                if (this._row >= this._table.Rows.Count) throw new InvalidOperationException("Enumerator is positioned after the end of the collection");
                if (this._current == null) throw new InvalidOperationException("No element at the current position");
                return this._current;
            }
        }

        public void Dispose()
        {
            if (this._table != null)
            {
                this._row = this._table.Rows.Count;
                this._table.Dispose();
            }
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
            if (this._table == null)
            {
                //Open the Data Table
                using (TAdapter adapter = this._manager.GetAdapter())
                {
                    adapter.SelectCommand = this._cmd;
                    this._table = new DataTable();
                    adapter.Fill(this._table);
                    adapter.Dispose();
                }
            }

            this._row++;
            if (this._row >= this._table.Rows.Count) return false;

            if (this._row <= this._table.Rows.Count)
            {
                DataRow row = this._table.Rows[this._row];

                //First need to get the Graph URI
                Uri graphUri = this._manager.GetGraphUri((int)row["graphID"]);

                //Then get the component parts of the Triple using the fixed parts where known
                IGraph g = this._factory[graphUri];
                INode subj = (this._s != null) ? this.CopyNode(g, this._s) : this._manager.DecodeVirtualNode(g, (byte)row["subjectType"], (int)row["subjectID"]);
                INode pred = (this._p != null) ? this.CopyNode(g, this._p) : this._manager.DecodeVirtualNode(g, (byte)row["predicateType"], (int)row["predicateID"]);
                INode obj = (this._o != null) ? this.CopyNode(g, this._o) : this._manager.DecodeVirtualNode(g, (byte)row["objectType"], (int)row["objectID"]);
                this._current = new Triple(subj, pred, obj);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("The Reset() operation is not supported by the AdoStreamingTripleEnumerator");
        }

        private INode CopyNode(IGraph target, INode n)
        {
            if (n is BaseVirtualNode<int, int>)
            {
                return ((BaseVirtualNode<int, int>)n).CopyNode(target);
            }
            else
            {
                return n.CopyNode(target);
            }
        }
    }
}
