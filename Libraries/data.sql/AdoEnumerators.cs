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
    class AdoTripleEnumerable<TConn,TCommand,TParameter,TAdaptor,TException> 
        : IEnumerable<Triple>
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> _manager;
        private TCommand _cmd;
        private GraphFactory _factory;
        private INode _s, _p, _o;

        public AdoTripleEnumerable(BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> manager, GraphFactory factory, TCommand cmd)
        {
            this._manager = manager;
            this._factory = factory;
            this._cmd = cmd;
        }

        public AdoTripleEnumerable(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager, GraphFactory factory, TCommand cmd, INode subj, INode pred, INode obj)
            : this(manager, factory, cmd)
        {
            this._s = subj;
            this._p = pred;
            this._o = obj;
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            return new AdoTripleEnumerator<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, this._cmd, this._s, this._p, this._o);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class AdoTripleEnumerator<TConn,TCommand,TParameter,TAdaptor,TException> 
        : IEnumerator<Triple>
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> _manager;
        private GraphFactory _factory;
        private TCommand _cmd;
        private Triple _current;
        private DbDataReader _reader;
        private INode _s, _p, _o;

        public AdoTripleEnumerator(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager, GraphFactory factory, TCommand cmd, INode subj, INode pred, INode obj)
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
            throw new NotSupportedException("The Reset() operation is not supported by the AdoTripleEnumerator");
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
