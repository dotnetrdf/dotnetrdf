using System;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core.Transactions
{

    // TODO refactor this into a StorageMonitor or something ?
    /// <summary>
    /// The transactionLog class is responsible to log and track transactional events in a centralized distributed graph on a specific storage
    /// </summary>
    internal class TransactionLog
    {

        /// <summary>
        /// The distributed transaction log's graph Uri
        /// </summary>
        internal static Uri TRANSACTION_LOG_URI = UriFactory.Create(DOTNETRDF_TRANS.BASE_URI + "-log");

        /// <summary>
        /// A reference of locally managed transactions
        /// </summary>
        //private Dictionary<Uri, BaseSpinTransaction> _transactions = new Dictionary<Uri, BaseSpinTransaction>(RDFHelper.uriComparer);

        // TODO make this threaded while the connection's State is either Fetching or Executing
        internal static void Ping(Connection connection) { 
            if (!(connection.UnderlyingStorage is IUpdateableStorage)) return;
            IUpdateableStorage storage = (IUpdateableStorage)connection.UnderlyingStorage;
            SparqlParameterizedString pingCommand = new SparqlParameterizedString(@"
WITH @transactionLog
INSERT {
    ?s @startedAtUri ?startdDate .
}
WHERE {
    FILTER NOT EXISTS { ?s @startedAtUri ?anyStartDate . }
    BIND (NOW() as ?startdDate)
};

WITH @transactionLog
DELETE {
    ?s @lastAccessUri ?lastAccessDate .
}
INSERT {
    ?s @lastAccessUri ?now .
}
WHERE {
    BIND (NOW() as ?now)
    OPTIONAL {
        ?s @lastAccessUri ?lastAccessDate .
    }
};
");
            pingCommand.SetParameter("transactionLog", RDFHelper.CreateUriNode(TRANSACTION_LOG_URI));
            pingCommand.SetParameter("lastAccessUri", DOTNETRDF_TRANS.PropertyLastAccess);
            pingCommand.SetParameter("startedAtUri", DOTNETRDF_TRANS.PropertyStartedAt);
            pingCommand.SetVariable("s", RDFHelper.CreateUriNode(connection.Uri));
            storage.Update(pingCommand.ToString());
        }

        #region events


        //internal event TransactionEventHandler Committed;

        #endregion

        /// DEPRECATED this is handled directly in the SparqlHandlerStrategies
//        #region Log queries

//        /*
//         * Transactions vocabulary:
//         * PREFIX trans: <urn:transactions#>
//         *      trans:Commitable    type for updating transactions. Default type is ReadOnly
//         *      trans:startedAt     timestamp the transaction's first command 
//         *      trans:committedAt   timestamp for the transaction commit operation
//         *      trans:updates       references a graph the transaction is currently trying to update 
//         */
//        // Notifies a transaction start to the log
//        private SparqlParameterizedString TX_START = new SparqlParameterizedString("PREFIX trans: <urn:transactions#> INSERT { GRAPH @transactionLog { ?txUri trans:startedAt ?txTimestamp . } } WHERE { BIND (NOW() as ?txTimestamp) FILTER NOT EXISTS { GRAPH @transactionLog { ?txUri trans:startedAt ?anyPriorTime . } } } ");

//        // Notifies a update to a graph for the transaction
//        private SparqlParameterizedString TX_WRITE = new SparqlParameterizedString("PREFIX trans: <urn:transactions#> INSERT { GRAPH @transactionLog { ?txUri a trans:Commitable . ?txUri trans:updated ?sourceGraph . } } ");

//        // Notifies a rollback the transaction
//        private SparqlParameterizedString TX_ROLLBACK = new SparqlParameterizedString("DELETE WHERE { GRAPH @transactionLog { ?txUri ?p ?o . } }");

//        // Applies a transaction updates to the store and updates the transaction log
//        // TODO decide whether we want to impact concurrent transactions temporary graphs to provide for proper serialized isolation ?
//        //      This should be done only for read-only transactions, write transaction should always use the real up-to-date graphs with their explicit changes so SPIN pipeline is effective throughout the store.
//        private SparqlParameterizedString TX_COMMIT =new SparqlParameterizedString(@"
//PREFIX trans: <urn:transactions#>
//
//# Makes writes conditional by checking whether there is a concurrent transaction commit pending
//# Apply Changes to the original graphs if possible 
//DELETE {
//    # delete removed triples from the original graph
//    GRAPH ?g {
//        ?removedTsubject ?removedTpredicate ?removedTobject .
//    }
//## SHOULD WE DO THIS here or let the target transaction handle this ? delete added and retracted triples from the readonly transaction graphs
//## note this could be 'optimized' for the target transaction by filtering the already read patterns but would cost more to the commit
//#    GRAPH ?reanOnlyRemovalsRevert {
//#        ?removedTsubject ?removedTpredicate ?removedTobject .
//#    }
//#    GRAPH ?reanOnlyAdditionsRevert {
//#        ?addedTsubject ?addedTpredicate ?addedTobject .
//#    }
//}
//INSERT {
//    # write transaction log event for this transaction
//    GRAPH @transactionLog {
//        ?txUri trans:committedAt ?txTimestamp .
//    }
//    # add assertions to the original graph
//    GRAPH ?g {
//        ?addedTsubject ?addedTpredicate ?addedTobject .
//    }
//## SHOULD WE DO THIS here or let the target transaction handle this ? delete added and retracted triples from the readonly transaction graphs
//## note this could be 'optimized' for the target transaction by filtering the already read patterns but would cost more to the commit
//#    GRAPH ?reanOnlyAdditionsRevert {
//#        ?removedTsubject ?removedTpredicate ?removedTobject .
//#    }
//#    GRAPH ?reanOnlyRemovalsRevert {
//#        ?addedTsubject ?addedTpredicate ?addedTobject .
//#    }
//} WHERE {
//    # initialisation and concurrency checking
//    BIND (NOW() as ?txTimestamp) .
//    ?txUri trans:startedAt ?txStart .
//    
//    # Changes handling
//
//    # get the updated graphs uris
//    ?txUri trans:updates ?g .
//    BIND (IRI(CONCAT(str(?txUri) ,':removals:', str(?g))) as ?gRemovals)
//    BIND (IRI(CONCAT(str(?txUri) ,':additions:', str(?g))) as ?gAdditions)
//
//    # get the updates
//    OPTIONAL { GRAPH ?gRemovals { ?removedTsubject ?removedTpredicate ?removedTobject . } }
//    OPTIONAL { GRAPH ?gAdditions { ?addedTsubject ?addedTpredicate ?addedTobject . } }
//    
//#    #get references to read-only transactions
//#    OPTIONAL {
//#        ?readOnlyTx trans:startedAt ?readOnlyTxStart .
//#        FILTER (false) # determine a definition for readonly transactions
//#        BIND (IRI(CONCAT(str(?readOnlyTx) ,':removals:', str(?g))) as ?reanOnlyRemovalsRevert)
//#        BIND (IRI(CONCAT(str(?readOnlyTx) ,':additions:', str(?g))) as ?reanOnlyAdditionsRevert)
//#    }
//
//    # Concurrency checks for required patterns, this should also optimally be handled beforehand during the transaction life-time
//	FILTER NOT EXISTS {
//		?concurrentTx trans:committedAt ?concurrentTxCommit .
//		FILTER (?concurrentTxCommit > ?txStart && ?concurrentTxCommit < NOW())
//		BIND (IRI(CONCAT(str(?concurrentTx) ,':removals:', str(?g))) as ?gConcurrentRemovals)
//		BIND (IRI(CONCAT(str(?concurrentTx),':additions:', str(?g))) as ?gConcurrentAdditions)
//			{ GRAPH ?gConcurrentRemovals { ?s ?p ?o . } }
//			UNION 
//			{ GRAPH ?gConcurrentAdditions { ?s ?p ?o . } }
//		# here we must check for conflicts by a developing a union of the transaction required RepeatableRead patterns
//		# TODO define what those patterns may encompass: would a list of triples patterns used by updates be sufficient ?
//	}
//}
//");

//        // Selects resources involved by any committed transaction that is not concurrent to a running transaction.
//        private SparqlParameterizedString GARBAGE_COLLECTION = new SparqlParameterizedString(@"
//PREFIX trans: <urn:transactions#>
//SELECT * WHERE {
//  GRAPH @transactionLog {
//    ?txUri trans:committedAt ?committed .
//    ?txUri trans:updates ?g .
//    BIND (IRI(CONCAT(str(?txUri) ,':removals:', str(?g))) as ?gRemovals)
//    BIND (IRI(CONCAT(str(?txUri) ,':additions:', str(?g))) as ?gAdditions)
//    FILTER NOT EXISTS {
//      ?concurrentTx trans:startedAt ?start .
//      FILTER NOT EXISTS { ?concurrentTx trans:committedAt ?anyCommit . }
//      FILTER (?start < ?committed)
//    }
//  }
//}
//");

//        #endregion
    }
}
