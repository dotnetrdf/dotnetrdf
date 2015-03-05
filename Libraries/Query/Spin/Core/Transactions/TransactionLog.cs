using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core.Transactions
{

    internal delegate void TransactionEventHandler(BaseSpinTransaction sender, TransactionEventArgs args);

    /// <summary>
    /// The transactionLog class is responsible to log and track transactional events in a centralized distributed graph
    /// Transaction rewriting strategy should be provided by this class
    /// Make the methods static
    /// </summary>
    internal class TransactionLog
    {
        internal const String URI_PREFIX = "tag:dotnetrdf.org:transactions:";

        internal const String URI_INFIX_ADDITIONS = ":additions:";
        internal const String URI_INFIX_REMOVALS = ":removals:";

        // The patterns for the transaction additions and removals Uris to a graph
        internal const String URI_PATTERN_ADDITIONS = "STR(?txUri) ,'" + URI_INFIX_ADDITIONS + "', ENCODE_FOR_URI(STR(?graphBaseUri))";
        internal const String URI_PATTERN_REMOVALS = "STR(?txUri) ,'" + URI_INFIX_REMOVALS + "', ENCODE_FOR_URI(STR(?graphBaseUri))";

        /// <summary>
        /// The distributed transaction log's graph Uri
        /// </summary>
        private static Uri _graphUri = UriFactory.Create(URI_PREFIX + "log");

        /// <summary>
        /// A reference of locally managed transactions
        /// </summary>
        private Dictionary<Uri, BaseSpinTransaction> _transactions = new Dictionary<Uri, BaseSpinTransaction>(RDFHelper.uriComparer);

        private BaseSpinWrappedStorage _connection;

        /// <summary>
        /// Creates a new TransactioLog object for the storage
        /// </summary>
        /// <param name="connection"></param>
        internal TransactionLog(BaseSpinWrappedStorage connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Returns the latest snapshot Uri of a given graph for the connection
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        internal Uri GetSnapshotUri(Connection connection, Uri graphUri) {
            return graphUri;
        }

        #region events

        internal event TransactionEventHandler Committed;

        #endregion

        #region Log queries

        /*
         * Transactions vocabulary:
         * PREFIX trans: <urn:transactions#>
         *      trans:Commitable    type for updating transactions. Default type is ReadOnly
         *      trans:startedAt     timestamp the transaction's first command 
         *      trans:committedAt   timestamp for the transaction commit operation
         *      trans:updates       references a graph the transaction is currently trying to update 
         */
        // Notifies a transaction start to the log
        private String TX_START = "PREFIX trans: <urn:transactions#> INSERT { GRAPH "+ _graphUri.ToString() +" { ?txUri trans:startedAt ?txTimestamp . } } WHERE { BIND (NOW() as ?txTimestamp) FILTER NOT EXISTS { GRAPH "+ _graphUri.ToString() +" { ?txUri trans:startedAt ?anyPriorTime . } } } ";

        // Notifies a update to a graph for the transaction
        private String TX_WRITE = "PREFIX trans: <urn:transactions#> INSERT { GRAPH "+ _graphUri.ToString() +" { ?txUri a trans:Commitable . ?txUri trans:updated ?sourceGraph . } } ";

        // Notifies a rollback the transaction
        private String TX_ROLLBACK = "DELETE WHERE { GRAPH "+ _graphUri.ToString() +" { ?txUri ?p ?o . } }";

        // Applies a transaction updates to the store and updates the transaction log
        // TODO decide whether we want to impact concurrent transactions temporary graphs to provide for proper serialized isolation ?
        //      This should be done only for read-only transactions, write transaction should always use the real up-to-date graphs with their explicit changes so SPIN pipeline is effective throughout the store.
        private String TX_COMMIT = @"
PREFIX trans: <urn:transactions#>

# Makes writes conditional by checking whether there is a concurrent transaction commit pending
# Apply Changes to the original graphs if possible 
DELETE {
    # delete removed triples from the original graph
    GRAPH ?g {
        ?removedTsubject ?removedTpredicate ?removedTobject .
    }
## SHOULD WE DO THIS here or let the target transaction handle this ? delete added and retracted triples from the readonly transaction graphs
## note this could be 'optimized' for the target transaction by filtering the already read patterns but would cost more to the commit
#    GRAPH ?reanOnlyRemovalsRevert {
#        ?removedTsubject ?removedTpredicate ?removedTobject .
#    }
#    GRAPH ?reanOnlyAdditionsRevert {
#        ?addedTsubject ?addedTpredicate ?addedTobject .
#    }
}
INSERT {
    # write transaction log event for this transaction
    GRAPH "+ _graphUri.ToString() +@" {
        ?txUri trans:committedAt ?txTimestamp .
    }
    # add assertions to the original graph
    GRAPH ?g {
        ?addedTsubject ?addedTpredicate ?addedTobject .
    }
## SHOULD WE DO THIS here or let the target transaction handle this ? delete added and retracted triples from the readonly transaction graphs
## note this could be 'optimized' for the target transaction by filtering the already read patterns but would cost more to the commit
#    GRAPH ?reanOnlyAdditionsRevert {
#        ?removedTsubject ?removedTpredicate ?removedTobject .
#    }
#    GRAPH ?reanOnlyRemovalsRevert {
#        ?addedTsubject ?addedTpredicate ?addedTobject .
#    }
} WHERE {
    # initialisation and concurrency checking
    BIND (NOW() as ?txTimestamp) .
    ?txUri trans:startedAt ?txStart .
    
    # Changes handling

    # get the updated graphs uris
    ?txUri trans:updates ?g .
    BIND (IRI(CONCAT(str(?txUri) ,':removals:', str(?g))) as ?gRemovals)
    BIND (IRI(CONCAT(str(?txUri) ,':additions:', str(?g))) as ?gAdditions)

    # get the updates
    OPTIONAL { GRAPH ?gRemovals { ?removedTsubject ?removedTpredicate ?removedTobject . } }
    OPTIONAL { GRAPH ?gAdditions { ?addedTsubject ?addedTpredicate ?addedTobject . } }
    
#    #get references to read-only transactions
#    OPTIONAL {
#        ?readOnlyTx trans:startedAt ?readOnlyTxStart .
#        FILTER (false) # determine a definition for readonly transactions
#        BIND (IRI(CONCAT(str(?readOnlyTx) ,':removals:', str(?g))) as ?reanOnlyRemovalsRevert)
#        BIND (IRI(CONCAT(str(?readOnlyTx) ,':additions:', str(?g))) as ?reanOnlyAdditionsRevert)
#    }

    # Concurrency checks for required patterns, this should also optimally be handled beforehand during the transaction life-time
	FILTER NOT EXISTS {
		?concurrentTx trans:committedAt ?concurrentTxCommit .
		FILTER (?concurrentTxCommit > ?txStart && ?concurrentTxCommit < NOW())
		BIND (IRI(CONCAT(str(?concurrentTx) ,':removals:', str(?g))) as ?gConcurrentRemovals)
		BIND (IRI(CONCAT(str(?concurrentTx),':additions:', str(?g))) as ?gConcurrentAdditions)
			{ GRAPH ?gConcurrentRemovals { ?s ?p ?o . } }
			UNION 
			{ GRAPH ?gConcurrentAdditions { ?s ?p ?o . } }
		# here we must check for conflicts by a developing a union of the transaction required RepeatableRead patterns
		# TODO define what those patterns may encompass: would a list of triples patterns used by updates be sufficient ?
	}
}
";

        // Selects resources involved by any committed transaction that is not concurrent to a running transaction.
        private String GARBAGE_COLLECTION = @"
PREFIX trans: <urn:transactions#>
SELECT * WHERE {
  GRAPH "+ _graphUri.ToString() +@" {
    ?txUri trans:committedAt ?committed .
    ?txUri trans:updates ?g .
    BIND (IRI(CONCAT(str(?txUri) ,':removals:', str(?g))) as ?gRemovals)
    BIND (IRI(CONCAT(str(?txUri) ,':additions:', str(?g))) as ?gAdditions)
    FILTER NOT EXISTS {
      ?concurrentTx trans:startedAt ?start .
      FILTER NOT EXISTS { ?concurrentTx trans:committedAt ?anyCommit . }
      FILTER (?start < ?committed)
    }
  }
}
";

        #endregion
    }
}
