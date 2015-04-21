using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin
{
    // TODO do we really need a connection state ? If yes :
    //  => check the state for the connection on each method
    //  => decide where and how to maintain it correctly

    // TODO define how the Connection Class can be used more simply and transparently within a HttpApplication
    //      We should then :
    //          => make it creatable by direct API call (ConnectionString style ?). This implies to :
    //              => make sure the correct strategy is associated with the underlying server
    //                  => determine how we can define RDF "drivers" and check the driver corresponds with the underlying engine
    //                  => determine how the required SPIN graphs to use can be associated with a specific Storage by either:
    //                      => finding a way to force configuration
    //                      => perform a global storage exploration to get all statements with spin:imports predicate along with the graph they appear in
    //                          => this may be dangerous and would require having a security layer to be sure that normal users could not add such statement into the store
    //          => force named configuration of the remote service and create the connection using the alias

    // TODO Since we want to SPIN, it may be useful to wrap the storage within an internal security layer defined directly in the SPIN configuration
    //      => Such a layer could be handled as a ISparqlHandlingStrategy to ensure queries and updates are restricted accordlingly to the current user's rights
    //      => if present it may however be advisable that the module would affect any SPARQL1.1 ServiceDescription returned to the client (for instance return Dataset limitations, ie. the graphs the user can access)
    //          => check how this behaviour can be enforced or ignored from within the connection.

    // TODO define how to handle possible concurrent Spin configuration changes and what to do in these cases.
    //      Either rollback the transaction or try to rerun it from the start using the new configuration ?

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

        public IEnumerable<Uri> GraphUris
        {
            get
            {
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

    #endregion Event args and delegates

    /// <summary>
    /// The Connection class wraps a client's connection to a SPIN capable or enhanced RDF storage <todo>or SPARQLEndpoint</todo>.
    /// It also ensures session variables management and ACID access to the storage.
    /// </summary>
    /// <remarks>
    /// There can be only one active transaction per connection. All client commands are to be executed under the Serializable isolation regime.
    /// To ensure the storage's consistency, any access to the underlying storage should be wrapped by instances of this class. Direct access is to be prohibited by any necessary mean.
    /// Though we do not implement any other IDbConnection interface methods, I just added the Open methods to allow for storage to provide their own User Access/Rights layer
    /// </remarks>
    // TODO complete the IUpdateableStorage implementation
    // TODO define how to allow for Sparql compliant command parameters, either:
    //      => replace parameters with variables:
    //          PRO: simpler, doe not require complex text replacement
    //          CONS: limits usable names when parameters
    //      => define a dummy function
    //          PRO: more consistent with SWP ui:param() function, allows direct disambiguation from "legacy" sparql variables
    //          CONS: requires complex text replacement with possible namespace resolution and aliasing to avoid variable conflict
    //      TODO define how missing parameters should be handled (either UNDEF or dumb NULL value as done for SQL ?)
    public sealed class Connection
        : SparqlTemporaryResourceMediator, IUpdateableStorage, ITransactionalStorage
    {
        private SparqlQueryParser _queryParser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private SpinStorageProvider _sparqlHandler;
        private IUpdateableStorage _storage;

        private IGraph _serviceDescription = null;
        private ConnectionState _state = ConnectionState.Closed;

        private IPrincipal _currentUser;
        private Dictionary<string, INode> _parameters = new Dictionary<string, INode>();

        /// <summary>
        /// A client SPIN-capable connection over a physical IUpdateableStorage
        /// </summary>
        /// <param name="server"></param>
        /// <param name="storage"></param>
        internal Connection(SpinStorageProvider server, IUpdateableStorage storage)
            : base()
        {
            _sparqlHandler = server;
            _storage = storage;
            SpinModel model = SpinModel.Get(this);
            _queryParser.ExpressionFactories = new List<ISparqlCustomExpressionFactory>() { model };
            _updateParser.ExpressionFactories = new List<ISparqlCustomExpressionFactory>() { model };
        }

        #region Public implementation

        public void Open()
        {
            //if (State.HasFlag(ConnectionState.Open)) Close();
            _state |= ConnectionState.Open;
        }

        public void Open(string userId, System.Security.SecureString password)
        {
            if (State.HasFlag(ConnectionState.Open)) Close();
            // TODO set the userId env value
            Open();
        }

        public IGraph ServiceDescription
        {
            get
            {
                // TODO for all SparqlSDContributor, provide a INode or Uri for reference in the SD graph
                // TODO check wether the service description can be set for the storage or if it depends on the client's identity.
                //      => if not try to use a single graph for the storage
                if (_serviceDescription != null) return ServiceDescription;
                try
                {
                    _serviceDescription = UnderlyingStorage.ServiceDescription;
                }
                catch (NotSupportedException nse)
                {
                    _serviceDescription = new Graph();
                }
                // Enrich the description with the Sparql handlers informations if any provided.
                _serviceDescription.Merge(_sparqlHandler.SparqlSDContribution);
                // TODO try and determine the dataset usable by the client (either static for the storage or may be dependent on the current client's identity
                return _serviceDescription;
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

        public INode this[String name]
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

        /// <summary>
        /// Closes the connection
        /// </summary>
        /// <remarks>
        /// Since the underlying storage cannot be physically "closed", this comes to rolling back any unfinished transaction, freeing temporary resources, and clearing the connection parameters
        /// </remarks>
        public void Close()
        {
            // free all temporary resources
            RaiseReleased();
            _parameters.Clear();
            _serviceDescription = null;
            _state = ConnectionState.Closed;
        }

        #region Transaction events

        // TODO do we need to handle this

        internal event ConnectionEventHandler Aborted;

        // TODO split events between internal pre event and public complete event
        internal event ConnectionEventHandler Committed;

        internal event ConnectionEventHandler Rolledback;

        #endregion Transaction events

        #region IUpdateableStorage members

        public IStorageServer ParentServer
        {
            get
            {
                return null;// _strategyProvider;
            }
        }

        public object Query(string sparqlQuery)
        {
            return Query(_queryParser.ParseFromString(sparqlQuery));
        }

        public object Query(SparqlQuery sparqlQuery)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlCommand command = CreateCommand(sparqlQuery);

            object results;
            _state.SetFlag(ConnectionState.Executing);
            results = command.ExecuteReader();
            _state.RemoveFlag(ConnectionState.Executing);
            return results;
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            Query(rdfHandler, resultsHandler, _queryParser.ParseFromString(sparqlQuery));
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery sparqlQuery)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlCommand command = CreateCommand(sparqlQuery);
            _state.SetFlag(ConnectionState.Executing);
            command.ExecuteReader(rdfHandler, resultsHandler);
            _state.RemoveFlag(ConnectionState.Executing);
        }

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(new GraphHandler(g), graphUri);
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            LoadGraph(new GraphHandler(g), UriFactory.Create(graphUri));
        }

        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlParameterizedString commandText = new SparqlParameterizedString("CONSTRUCT FROM @_graphUri WHERE { ?s ?p ?o . }");
            // in time, we should replace parameters by function calls for possible direct parsing
            AssignParameters(commandText);
            commandText.SetUri("_graphUri", graphUri);
            SparqlCommand command = CreateCommand(_queryParser.ParseFromString(commandText));

            _state.SetFlag(ConnectionState.Executing);
            command.ExecuteReader(handler, null);
            _state.RemoveFlag(ConnectionState.Executing);
        }

        public void LoadGraph(IRdfHandler handler, string graphUri)
        {
            LoadGraph(handler, UriFactory.Create(graphUri));
        }

        public void SaveGraph(IGraph g)
        {
            // create a CLEAR command and serialize the graph into a ModifyCommand
            throw new NotImplementedException();
        }

        public void Update(string sparqlUpdate)
        {
            Update(_updateParser.ParseFromString(sparqlUpdate));
        }

        public void Update(SparqlUpdateCommandSet sparqlUpdate)
        {
            if (!State.HasFlag(ConnectionState.Open)) throw new ConnectionStateException();
            SparqlCommand command = CreateCommand(sparqlUpdate);
            _state.SetFlag(ConnectionState.Executing);
            command.ExecuteNonQuery();
            _state.RemoveFlag(ConnectionState.Executing);
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            // TODO tranform the additions and removals into GraphGraphPatterns and create a modify command
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public void DeleteGraph(Uri graphUri)
        {
            DropCommand command = new DropCommand(graphUri);
            Update(command.ToString());
        }

        public void DeleteGraph(string graphUri)
        {
            DeleteGraph(UriFactory.Create(graphUri));
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

        #endregion IUpdateableStorage members

        #region ITransactionalStorage members

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

        #endregion ITransactionalStorage members

        #endregion Public implementation

        #region Internal implementation

        // TODO find a more generic name
        internal SpinStorageProvider StorageProvider
        {
            get
            {
                return _sparqlHandler;
            }
        }

        internal override IUpdateableStorage UnderlyingStorage
        {
            get
            {
                return _storage;
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

        // TODO implement this so we can try and pass direct object references as parameters ?
        internal void AssignParameters(SparqlParameterizedString command)
        {
            foreach (String paramName in ((Dictionary<String, INode>)command.Parameters).Keys)
            {
                object value = this[paramName];
                if (value != null)
                {
                }
                else
                {
                    command.SetParameter(paramName, RuntimeHelper.BLACKHOLE);
                }
            }
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

        // TODO either
        //      1. provide generic methods to check whether a graph is being updated or requires isolation
        //          => define names that do not imply anything about possible transaction strategies
        //      2. make the strategies listen to events and maintain the case locally (better but more complex ?)

        #endregion Internal implementation

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
        /// Event which is raised when Graphs has been updated (changes committed) by other connections or processes
        /// </summary>
        /// <remarks>
        /// if we want to give priority to Spin model graphs, we may have to make this event cancellable
        /// or define how the connection must react whenever a configuration graph is changed (this will be driven from the SpinStrategy implementation)
        /// </remarks>
        /// TODO relocate this in a storage associated class (namely the TransactionLog that should be renamed to relate more to the storage notion
        internal event ConnectionEventHandler GraphsChanged;

        /// <summary>
        /// Event which is raised when a Graphs contents changes within the current transaction
        /// </summary>
        internal event ConnectionEventHandler GraphsUpdated;

        /// <summary>
        /// Helper method for raising the <see cref="GraphsUpdated">GraphsChanged</see> event
        /// </summary>
        /// <param name="args">List of the changed graphs' uri</param>
        /// TODO handle the fact that until commit, only the current connection should listen to this event...
        ///     => emit different events or handle different scoping like here and in TransactionLog/RuntimeLog ?
        internal void RaiseGraphsUpdated(IEnumerable<Uri> args)
        {
            ConnectionEventHandler d = this.GraphsUpdated;
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

        #endregion Events and helpers
    }
}