/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
    [Obsolete("The Data.Sql Library is being deprecated in favour of the many open source and commercial triple stores supported by the core library which are far more performant.  Please switch over your code to an alternative triple store, we will no longer support/distribute this library after the 0.7.x series of releases", true)]
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

    [Obsolete("The Data.Sql Library is being deprecated in favour of the many open source and commercial triple stores supported by the core library which are far more performant.  Please switch over your code to an alternative triple store, we will no longer support/distribute this library after the 0.7.x series of releases", true)]
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

    [Obsolete("The Data.Sql Library is being deprecated in favour of the many open source and commercial triple stores supported by the core library which are far more performant.  Please switch over your code to an alternative triple store, we will no longer support/distribute this library after the 0.7.x series of releases", true)]
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
