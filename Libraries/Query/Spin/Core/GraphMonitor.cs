using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;

namespace VDS.RDF.Query.Spin.Core
{
    // TODO define a class for CompiledGraphMonitor
    /// <summary>
    /// The GraphMonitor class monitors read/write activity on a graph during a transaction and validates concurrency control
    /// </summary>
    internal class GraphMonitor
        : IEnlistmentNotification, IDisposable
    {

        private Uri _baseUri;

        public GraphMonitor(Uri baseUri)
        {
            _baseUri = baseUri;
            if (Transaction.Current != null) Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
        }

        /// <summary>
        /// Returns the Uri of the graph being monitored by this instance
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
        }

        internal Uri GraphUri
        {
            get
            {
                Uri currentTempGraph = null;
                //if (Transaction.Current != null)
                    //currentTempGraph = UriFactory.Create(BASE_URI + Transaction.Current.TransactionInformation.LocalIdentifier.ToString() + ":" + Uri.EscapeDataString(BaseUri.ToString()));
                return currentTempGraph;
            }
        }

        #region Internal management


        #endregion

        #region IEnlistmentNotification implementation
        // Code handling transactional changes to a particular graph
        // What needs to be done is notifying all concurrent graph monitors of the committed changes and intercepting those changes to check the state of the graph


        public void Commit(Enlistment enlistment)
        {
            try
            {
                // TODO write an entry into the storage transaction log along with the changes in an ACID manner
                // TODO notify the local concurrent graph monitors of the change so they can check for concurrency ?
                HttpContext.Current.Response.Write("Committing changes");
                enlistment.Done();
            }
            finally
            {

            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {

            // TODO try and register the monitor into the underlying storage ?
            preparingEnlistment.Prepared();
            try
            {
            }
            catch (TransactionAbortedException ex)
            {
                preparingEnlistment.ForceRollback();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                // TODO write an entry into the storage transaction log along with the changes in an ACID manner
                // TODO notify the local concurrent graph monitors of the change so they can check for concurrency ?
                HttpContext.Current.Response.Write("Rollingback changes");
                enlistment.Done();
            }
            catch
            {
            }
            finally
            {

            }
        }

        #endregion

        public void Dispose()
        {

        }
    }
}
