using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Utility;
using System.Transactions;

namespace VDS.RDF.Query.Spin.Core.Transactions
{
    /// <summary>
    /// Spin Transactions are ACID:
    /// Atomicity: is achieved by tracking updates in temporary graphs that are written all at once at commit time. Depends on atomicity of Sparql updates by the server.
    /// Consistency:
    /// Isolation: read-only transactions use a snapshot obtained at first read and maintained by using the rollback segment of concurrent committed transactions
    /// Durability: is provided by the underlying server.
    /// </summary>
    internal class BaseSpinTransaction
        : IDisposable
    {

        private Uri _txUri;
        private DateTime? firstCommandTimeStamp = null;
        private TransactionLog _logger;

        private TransactionScopeOption _currentTxScope = TransactionScopeOption.Suppress;
        internal bool IsExplicit { get; set; }

        private Dictionary<Uri, GraphMonitor> _graphs = new Dictionary<Uri, GraphMonitor>(RDFHelper.uriComparer);

        internal BaseSpinTransaction(BaseSpinWrappedStorage storage)
            : this(storage, UriFactory.Create(TransactionLog.URI_PREFIX + Guid.NewGuid().ToString()))
        {
        }

        internal BaseSpinTransaction(BaseSpinWrappedStorage storage, Uri txUri)
        {
            _txUri = txUri;
            _logger = storage.TransactionLog;

            // Subscribe to the storage's transactionLog events
            _logger.Committed += Transaction_Committed;
        }

        internal Uri BaseUri {
            get {
                return _txUri;
            }
        }

        /// <summary>
        /// Returns the required scope to provide to associated System.TransactionScope objects
        /// </summary>
        internal TransactionScopeOption CurrentScope
        {
            get
            {
                return _currentTxScope;
            }
        }

        #region events

        event TransactionEventHandler Start;

        #endregion

        #region Global transactions event handlers

        /// <summary>
        /// Delegate for Commit events raised by the storage's TransactionLog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Transaction_Committed(BaseSpinTransaction sender, TransactionEventArgs args)
        {
            // TODO check if the commits is concurrent with the transaction ie modified triples conflict with the transaction read patterns.
            // if not, ignore the event
            // if it is, either create a local revert or abort the current transaction depending on read/write concurrency.

        }

        #endregion

        #region Local transaction event handlers 
        // these should not be events and be called directly by the SpinWrappedStorage on query completion with the underlying storage timestamp

        private void Read() 
        {
            // TODO use the storage timestamp instead
            if (firstCommandTimeStamp == null) Start(this, null);
            // TODO for each read pattern in the event, acquire a GraphMonitor object and pass it the read pattern.
            // TODO handle the case for CompiledGraphMonitor if not already used
        }

        private void Asserted()
        {
        }

        private void Retracted()
        {
        }

        #endregion

        public void Dispose()
        {
            if (_logger != null)
            {
                _logger.Committed -= Transaction_Committed;
            }
            // TODO send notifications so temporary resources can be disposed of if required
        }
    }
}
