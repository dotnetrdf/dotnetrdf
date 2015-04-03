using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Spin
{
    // TODO check the state for the connection on each method
    // TODO decide who is responsible for state maintenance (=> SparqlCommand class... ?)

    #region Event args and delegates

    internal class ConnectionEventArgs : EventArgs
    {
        internal ConnectionEventArgs()
            : base()
        {
        }
    }

    /// <summary>
    /// Delegate Type for SparqlCommand events
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Triple Event Arguments</param>
    internal delegate void ConnectionEventHandler(Object sender, ConnectionEventArgs args);

    #endregion

    /// <summary>
    /// The Connection class wraps a client's connection to a SPIN capable or enhanced RDF storage <todo>or SPARQLEndpoint</todo>.
    /// It provides for session variables management and ACID access to the storage.
    /// </summary>
    /// <remarks>
    /// To ensure the storage's data consistency, any access should be wrapped by instances of this class. Direct access is to be prohibited by any necessary mean.
    /// </remarks>
    public sealed class Connection
        : BaseTemporaryGraphConsumer, IUpdateableStorage, ITransactionalStorage
    {
        private static SparqlQueryParser _queryParser = new SparqlQueryParser();
        private static SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private String _id = Guid.NewGuid().ToString().Replace("-", "");

        private IQueryableStorage _underlyingStorage;
        private ConnectionState _state = ConnectionState.Closed;
        private IPrincipal _currentUser;

        #region Public implementation (IDbConnection-like)

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

        public void Close()
        {
            MakeDisposable();
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
            if (State != ConnectionState.Open) throw new ConnectionStateException();
            SparqlParameterizedString commandText = new SparqlParameterizedString(sparqlQuery);
            // TODO replace connection env parameters 
            // in time, we should replace them by function calls for possible query caching
            SparqlCommand command = CreateCommand(_queryParser.ParseFromString(commandText));
            //_state = ConnectionState.Executing;
            return command.ExecuteReader();
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
            if (State != ConnectionState.Open) throw new ConnectionStateException();
            SparqlParameterizedString commandText = new SparqlParameterizedString(sparqlUpdate);
            // TODO replace connection env parameters 
            // in time, we should replace them by function calls for possible query caching
            SparqlCommand command = CreateCommand(_updateParser.ParseFromString(commandText));
            //_state = ConnectionState.Executing;
            command.ExecuteNonQuery();
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
        internal override BaseTemporaryGraphConsumer ParentContext
        {
            get
            {
                return null;
            }
        }

        #endregion

    }
}
