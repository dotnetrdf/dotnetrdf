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

        private BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> _manager;
        private GraphFactory _factory = new GraphFactory();

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
            IGraph g = this.GetGraphInternal(graphUri);
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
            bool created;
            IGraph g = this._factory.TryGetGraph(graphUri, out created);
            if (created || g.IsEmpty)
            {
                //If it was just created or is empty then fill it with data

                //Note: For genuinely empty graphs we may make unecessary round trips to the server
                //to try loading the data but as there will be nothing to load this should be
                //relatively cheap
                //The problem is that the factory gets used in other places such as the triple
                //enumerators so we can't distinguish whether the graph reference was created in
                //this method or if it was created in an enumerator.  In the latter case the graph
                //won't be loaded so we must try doing this now (even if we tried and the graph is
                //actually empty)
                this._manager.LoadGraphVirtual(g, graphUri);
            }
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
            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsVirtual";
            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, subj, null, null);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            //Get the ID of the fixed nodes
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithPredicateVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, null, pred, null);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            //Get the ID of the fixed nodes
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithObjectVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, null, null, obj);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectPredicateVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, subj, pred, null);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectObjectVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, subj, null, obj);
        }

        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            //Get the ID of the fixed nodes
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithPredicateObjectVirtual";
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdaptor, TException>(this._manager, this._factory, cmd, null, pred, obj);
        }

        protected override void FlushInternal()
        {
            this._factory.Reset();
        }

        protected override void DiscardInternal()
        {
            this._factory.Reset();
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
