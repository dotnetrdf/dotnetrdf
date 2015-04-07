using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Spin
{
    // TODO do we really need a connection state ? If yes : 
    //  => check the state for the connection on each method
    //  => decide where and how to maintain it correctly

    #region Event args and delegates

    public class ConnectionEventArgs : EventArgs
    {
        internal ConnectionEventArgs()
            : base()
        {
        }
    }

    public class GraphsChangedEventArgs : ConnectionEventArgs
    {

        private IEnumerable<Uri> _changedGraphs;

        internal GraphsChangedEventArgs(IEnumerable<Uri> graphUris)
            : base()
        {
            _changedGraphs = graphUris;
        }

        public IEnumerable<Uri> GraphUris {
            get {
                return _changedGraphs;
            }
        }
    }

    /// <summary>
    /// Delegate Type for SparqlCommand events
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Triple Event Arguments</param>
    public delegate void ConnectionEventHandler(Object sender, ConnectionEventArgs args);

    #endregion

    /// <summary>
    /// The Connection class wraps a client's connection to a SPIN capable or enhanced RDF storage <todo>or SPARQLEndpoint</todo>.
    /// It provides for session variables management and ACID access to the storage.
    /// </summary>
    /// <remarks>
    /// To ensure the storage's data consistency, any access should be wrapped by instances of this class. Direct access is to be prohibited by any necessary mean.
    /// </remarks>
    /// TODO complete the IUpdateableStorage implementation
    /// TODO complete session parameters assignation using objects
    /// TODO define events to allow dynamic changes to the internal components
    ///     => either during the connection for local changes or at commit for concurrent connections
    public sealed class Connection
        : SparqlTemporaryResourceMediator, IUpdateableStorage, ITransactionalStorage
    {
        private static SparqlQueryParser _queryParser = new SparqlQueryParser();
        private static SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private String _id = Guid.NewGuid().ToString().Replace("-", "");

        private IQueryableStorage _underlyingStorage;
        private ConnectionState _state = ConnectionState.Closed;
        private IPrincipal _currentUser;
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        #region Public implementation (amap IDbConnection-like)

        public Connection()
            : base()
        {
            //Uri = UriFactory.Create(BaseTemporaryGraphConsumer.NS_URI + "connection:" + _id);
        }

        public void Open(IQueryableStorage storage)
        {
            _underlyingStorage = storage;
            _state = ConnectionState.Open;
        }

        // 
        public void Open(IQueryableStorage storage, string userId, System.Security.SecureString password)
        {
            _underlyingStorage = storage;
            _state = ConnectionState.Open;

        }

        public IGraph ServiceDescription
        {
            get
            {
                throw new NotImplementedException("TODO: find  way to get a SPARQL Service Description graph properly");
            }
        }

        public ConnectionState State
        {
            get
            {
                return _state;
            }
        }

        public IPrincipal User
        {
            get
            {
                return _currentUser;
            }
        }

        public object this[String name]
        {
            get
            {
                if (_parameters.ContainsKey(name)) return _parameters[name];
                return null;
            }
            set
            {
                _parameters[name] = value;
            }

        }

        internal void AssignParameters(SparqlParameterizedString command) {
            foreach (String paramName in ((Dictionary<String, INode>)command.Parameters).Keys)
            {
                object value = this[paramName];
                if (value != null)
                {
                }
                else
                {
                    command.SetParameter(paramName, RDFHelper.RdfNull);
                }
            }
        }

        public void Close()
        {
            RaiseReleased();
            _parameters.Clear();
            _underlyingStorage = null;
            // free all temporary reources
            _state = ConnectionState.Closed;
        }

        internal SparqlCommand CreateCommand(SparqlQuery query)
        {
            SparqlCommand command = new SparqlCommand(this, query);
            return command;
        }

        internal SparqlCommand CreateCommand(SparqlUpdateCommandSet updateSet)
        {
            SparqlCommand command = new SparqlCommand(this, updateSet);
            return command;
        }

        #region Events

        internal event ConnectionEventHandler Committed;
        internal event ConnectionEventHandler Rolledback;

        #endregion

        #region IUpdateableStorage implementation

        public IStorageServer ParentServer
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public object Query(string sparqlQuery)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlParameterizedString commandText = new SparqlParameterizedString(sparqlQuery);
            // in time, we should replace them by function calls for possible query caching
            AssignParameters(commandText);
            SparqlCommand command = CreateCommand(_queryParser.ParseFromString(commandText));
            _state.WithFlag(ConnectionState.Executing);
            object result = command.ExecuteReader();
            _state.WithoutFlag(ConnectionState.Executing);
            return result;
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            throw new NotImplementedException();
        }

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            throw new NotImplementedException();
        }

        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void LoadGraph(IRdfHandler handler, string graphUri)
        {
            throw new NotImplementedException();
        }

        public void SaveGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        public void Update(string sparqlUpdate)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlParameterizedString commandText = new SparqlParameterizedString(sparqlUpdate);
            // in time, we should replace them by function calls for possible query caching
            AssignParameters(commandText); 
            SparqlCommand command = CreateCommand(_updateParser.ParseFromString(commandText));
            _state.WithFlag(ConnectionState.Executing);
            command.ExecuteNonQuery();
            _state.WithoutFlag(ConnectionState.Executing);
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public void DeleteGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void DeleteGraph(string graphUri)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> ListGraphs()
        {
            throw new NotImplementedException();
        }

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the GetConfiguration
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the GetConfiguration
        public IOBehaviour IOBehaviour
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the GetConfiguration
        public bool UpdateSupported
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the GetConfiguration
        public bool DeleteSupported
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the GetConfiguration
        public bool ListGraphsSupported
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            this.Close();
        }

        #endregion

        #region ITransactionalStorage implementation

        public void Begin()
        {
            if (UnderlyingStorage is ITransactionalStorage)
            {
                ((ITransactionalStorage)UnderlyingStorage).Begin();
            }
            else
            {
                throw new NotImplementedException("TODO provide delegated transaction handling");
            }
        }

        public void Commit()
        {
            if (UnderlyingStorage is ITransactionalStorage)
            {
                ((ITransactionalStorage)UnderlyingStorage).Commit();
            }
            ConnectionEventHandler handler = Committed;
            if (handler != null)
            {
                handler.Invoke(this, new ConnectionEventArgs());
            }
        }

        public void Rollback()
        {
            if (UnderlyingStorage is ITransactionalStorage)
            {
                ((ITransactionalStorage)UnderlyingStorage).Rollback();
            }
            ConnectionEventHandler handler = Rolledback;
            if (handler != null)
            {
                handler.Invoke(this, new ConnectionEventArgs());
            }
        }

        #endregion

        #endregion


        #region Internal implementation

        internal override IQueryableStorage UnderlyingStorage
        {
            get
            {
                return _underlyingStorage;
            }
        }

        // TODO check what to do for update and cleaning of concurrent additions/removals graphs
        internal override SparqlTemporaryResourceMediator ParentContext
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region Events and helpers

        ///// <summary>
        ///// Event which is raised when a Graph is added
        ///// </summary>
        //event TripleStoreEventHandler GraphAdded;

        ///// <summary>
        ///// Event which is raised when a Graph is removed
        ///// </summary>
        //event TripleStoreEventHandler GraphRemoved;

        /// <summary>
        /// Event which is raised when a Graphs contents changes
        /// </summary>
        protected event ConnectionEventHandler GraphsChanged;

        /// <summary>
        /// Helper method for raising the <see cref="GraphsChanged">GraphsChanged</see> event
        /// </summary>
        /// <param name="args">List of the changed graphs' uri</param>
        /// TODO handle the fact that until commit, only the current connection should listen to this event...
        ///     => emit different events or handle different scoping like here and in TransactionLog/RuntimeLog ?
        internal void RaiseGraphsChanged(IEnumerable<Uri> args)
        {
            ConnectionEventHandler d = this.GraphsChanged;
            if (d != null)
            {
                d.Invoke(this, new GraphsChangedEventArgs(args));
            }
        }

        ///// <summary>
        ///// Event which is raised when a Graph is cleared
        ///// </summary>
        //event TripleStoreEventHandler GraphCleared;

        ///// <summary>
        ///// Event which is raised when a Graph has a merge operation performed on it
        ///// </summary>
        //event TripleStoreEventHandler GraphMerged;

        #endregion
    }
}
