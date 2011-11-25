/*

Copyright Robert Vesse 2009-11
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Base implementation of a dataset against an ADO Store
    /// </summary>
    /// <typeparam name="TConn">Connection Type</typeparam>
    /// <typeparam name="TCommand">Command Type</typeparam>
    /// <typeparam name="TParameter">Parameter Type</typeparam>
    /// <typeparam name="TAdapter">Adaptor Type</typeparam>
    /// <typeparam name="TException">Exception Type</typeparam>
    public abstract class BaseAdoDataset<TConn, TCommand, TParameter, TAdapter, TException> 
        : BaseTransactionalDataset
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdapter : DbDataAdapter
        where TException : Exception
    {
        #region Member Variables

        private BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> _manager;
        private GraphFactory _factory = new GraphFactory();

        #endregion

        /// <summary>
        /// Creates a new Base ADO Dataset
        /// </summary>
        /// <param name="manager">ADO Store Manager</param>
        public BaseAdoDataset(BaseAdoStore<TConn, TCommand, TParameter, TAdapter, TException> manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Adds a Graph to the dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        protected sealed override void AddGraphInternal(IGraph g)
        {
            this._manager.SaveGraph(g);
        }

        /// <summary>
        /// Gets a modifiable graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected sealed override ITransactionalGraph GetModifiableGraphInternal(Uri graphUri)
        {
            IGraph g = this.GetGraphInternal(graphUri);
            return new StoreGraphPersistenceWrapper(this._manager, g, graphUri, false);
        }

        /// <summary>
        /// Removes a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected sealed override void RemoveGraphInternal(Uri graphUri)
        {
            this._manager.DeleteGraph(graphUri);
        }

        /// <summary>
        /// Determines whether the dataset has a specific Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected sealed override bool HasGraphInternal(Uri graphUri)
        {
            int id = this._manager.GetGraphID(graphUri);
            return id > 0;
        }

        /// <summary>
        /// Gets the Graphs from the Dataset
        /// </summary>
        public sealed override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this.GraphUris
                        select this[u]);
            }
        }

        /// <summary>
        /// Gets the URIs of Graphs contained in the dataset
        /// </summary>
        public sealed override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._manager.ListGraphs();
            }
        }

        /// <summary>
        /// Gets a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets whether the dataset contains a given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
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
            cmd.Connection = this._manager.Connection;
            this._manager.EncodeNodeID(cmd, s, TripleSegment.Subject);
            this._manager.EncodeNodeID(cmd, p, TripleSegment.Predicate);
            this._manager.EncodeNodeID(cmd, o, TripleSegment.Object);
            cmd.Parameters.Add(this._manager.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return ((int)cmd.Parameters["RC"].Value) == 1;
        }

        /// <summary>
        /// Gets all triples from the dataset
        /// </summary>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetAllTriples()
        {
            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsVirtual";
            cmd.Connection = this._manager.Connection;
            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd);
        }

        /// <summary>
        /// Gets triples with the given subject from the dataset
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, subj, null, null);
        }

        /// <summary>
        /// Gets triples with the given predicate from the dataset
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            //Get the ID of the fixed nodes
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithPredicateVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, null, pred, null);
        }

        /// <summary>
        /// Gets triples with the given object from the dataset
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            //Get the ID of the fixed nodes
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithObjectVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, null, null, obj);
        }

        /// <summary>
        /// Gets triples with the given subject and predicate from the dataset
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectPredicateVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, subj, pred, null);
        }

        /// <summary>
        /// Gets triples with the given subject and object from the dataset
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            //Get the ID of the fixed nodes
            int s = (subj.NodeType != NodeType.Blank) ? this._manager.GetID(subj) : this._manager.GetBlankNodeID((IBlankNode)subj);
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithSubjectObjectVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("subjectID"));
            cmd.Parameters["subjectID"].DbType = DbType.Int32;
            cmd.Parameters["subjectID"].Value = s;
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, subj, null, obj);
        }

        /// <summary>
        /// Gets triples with the given predicate and object from the dataset
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected sealed override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            //Get the ID of the fixed nodes
            int p = (pred.NodeType != NodeType.Blank) ? this._manager.GetID(pred) : this._manager.GetBlankNodeID((IBlankNode)pred);
            int o = (obj.NodeType != NodeType.Blank) ? this._manager.GetID(obj) : this._manager.GetBlankNodeID((IBlankNode)obj);

            TCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetQuadsWithPredicateObjectVirtual";
            cmd.Connection = this._manager.Connection;
            cmd.Parameters.Add(this._manager.GetParameter("predicateID"));
            cmd.Parameters["predicateID"].DbType = DbType.Int32;
            cmd.Parameters["predicateID"].Value = p;
            cmd.Parameters.Add(this._manager.GetParameter("objectID"));
            cmd.Parameters["objectID"].DbType = DbType.Int32;
            cmd.Parameters["objectID"].Value = o;

            return new AdoTripleEnumerable<TConn, TCommand, TParameter, TAdapter, TException>(this._manager, this._factory, cmd, null, pred, obj);
        }

        /// <summary>
        /// Takes internal flush actions when a transaction is flushed
        /// </summary>
        protected override void FlushInternal()
        {
            this._factory.Reset();
        }

        /// <summary>
        /// Takes internal discard actions when a transaction is discarded
        /// </summary>
        protected override void DiscardInternal()
        {
            this._factory.Reset();
        }
    }

    /// <summary>
    /// Abstract implementation of a dataset against an ADO Store that uses the System.Data.SqlClient API to communicate with the database
    /// </summary>
    public abstract class BaseAdoSqlClientDataset
        : BaseAdoDataset<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        /// <summary>
        /// Creates a new ADO SQL Client Dataset
        /// </summary>
        /// <param name="manager">ADO SQL Client Store Manager</param>
        public BaseAdoSqlClientDataset(BaseAdoSqlClientStore manager)
            : base(manager) { }
    }

    /// <summary>
    /// A dataset backed by an ADO Store on Microsoft SQL Server
    /// </summary>
    public class MicrosoftAdoDataset
        : BaseAdoSqlClientDataset
    {
        /// <summary>
        /// Creates a new Microsoft SQL Server ADO Dataset
        /// </summary>
        /// <param name="manager">Microsoft SQL Server ADO Manager</param>
        public MicrosoftAdoDataset(MicrosoftAdoManager manager)
            : base(manager) { }
    }

    //public class SqlCeAdoDataset
    //    : BaseAdoDataset<SqlCeConnection, SqlCeCommand, SqlCeParameter, SqlCeDataAdapter, SqlCeException>
    //{
    //    public SqlCeAdoDataset(SqlCeAdoManager manager)
    //        : base(manager) { }
    //}
}
