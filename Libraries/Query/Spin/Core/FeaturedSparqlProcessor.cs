using System;
using System.Collections.Generic;
using System.Transactions;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.SparqlStrategies;

namespace VDS.RDF.Query.Spin.Core
{
    /*
     * PERHAPS USE REAL MVCC design to avoid maintaining a diff per read transaction or find a way to mutualize the diff ?
     *  => define/use statistics to determine the best policy
     *  
     * THINGS TO CHECK :
     * INSERT/DELETE DATA updates : 
     *      1) start transaction
     *      2) update additions/removals for the graphs
     *      3) trim additions and removals depending on the real existing triples.
     * 
     * INSERT/DELETE WHERE updates:
     *      1) start transaction
     *      2?) capture read patterns 
     *      3?) check whether a concurrent transation updated those => this is the core of the OCC to determine
     *      
     */
    /// <summary>
    /// A class that decorates a simple IStorageProvider or Sparql Endpoint with SPIN capabilities.
    /// Implementors are responsible for maintaining ACID state
    /// </summary>
    /// <remarks>Due to how some IStorageProvider are implemented see StardogConnector transaction implementation) there is no advantage to encapsulate the storage.
    /// We should instead provide direct implementation for ISParqlQueryProcessor/ISparqlUpdateProcessor interfaces </remarks>
    /// <remarks>Required SPARQL interpretation strategies should be determined by exporation/configuration of the underlying storage capabilities
    /// For instance: 
    /// => Stardog provides some native transactional support so there is no need for a TransactionalRewritingStrategy
    /// </remarks>
    internal class FeaturedSparqlProcessor
    {

        static FeaturedSparqlProcessor()
        {
            VDS.RDF.Options.QueryExecutionTimeout = 10000;
        }

        /// <summary>
        /// Returns a FeaturedSparqlProcessor for the current connection
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        internal static FeaturedSparqlProcessor Get(Connection context) {
            if (context.UnderlyingStorage == null) throw new ConnectionStateException();
            return new FeaturedSparqlProcessor();
            throw new NotImplementedException("TODO Define how to get the processor");
        }

        /// <summary>
        /// Maintains a chain of the rewriting strategy types for the processor depending on the capabilities of the underlying storage
        /// </summary>
        private List<Type> _rewritingStrategyChain = new List<Type>();

        private List<Type> _evaluationStrategyChain = new List<Type>();

        private FeaturedSparqlProcessor()
        {
        }

        #region Sparql rewriters/strategies support for this storage ?

        /// <summary>
        /// Returns the rewriting strategy that is necessary to provide full features over the connection's underlying storage
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// TODO make this easily configurable/cached or whatever by using reflection on the _rewritingStrategyChain types
        internal SparqlRewriteStrategy GetRewriteStrategyFor(SparqlCommand command)
        {
            SparqlRewriteStrategy strategyChain = new SparqlRewriteStrategy(_rewritingStrategyChain);
            // TODO Use reflection 
            // TODO Get the TransactionSupportStrategy through the transaction log or configuration
            if (!(command.Connection.UnderlyingStorage is ITransactionalStorage)) strategyChain.Add(new TransactionSupportStrategy());
            //if (command.CommandType == SparqlCommandType.SparqlUpdate) strategyChain.AddLast(new GraphDiffMonitorStrategy());
            return strategyChain;
        }

        #endregion

        #region Transaction management

        // Related to the transaction scope
        // 1) The transaction is created in read-only mode for a connection on the first command
        // 2) The transaction is set to Serializable by default
        // 3) On explicit writes operation the transaction is promoted to a CommitableTransaction


        #endregion

        #region ISpinWrappedStorage implementation

        internal IGraph GetServiceDescription(Connection connection)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<Uri> SpinConfigurationGraphUris
        {
            get { throw new NotImplementedException(); }
        }

        internal object Query(Connection connection, string sparqlQuery)
        {
            // TODO maintain a SparqlCommand context to have copact reference to local/temporary/configuration resources
            throw new NotImplementedException();
        }

        internal void Query(Connection connection, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {

            throw new NotImplementedException();
        }

        internal void Update(Connection connection, string sparqlUpdate)
        {
            // TODO use a SparqlParameterizedString to allow substitution for connection settings
            Update(connection, new SparqlUpdateParser().ParseFromString(sparqlUpdate));
        }

        // TODO make this internal
        internal void Update(Connection connection, Update.SparqlUpdateCommandSet sparqlUpdateSet)
        {
            throw new NotImplementedException();
        }

        internal void ListGraphs(Connection connection)
        {
            throw new NotImplementedException();
        }

        internal void DeleteGraph(Connection connection, string graphUri)
        {
            DeleteGraph(connection, UriFactory.Create(graphUri));
        }

        internal void DeleteGraph(Connection connection, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        internal void LoadGraph(Connection connection, IGraph graph, string graphUri)
        {
            throw new NotImplementedException();
        }


        internal void LoadGraph(Connection connection, IGraph graph, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        internal void LoadGraph(Connection connection, IRdfHandler rdfHandler, string graphUri)
        {
            throw new NotImplementedException();
        }

        internal void LoadGraph(Connection connection, IRdfHandler rdfHandler, Uri graphUri)
        {
            throw new NotImplementedException();
        }

        internal void SaveGraph(Connection connection, IGraph graph)
        {
            throw new NotImplementedException();
        }

        internal void UpdateGraph(Connection connection, string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            UpdateGraph(connection, UriFactory.Create(graphUri), additions, removals);
        }

        /// <summary>
        /// Updates the underlying graph in the storage.
        /// If the Update is called within an explicit transaction no SPIN checking is performed
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="graphUri"></param>
        /// <param name="additions"></param>
        /// <param name="removals"></param>
        internal void UpdateGraph(Connection connection, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        internal bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        internal bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        internal Storage.IOBehaviour IOBehaviour
        {
            get { throw new NotImplementedException(); }
        }

        internal bool UpdateSupported
        {
            get { throw new NotImplementedException(); }
        }

        internal bool DeleteSupported
        {
            get { throw new NotImplementedException(); }
        }

        internal bool ListGraphsSupported
        {
            get { throw new NotImplementedException(); }
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region internal storage handling

        #endregion

    }
}
