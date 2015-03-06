using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using System.Transactions;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin
{
    // TODO check the state for the connection on each method

    /// <summary>
    /// The Connection class  a client's connection to a SPIN capable RDF storage <todo>or SPARQLEndpoint</todo>.
    /// It provides for session variables management, transaction and isolation. 
    /// </summary>
    public class Connection
        : IUpdateableStorage, ITransactionalStorage
    {

        private String _id = Guid.NewGuid().ToString().Replace("-", "");

        private IQueryableStorage _underlyingStorage;
        private ConnectionState _state = ConnectionState.Closed;
        private IPrincipal _currentUser;

        #region Public implementation

        public Connection()
        {
        }

        public void Open(IQueryableStorage storage)
        {
            UnderlyingStorage = storage;
            _state = ConnectionState.Open;
        }

        // 
        public void Open(IQueryableStorage storage, string userId, System.Security.SecureString password)
        {
            UnderlyingStorage = storage;
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

        internal String ID {
            get {
                return _id;
            }
        }

        internal Uri Uri { 
            get {
                return UriFactory.Create(TransactionLog.URI_PREFIX + ID);
            }
        }
        public void Close()
        {
            UnderlyingStorage = null;
            _state = ConnectionState.Closed;
        }


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
            SparqlCommand command = new SparqlCommand();
            command.Connection = this;
            return command.ExecuteReader(sparqlQuery);
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlProcessor.Query(this, rdfHandler, resultsHandler, sparqlQuery);
        }

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            SparqlProcessor.LoadGraph(this, g, graphUri);
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            SparqlProcessor.LoadGraph(this, g, graphUri);
        }

        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            SparqlProcessor.LoadGraph(this, handler, graphUri);
        }

        public void LoadGraph(IRdfHandler handler, string graphUri)
        {
            SparqlProcessor.LoadGraph(this, handler, graphUri);
        }

        public void SaveGraph(IGraph g)
        {
            SparqlProcessor.SaveGraph(this, g);
        }

        public void Update(string sparqlUpdate)
        {
            SparqlProcessor.Update(this, sparqlUpdate);
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            SparqlProcessor.UpdateGraph(this, graphUri, additions, removals);
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            SparqlProcessor.UpdateGraph(this, graphUri, additions, removals);
        }

        public void DeleteGraph(Uri graphUri)
        {
            SparqlProcessor.DeleteGraph(this, graphUri);
        }

        public void DeleteGraph(string graphUri)
        {
            SparqlProcessor.DeleteGraph(this, graphUri);
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

        private Dictionary<Uri, GraphMonitor> _graphs = new Dictionary<Uri, GraphMonitor>(RDFHelper.uriComparer);

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
            else
            {
                throw new NotImplementedException("TODO provide delegated transaction handling");
            }
        }

        public void Rollback()
        {
            if (UnderlyingStorage is ITransactionalStorage)
            {
                ((ITransactionalStorage)UnderlyingStorage).Rollback();
            }
            else
            {
                throw new NotImplementedException("TODO provide delegated transaction handling");
            }
        }

        #endregion

        #endregion


        #region Internal implementation

        internal IQueryableStorage UnderlyingStorage
        {
            get
            {
                return _underlyingStorage;
            }
            private set
            {
                _underlyingStorage = value;
            }
        }

        internal FeaturedSparqlProcessor SparqlProcessor
        {
            get
            {
                return FeaturedSparqlProcessor.Get(this);
            }
        }

        #endregion

    }
}
