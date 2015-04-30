/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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

    // TODO Since we want to SPIN, it may be useful or even recommended to provide a security layer to restrict users' possible updates or exploration of the SPIN model or other sensitive graphs
    //      => though there is a legacy Permissions Configuration feature, we may want to define a strategy do refine actions/rights granularity for the corresponding store.
    
    // TODO define how to handle possible concurrent Spin configuration changes and what to do in these cases.
    //      => Either rollback the transaction or try to rerun it from the start using the new configuration ?

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
    // TODO complete events definitions and handling
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
        /// <remarks>
        /// Connection should only be creatable through SpinStorageProvider.GetConnection call
        /// </remarks>
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
            //if (State.HasFlag(ConnectionState.Open)) Close();
            // TODO set the userId env value
            Open();
        }

        public IGraph ServiceDescription
        {
            get
            {
                // TODO for all SparqlSDContributor, provide a INode or Uri for reference in the SD graph
                // TODO check wether the service description can be set for the storage or if it depends on the client's identity.
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
                // TODO try and determine the dataset usable by the client (either static for the storage or may be dependent on the current client's identity)
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

        /// <summary>
        /// Gets/Sets a parameter value for this connection
        /// </summary>
        /// <remarks>
        /// Any nulled/undefined parameter is currently subsituted by an unbound variable upon execution.
        /// TODO this may cause some security issues. Check whether it is our responsibility to handle any policy
        /// </remarks>
        /// <param name="name"></param>
        /// <returns></returns>
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

        // TODO maybe split events into an internal Start/Requested event and a public one so we can ensure that all work is done correctly before notifying any external resource ?
        internal event ConnectionEventHandler Committed;

        internal event ConnectionEventHandler Rolledback;

        #endregion Transaction events

        #region IUpdateableStorage members

        // TODO check wether we can return anything safely ?
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
            // TODO tranform the additions and removals into a SPARQL1.1 UpdateCommand
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

        // TODO base this on the connection's current configuration
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the connection's current configuration
        public IOBehaviour IOBehaviour
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the connection's current configuration
        public bool UpdateSupported
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the connection's current configuration
        public bool DeleteSupported
        {
            get { throw new NotImplementedException(); }
        }

        // TODO base this on the connection's current configuration
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        internal void AssignParameters(SparqlParameterizedString command)
        {
            foreach (String paramName in _parameters.Keys)
            {
                command.SetParameter(paramName, this[paramName]);
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
        /// Event which is raised when Graphs has been updated (changes committed) outside of this connection
        /// </summary>
        /// <remarks>
        /// if we want to give priority to Spin model graphs, we may have to make this event cancellable
        /// or define how the connection must react whenever a configuration graph is changed (this will be driven from the SpinStrategy implementation)
        /// </remarks>
        /// TODO relocate this in the StorageRuntimeMonitor class
        internal event ConnectionEventHandler GraphsChanged;

        /// <summary>
        /// Event which is raised when a Graph contents changes within the current transaction
        /// </summary>
        internal event ConnectionEventHandler GraphsUpdated;

        /// <summary>
        /// Helper method for raising the <see cref="GraphsUpdated">GraphsChanged</see> event
        /// </summary>
        /// <param name="args">List of the changed graphs' uri</param>
        internal void RaiseGraphsUpdated(IEnumerable<Uri> args)
        {
            ConnectionEventHandler d = this.GraphsUpdated;
            if (d != null)
            {
                d.Invoke(this, new GraphsChangedEventArgs(args));
            }
        }

        // YET UNUSED 
        ///// <summary>
        ///// Event which is raised when a Graph is cleared
        ///// </summary>
        //event TripleStoreEventHandler GraphCleared;

        // YET UNUSED 
        ///// <summary>
        ///// Event which is raised when a Graph has a merge operation performed on it
        ///// </summary>
        //event TripleStoreEventHandler GraphMerged;

        #endregion Events and helpers
    }
}