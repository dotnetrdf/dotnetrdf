using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core
{

    /// <summary>
    /// This class 
    /// </summary>
    internal class SparqlFeaturesProvider
    {

        static SparqlFeaturesProvider()
        {
            VDS.RDF.Options.QueryExecutionTimeout = 10000;
        }

        /// <summary>
        /// Returns the SparqlFeaturesProvider instance required for the given connection
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        internal static SparqlFeaturesProvider Get(Connection context) {
            if (context.UnderlyingStorage == null) throw new ConnectionStateException();
            return new SparqlFeaturesProvider();
            throw new NotImplementedException("TODO Define how to get the processor");
        }

        /// <summary>
        /// Maintains a chain of the rewriting strategy types for the processor depending on the capabilities of the underlying storage
        /// </summary>
        private List<Type> _rewritingStrategyChain = new List<Type>();

        //private List<Type> _evaluationStrategyChain = new List<Type>();

        private SparqlFeaturesProvider()
        {
        }

        #region Sparql rewriters/strategies support for this storage ?

        /// <summary>
        /// Returns the rewriting strategy that is necessary to provide full features over the connection's underlying storage
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// TODO make this easily configurable/cached or whatever by using reflection on the _rewritingStrategyChain types
        internal SparqlRewriteStrategyChain GetRewriteStrategyFor(SparqlCommand command)
        {
            SparqlRewriteStrategyChain strategyChain = new SparqlRewriteStrategyChain(_rewritingStrategyChain);
            // TODO Use reflection 
            // TODO Get the TransactionSupportStrategy through the transaction log or configuration
            if (!(command.Connection.UnderlyingStorage is ITransactionalStorage)) strategyChain.Add(new TransactionSupportStrategy());
            //if (command.CommandType == SparqlCommandType.SparqlUpdate) strategyChain.AddLast(new GraphDiffMonitorStrategy());
            return strategyChain;
        }

        #endregion

        #region Internal implementation

        internal IGraph GetServiceDescription(Connection connection)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<Uri> SpinConfigurationGraphUris
        {
            get { throw new NotImplementedException(); }
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
