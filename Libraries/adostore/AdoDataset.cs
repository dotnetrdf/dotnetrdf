using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Query.Datasets
{
    public abstract class BaseAdoDataset<TConn, TCommand, TParameter, TAdaptor, TException> 
        : BaseTransactionalDataset
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        #region Member Variables

        //

        private BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> _manager;
        private TripleStore _store = new TripleStore();

        #endregion

        public BaseAdoDataset(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
        {
            this._manager = manager;
        }

        protected sealed override void AddGraphInternal(IGraph g)
        {
            this._manager.SaveGraph(g);
        }

        protected sealed override ITransactionalGraph GetModifiableGraphInternal(Uri graphUri)
        {
            Graph g = new Graph();
            this._manager.LoadGraphVirtual(g, graphUri);
            return new StoreGraphPersistenceWrapper(this._manager, g, graphUri, false);
        }

        protected sealed override void RemoveGraphInternal(Uri graphUri)
        {
            this._manager.DeleteGraph(graphUri);
        }

        protected sealed override bool HasGraphInternal(Uri graphUri)
        {
            int id = this._manager.GetGraphID(graphUri);
            return id == 1;
        }

        public sealed override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this.GraphUris
                        select this[u]);
            }
        }

        public sealed override IEnumerable<Uri> GraphUris
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        protected sealed override IGraph GetGraphInternal(Uri graphUri)
        {
            Graph g = new Graph();
            this._manager.LoadGraphVirtual(g, graphUri);
            return g;
        }

        protected sealed override bool ContainsTripleInternal(Triple t)
        {
            int s = this._manager.GetID(t.Subject);
            if (s == 0) return false;
            int p = this._manager.GetID(t.Predicate);
            if (p == 0) return false;
            int o = this._manager.GetID(t.Object);
            if (o == 0) return false;

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "HasQuad";
            this._manager.EncodeNodeID(cmd, s, TripleSegment.Subject);
            this._manager.EncodeNodeID(cmd, p, TripleSegment.Predicate);
            this._manager.EncodeNodeID(cmd, o, TripleSegment.Object);
            cmd.Parameters.Add(this._manager.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return ((int)cmd.Parameters["RC"].Value) == 1;
        }

        protected sealed override IEnumerable<Triple> GetAllTriples()
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class BaseAdoSqlClientDataset
        : BaseAdoDataset<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        public BaseAdoSqlClientDataset(BaseAdoSqlClientStore manager)
            : base(manager) { }
    }

    public class MicrosoftAdoDataset
        : BaseAdoSqlClientDataset
    {
        public MicrosoftAdoDataset(MicrosoftAdoManager manager)
            : base(manager) { }
    }
}
