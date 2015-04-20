using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    #region Transaction support vocabulary

    // TODO relocate this into the Runtime namespace
    public static class DOTNETRDF_TRANS
    {

        public const String BASE_URI = "tmp:dotnetrdf.org:transactions";
        public const String NS_URI = BASE_URI + "#";
        public const String RES_URI = BASE_URI + ":";
        public const String PREFIX = "trans";

        public readonly static Uri NoGraph = RDFHelper.RdfNull.Uri;

        public readonly static IUriNode ClassTransaction = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Transaction"));
        public readonly static IUriNode ClassConcurrentAssertionsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ConcurrentAssertionsGraph")); // use this as a class ?
        public readonly static IUriNode ClassConcurrentRemovalsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ConcurrentRemovalsGraph"));
        public readonly static IUriNode ClassPendingAssertionsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "PendingAssertionsGraph"));
        public readonly static IUriNode ClassPendingRemovalsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "PendingRemovalsGraph"));
        public readonly static IUriNode ClassTemporaryGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "TemporaryGraph"));

        public readonly static IUriNode PropertyAffectsGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "affectsGraph"));
        public readonly static IUriNode PropertyCommittedAt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "committedAt"));
        public readonly static IUriNode PropertyHasDefaultGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasDefaultGraph"));
        public readonly static IUriNode PropertyHasNamedGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasNamedGraph"));
        public readonly static IUriNode PropertyHasScope = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasScope"));
        public readonly static IUriNode PropertyLastAccess = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "lastAccess"));
        public readonly static IUriNode PropertyMonitors = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "monitors"));
        public readonly static IUriNode PropertyRequiredBy = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "requiredBy"));
        public readonly static IUriNode PropertyStartedAt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "startedAt"));

    }

    #endregion

    /// <summary>
    /// This rewrite Strategy is responsible for :
    ///     => handling commands ACIDity
    ///     => providing full Serializable isolation to the clients during transaction.
    ///     => issuing the graphs to allow for any constraint checking at commit
    /// </summary>
    /// <remarks>
    /// TODO provide Sparql1.1 ServiceDescription informations about this module.
    /// 
    /// TODO relocate the events' sparql template sowhere for better maintenance
    /// TODO add a transaction isolation level property to the connection that is either SERIALIZABLE or READ_COMMITTED for the contraints check processing
    /// TODO alleviate the transaction commit by reference graphs that are read by a concurrent connection so we do not have to create concurrentUpdates graphs if the graph has not been used
    ///     => for a future optimized process, the strategy would also reference triple patterns read for each graph
    /// TODO make the rewritter used even for transactional storage so we can rewrite the inserts into temporary graphs for possible constraint checking
    /// TODO checkproof the event attach/detach cycle depending on whether the object is reusable or not
    /// </remarks>
    /// TODO later it may be cleaner to segment the split command metas bewteen the trnasaction log for command dependencies and a dedicated graph for graph dependencies
    public sealed class TransactionSupportStrategy
        : ISparqlHandlingStrategy
    {
        private static readonly IUriNode RESOURCE = RDFHelper.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org/spin/sparqlStrategies#FullTransactionSupport"));
        //private static readonly IGraph SD_CONTRIBUTION = new Graph();

        //static TransactionSupportStrategy()
        //{
        //    // TODO initialise the SD_CONTRIBUTION graph better;
        //    SD_CONTRIBUTION.Assert(RESOURCE, RDF.PropertyType, SD.ClassFeature);
        //}


        // TODO refactor templates using named parameters instead of indexed
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>{0}: command ID</remarks>
        /// <remarks>{1}: predicate mapped ID</remarks>
        public static String COMPILATION_GRAPH_PREFIX_TEMPLATE = "tmp:dotnetrdf.org:{0}:compiledPattern:{1}#";
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>{0}: connection ID</remarks>
        /// <remarks>{1}: graph Token (either Uri/Variable/NAMED/DEFAULT</remarks>
        public static String CONCURRENT_ASSERTIONS_GRAPH_PREFIX_TEMPLATE = "tmp:dotnetrdf.org:{0}:concurrentAssertionsFor#";
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>{0}: connection ID</remarks>
        /// <remarks>{1}: graph Token (either Uri/Variable/NAMED/DEFAULT</remarks>
        public static String CONCURRENT_REMOVALS_GRAPH_PREFIX_TEMPLATE = "tmp:dotnetrdf.org:{0}:concurrentRemovalsFor#";
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>{0}: connection ID or Command ID</remarks>
        /// <remarks>{1}: graph Token (either Uri/Variable/NAMED/DEFAULT</remarks>
        public static String PENDING_ASSERTIONS_GRAPH_PREFIX_TEMPLATE = "tmp:dotnetrdf.org:{0}:pendingAdditionsFor#";
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>{0}: connection ID or Command ID</remarks>
        /// <remarks>{1}: graph Token (either Uri/Variable/NAMED/DEFAULT</remarks>
        public static String PENDING_REMOVALS_GRAPH_PREFIX_TEMPLATE = "tmp:dotnetrdf.org:{0}:pendingRemovalsFor#";

        private static Dictionary<String, IGraph> _commandMetas = new Dictionary<String, IGraph>();

        // Maintain a list of graph changed or updated per connection to optimize rewriting
        // TODO checkproof the isolationRequirement management since it should be event driven and may occur during the rewriting process
        private static Dictionary<String, HashSet<Uri>> _isolationRequirements = new Dictionary<String, HashSet<Uri>>();
        private static Dictionary<String, HashSet<Uri>> _updatedGraphs = new Dictionary<String, HashSet<Uri>>();

        public TransactionSupportStrategy()
        {
        }

        #region ISparqlSDPlugin members

        public INode Resource
        {
            get
            {
                return RESOURCE;
            }
        }

        public IGraph SparqlSDContribution
        {
            get
            {
                return new Graph();
            }
        }

        #endregion

        public void Handle(SparqlCommandUnit command)
        {
            command.Context.ExecutionStarted -= this.SparqlCommand_ExecutionStarted;
            command.Context.ExecutionStarted += this.SparqlCommand_ExecutionStarted;
            command.ExecutionStarted -= this.SparqlCommand_ExecutionStarted;
            command.ExecutionStarted += this.SparqlCommand_ExecutionStarted;
            new CommandRewriter(this).Rewrite(command);
        }

        internal void AddMetas(String objectRef, IEnumerable<Triple> metas)
        {
            IGraph objectMetas = (_commandMetas.ContainsKey(objectRef)) ? _commandMetas[objectRef] : null;
            if (objectMetas == null)
            {
                objectMetas = new ThreadSafeGraph();
                _commandMetas[objectRef] = objectMetas;
            }
            objectMetas.Assert(metas);
        }

        internal bool RequiresIsolationFor(SparqlCommandUnit command, IToken graphToken)
        {
            // TODO handle possible temp graphs references
            switch (graphToken.TokenType)
            {
                case Token.DEFAULT:
                    return _isolationRequirements.ContainsKey(command.Connection.ID) && _isolationRequirements[command.Connection.ID].Intersect(command.DefaultGraphs).Any();
                case Token.NAMED:
                    return _isolationRequirements.ContainsKey(command.Connection.ID) && _isolationRequirements[command.Connection.ID].Intersect(command.NamedGraphs).Any();
                case Token.URI:
                    return _isolationRequirements.ContainsKey(command.Connection.ID) && _isolationRequirements[command.Connection.ID].Contains(UriFactory.Create(graphToken.Value));
                case Token.VARIABLE:
                    return false;
                default:
                    throw new ArgumentException("Invalid token type.");
            }
        }

        internal bool RequiresUpdatesFor(SparqlCommandUnit command, IToken graphToken)
        {
            // TODO handle possible temp graphs references
            switch (graphToken.TokenType)
            {
                case Token.DEFAULT:
                    return _updatedGraphs.ContainsKey(command.Connection.ID) && _updatedGraphs[command.Connection.ID].Intersect(command.DefaultGraphs).Any();
                case Token.NAMED:
                    return _updatedGraphs.ContainsKey(command.Connection.ID) && _updatedGraphs[command.Connection.ID].Intersect(command.NamedGraphs).Any();
                case Token.URI:
                    return _updatedGraphs.ContainsKey(command.Connection.ID) && _updatedGraphs[command.Connection.ID].Contains(UriFactory.Create(graphToken.Value));
                case Token.VARIABLE:
                    return false;
                default:
                    throw new ArgumentException("Invalid token type.");
            }
        }

        #region Event handlers

        /* Connection.Committed
         * => update the graphs using connection's pending additions/removals graphs
         */
        internal void OnConnectionCommitted(Object sender, ConnectionEventArgs args)
        {
            Connection connection = (Connection)sender;
            IUpdateableStorage storage = (IUpdateableStorage)connection.UnderlyingStorage;
            // Apply changes to the graph
            SparqlParameterizedString command = new SparqlParameterizedString(@"
SELECT DISTINCT ?tempGraph ?affectedGraph
FROM @transactionLog 
WHERE {
    ?tempGraph @requiredBy @transUri .
    ?tempGraph @affectsGraph ?affectedGraph .
    FILTER (!sameTerm(?tempGraph, @RdfNull))
}");
            command.SetParameter("transactionLog", RDFHelper.CreateUriNode(TransactionLog.TRANSACTION_LOG_URI));
            command.SetParameter("committedAt", DOTNETRDF_TRANS.PropertyCommittedAt);
            command.SetParameter("startedAt", DOTNETRDF_TRANS.PropertyStartedAt);
            command.SetParameter("requiredBy", DOTNETRDF_TRANS.PropertyRequiredBy);
            command.SetParameter("pendingAssertions", DOTNETRDF_TRANS.ClassPendingAssertionsGraph);
            command.SetParameter("pendingRemovals", DOTNETRDF_TRANS.ClassPendingRemovalsGraph);
            command.SetParameter("concurrentAssertions", DOTNETRDF_TRANS.ClassConcurrentAssertionsGraph);
            command.SetParameter("concurrentRemovals", DOTNETRDF_TRANS.ClassConcurrentRemovalsGraph);
            command.SetParameter("affectsGraph", DOTNETRDF_TRANS.PropertyAffectsGraph);
            command.SetParameter("transUri", RDFHelper.CreateUriNode(connection.Uri));
            command.SetParameter("RdfNull", RDFHelper.RdfNull);

            SparqlResultSet metas = (SparqlResultSet)storage.Query(command.ToString());
            IEnumerable<Uri> affectedGraphs = metas.Results.Select(r => ((IUriNode)r.Value("affectedGraph")).Uri).Distinct();
            IEnumerable<Uri> pendingUpdatesGraph = metas.Results.Select(r => ((IUriNode)r.Value("tempGraph")).Uri);
            if (pendingUpdatesGraph.Any())
            {
                StringBuilder usingNamedSB = new StringBuilder();
                foreach (Uri graphUri in pendingUpdatesGraph)
                {
                    usingNamedSB.AppendLine("USING NAMED <" + graphUri.ToString() + ">");
                }
                command.CommandText = @"
DELETE {
    GRAPH ?g { ?pendingR_S  ?pendingR_P  ?pendingR_O . }
    GRAPH ?concurrentA { ?pendingR_S  ?pendingR_P  ?pendingR_O . }
    GRAPH ?concurrentR { ?pendingA_S  ?pendingA_P  ?pendingA_O . }
}
INSERT {
    GRAPH @transactionLog { 
        @transUri @committedAt ?commitTime . 
        ?concurrentA a @concurrentAssertions .     # is this necessary ?
        ?concurrentA @affectsGraph ?g .                 # is this necessary ?
        ?concurrentA @requiredBy ?concurrentTrans . 
        ?concurrentR a @concurrentRemovals .       # is this necessary ?
        ?concurrentR @affectsGraph ?g .                 # is this necessary ?
        ?concurrentR @requiredBy ?concurrentTrans .
    }
    GRAPH ?g { ?pendingA_S  ?pendingA_P  ?pendingA_O . }
    GRAPH ?concurrentA { ?pendingA_S  ?pendingA_P  ?pendingA_O . }
    GRAPH ?concurrentR { ?pendingR_S  ?pendingR_P  ?pendingR_O . }
}
USING NAMED @transactionLog 
" + usingNamedSB.ToString() + @"
WHERE {
    BIND (now() as ?commitTime)
    GRAPH @transactionLog {
        OPTIONAL {
            ?concurrentTrans @startedAt ?startDate .
            FILTER (!sameTerm(?concurrentTrans, @transUri))
            FILTER NOT EXISTS { ?concurrentTrans @committedAt ?anyDate . }
        }
        OPTIONAL 
        {
            ?pendingA @requiredBy @transUri .
            ?pendingA a @pendingAssertions .
            ?pendingA @affectsGraph ?g .
            GRAPH ?pendingA { ?pendingA_S  ?pendingA_P  ?pendingA_O . }
        }
        OPTIONAL 
        {
            ?pendingR @requiredBy @transUri .
            ?pendingR a @pendingRemovals .
            ?pendingR @affectsGraph ?g .
            GRAPH ?pendingR { ?pendingR_S  ?pendingR_P  ?pendingR_O . }
        }
   }    
    BIND (COALESCE(IRI(CONCAT(STR(?concurrentTrans), ':concurrentAssertionsFor#', STR(?g))), @RdfNull) as ?concurrentA)
    BIND (COALESCE(IRI(CONCAT(STR(?concurrentTrans), ':concurrentRemovalsFor#', STR(?g))), @RdfNull) as ?concurrentR)
};

DROP SILENT GRAPH @RdfNull;
";

                storage.Update(command.ToString());
                // TODO complete the distributed layer so other clients are also made aware of the changes
                //TransactionLog.RaiseGraphsChanged(affectedGraphs);
            }
        }


        /* Connection.Rolledback
         * => nothing to to
         */
        internal void OnConnectionRolledBack(Object sender, ConnectionEventArgs args)
        {
        }

        /* Connection.Rolledback
         * => nothing to to
         */
        internal void OnStorageGraphsChanged(Object sender, ConnectionEventArgs args)
        {
        }

        /* Connection.Disposed
         * => clear all connection's resources
         */
        internal void DetachFromConnection(Object sender)
        {
            Connection connection = (Connection)sender;
            connection.Committed -= this.OnConnectionCommitted;
            connection.Rolledback -= this.OnConnectionRolledBack;
            connection.Released -= this.OnMediatorReleased;
            connection.Released -= this.DetachFromConnection;

            if (_isolationRequirements.ContainsKey(connection.ID))
            {
                _isolationRequirements.Remove(connection.ID);
            }
            if (_updatedGraphs.ContainsKey(connection.ID))
            {
                _updatedGraphs.Remove(connection.ID);
            }
        }

        /* SparqlCommand.ExecutionStarted
         * => copy the connection's pending assertions/removals to the command ones
         */
        internal void SparqlCommand_ExecutionStarted(Object sender, SparqlExecutableEventArgs args)
        {
            SparqlExecutable command = (SparqlExecutable)sender;
            // TODO centralize connection events bindings into another method
            command.Connection.GraphsChanged -= this.OnStorageGraphsChanged;
            command.Connection.GraphsChanged += this.OnStorageGraphsChanged;
            command.Connection.Committed -= this.OnConnectionCommitted;
            command.Connection.Committed += this.OnConnectionCommitted;
            command.Connection.Rolledback -= this.OnConnectionRolledBack;
            command.Connection.Rolledback += this.OnConnectionRolledBack;
            command.Connection.Released -= this.OnMediatorReleased;
            command.Connection.Released += this.OnMediatorReleased;
            command.Connection.Released -= this.DetachFromConnection;
            command.Connection.Released += this.DetachFromConnection;

            command.Failed += this.OnCommandFailure;
            command.Succeeded += this.OnCommandSuccess;
            command.Released += this.OnMediatorReleased;

            if (_commandMetas.ContainsKey(command.ID) && _commandMetas[command.ID] != null)
            {
                // First add a dependency to the command's parent context for possible garbage collection in case of crash
                _commandMetas[command.ID].Assert(RDFHelper.CreateUriNode(command.Uri), DOTNETRDF_TRANS.PropertyRequiredBy, RDFHelper.CreateUriNode(command.ParentContext.Uri));
                // Then adds the commands metas
                command.UnderlyingStorage.UpdateGraph(TransactionLog.TRANSACTION_LOG_URI, _commandMetas[command.ID].Triples, null);
                // For SparqlCommand, copy the read transaction's pendingUpdate graphs into the Command's scope to provide for correct handling of Command's interruption if needed
                if (command is SparqlCommand)
                {
                    IUpdateableStorage storage = (IUpdateableStorage)command.UnderlyingStorage;
                    SparqlParameterizedString graphCopy = new SparqlParameterizedString("COPY SILENT GRAPH @sourceUri TO @targetUri");
                    foreach (IUriNode targetGraph in _commandMetas[command.ID].GetTriplesWithPredicate(DOTNETRDF_TRANS.PropertyAffectsGraph).Select(t => (IUriNode)t.Subject))
                    {
                        graphCopy.SetParameter("targetUri", targetGraph);
                        graphCopy.SetParameter("sourceUri", RDFHelper.CreateUriNode(UriFactory.Create(targetGraph.Uri.ToString().Replace(command.ID, command.Connection.ID))));
                        storage.Update(graphCopy.ToString());
                    }
                }
            }
        }

        /* SparqlCommand.ExecutionInterrupted
         * => clear the command's pending assertions/removals
        */
        internal void OnCommandFailure(Object sender, SparqlExecutableEventArgs args)
        {
        }

        /* SparqlCommand.ExecutionEnded
         * => copy the command's pending assertions/removals to the connection ones
        */
        internal void OnCommandSuccess(Object sender, SparqlExecutableEventArgs args)
        {
            SparqlExecutable context = (SparqlExecutable)sender;
            IUpdateableStorage storage = (IUpdateableStorage)context.UnderlyingStorage;
            // Append command updates to the transaction's
            SparqlParameterizedString command = new SparqlParameterizedString(@"
SELECT DISTINCT ?tempGraph ?affectedGraph
FROM @transactionLog 
WHERE {
    ?tempGraph @requiredBy @commandUri .
    ?tempGraph @affectsGraph ?affectedGraph
    FILTER (!sameTerm(?tempGraph, @RdfNull))
}");
            command.SetParameter("transactionLog", RDFHelper.CreateUriNode(TransactionLog.TRANSACTION_LOG_URI));
            command.SetParameter("requiredBy", DOTNETRDF_TRANS.PropertyRequiredBy);
            command.SetParameter("pendingAssertions", DOTNETRDF_TRANS.ClassPendingAssertionsGraph);
            command.SetParameter("pendingRemovals", DOTNETRDF_TRANS.ClassPendingRemovalsGraph);
            command.SetParameter("affectsGraph", DOTNETRDF_TRANS.PropertyAffectsGraph);
            command.SetParameter("commandUri", RDFHelper.CreateUriNode(context.Uri));
            command.SetParameter("transUri", RDFHelper.CreateUriNode(context.ParentContext.Uri));
            command.SetParameter("cmdID", RDFHelper.CreateLiteralNode(context.ID));
            command.SetParameter("transID", RDFHelper.CreateLiteralNode(context.ParentContext.ID));
            command.SetParameter("RdfNull", RDFHelper.RdfNull);

            // TODO use a SparqlResultHandler to build at once the usingNamedSB
            SparqlResultSet metas = (SparqlResultSet)storage.Query(command.ToString());
            IEnumerable<Uri> affectedGraphs = metas.Results.Select(r => ((IUriNode)r.Value("affectedGraph")).Uri).Distinct();
            IEnumerable<Uri> pendingUpdatesGraphs = metas.Results.Select(r => ((IUriNode)r.Value("tempGraph")).Uri);
            if (pendingUpdatesGraphs.Any())
            {
                StringBuilder usingNamedSB = new StringBuilder();
                foreach (Uri graphUri in pendingUpdatesGraphs)
                {
                    usingNamedSB.AppendLine("USING NAMED <" + graphUri.ToString() + ">");
                }
                command.CommandText = @"
DELETE {
    GRAPH ?transPendingA { ?cmdPendingR_S  ?cmdPendingR_P  ?cmdPendingR_O . }
    GRAPH ?transPendingR { ?cmdPendingA_S  ?cmdPendingA_P  ?cmdPendingA_O . }
}
INSERT {
    GRAPH @transactionLog {
        ?transPendingA a @pendingAssertions .
        ?transPendingA @affectsGraph ?g .
        ?transPendingA @requiredBy @transUri .
        ?transPendingR a @pendingRemovals .
        ?transPendingR @affectsGraph ?g .
        ?transPendingR @requiredBy @transUri .
    }
    GRAPH ?transPendingA { ?cmdPendingA_S  ?cmdPendingA_P  ?cmdPendingA_O . }
    GRAPH ?transPendingR { ?cmdPendingR_S  ?cmdPendingR_P  ?cmdPendingR_O . }
}
USING NAMED @transactionLog 
" + usingNamedSB.ToString() + @"
WHERE {
    GRAPH @transactionLog {
        @commandUri @requiredBy @transUri .
        OPTIONAL 
        {
            ?cmdPendingA @requiredBy @commandUri .
            ?cmdPendingA a @pendingAssertions .
            ?cmdPendingA @affectsGraph ?g .
            GRAPH ?cmdPendingA { ?cmdPendingA_S  ?cmdPendingA_P  ?cmdPendingA_O . }
        }
        OPTIONAL 
        {
            ?cmdPendingR @requiredBy @commandUri .
            ?cmdPendingR a @pendingRemovals .
            ?cmdPendingR @affectsGraph ?g .
            GRAPH ?cmdPendingR { ?cmdPendingR_S  ?cmdPendingR_P  ?cmdPendingR_O . }
        }
        BIND (COALESCE(IRI(REPLACE(STR(?cmdPendingA), @cmdID, @transID)), @RdfNull) as ?transPendingA)
        BIND (COALESCE(IRI(REPLACE(STR(?cmdPendingR), @cmdID, @transID)), @RdfNull) as ?transPendingR)
    }    
};

DROP SILENT GRAPH @RdfNull;
";

                storage.Update(command.ToString());
                // on full command completion, notify the associated connection that some graphs have changed
                if (context is SparqlCommand)
                {
                    // References the graph as updated for the associated connection
                    HashSet<Uri> updatedGraphs;
                    if (!_updatedGraphs.ContainsKey(context.Connection.ID))
                    {
                        updatedGraphs = new HashSet<Uri>(RDFHelper.uriComparer);
                        _updatedGraphs[context.Connection.ID] = updatedGraphs;
                    }
                    else
                    {
                        updatedGraphs = _updatedGraphs[context.Connection.ID];
                    }
                    updatedGraphs.UnionWith(affectedGraphs);
                    // Then make the connection notify its listeners
                    context.Connection.RaiseGraphsUpdated(affectedGraphs);
                }
            }
        }

        /* SparqlCommand.Disposed
         * => Release all the command's temporary resources
         * TODO check whether this is called correctly under normal circumstances
        */
        internal void OnMediatorReleased(Object sender)
        {
            SparqlTemporaryResourceMediator context = (SparqlTemporaryResourceMediator)sender;
            if (context is SparqlExecutable)
            {
                SparqlExecutable executable = (SparqlExecutable)context;
                //executable.ExecutionStarted -= this.SparqlCommand_ExecutionStarted;
                executable.Failed -= this.OnCommandFailure;
                executable.Succeeded -= this.OnCommandSuccess;
            }
            context.Released -= this.OnMediatorReleased;
            IUpdateableStorage storage = (IUpdateableStorage)context.UnderlyingStorage;
            // Clear any commands temporary graphs and reference
            SparqlParameterizedString command = new SparqlParameterizedString(@"
SELECT DISTINCT ?tempGraph 
FROM @transactionLog 
WHERE {
    ?tempGraph @requiredBy @consumerUri .
    # to clear any reference to temp graphs
    # the graph must not be referenced by another consumer
    FILTER (NOT EXISTS {
        ?tempGraph @requiredBy ?concurrentConsumer .
        FILTER (!sameTerm(@consumerUri, ?concurrentConsumer))
    })
}");
            command.SetParameter("transactionLog", RDFHelper.CreateUriNode(TransactionLog.TRANSACTION_LOG_URI));
            command.SetParameter("requiredBy", DOTNETRDF_TRANS.PropertyRequiredBy);
            command.SetParameter("hasScope", DOTNETRDF_TRANS.PropertyHasScope);
            command.SetParameter("consumerUri", RDFHelper.CreateUriNode(context.Uri));
            command.SetParameter("RdfNull", RDFHelper.RdfNull);

            SparqlResultSet metas = (SparqlResultSet)storage.Query(command.ToString());
            StringBuilder dropGraphsSB = new StringBuilder();
            StringBuilder releasableResourceFilter = new StringBuilder();
            releasableResourceFilter.AppendLine("@consumerUri");
            foreach (Uri graphUri in metas.Results.Select(r => ((IUriNode)r.Value("tempGraph")).Uri))
            {
                releasableResourceFilter.AppendLine(", <" + graphUri.ToString() + ">");
                dropGraphsSB.AppendLine("DROP SILENT GRAPH <" + graphUri.ToString() + ">;");
            }
            command.CommandText = @"
DELETE {
    GRAPH @transactionLog {
        ?garbagedResource ?p ?o .
    }
}
USING NAMED @transactionLog 
WHERE {
    GRAPH @transactionLog {
        ?garbagedResource ?p ?o .
        FILTER (?garbagedResource IN(" + releasableResourceFilter.ToString() + @"))
    }
};

" + dropGraphsSB.ToString() + @"
#DROP SILENT GRAPH @RdfNull;
";

            storage.Update(command.ToString());
            if (_commandMetas.ContainsKey(context.ID))
            {
                _commandMetas.Remove(context.ID);
            }
        }

        #endregion

        /// <summary>
        /// A unitary command rewriter helper to help local context maintenance
        /// </summary>
        /// TODO provide differenciation between concurrent and pending assertions and deletions
        private class CommandRewriter
        {

            private static int _pptyIndex = 0;

            // We make this one static so it can be shared in a thread
            // TODO find a way to make it shareable throughout the framework
            private static Dictionary<INode, String> _compiledPropertyVarMap = new Dictionary<INode, String>();

            private static IToken DEFAULT_GRAPH = new FromKeywordToken(0, 0);
            private static IToken NAMED_GRAPH = new FromNamedKeywordToken(0, 0);

            private TransactionSupportStrategy _monitor;
            private readonly bool _simulateIsolation;
            private SparqlCommandUnit _command;

            private IToken _activeGraph;

            private HashSet<Uri> _defaultGraphs = new HashSet<Uri>(RDFHelper.uriComparer);
            private HashSet<Uri> _namedGraphs = new HashSet<Uri>(RDFHelper.uriComparer);

            private Dictionary<String, BindPattern> _graphBindingsExpressions = new Dictionary<String, BindPattern>();
            private Dictionary<INode, HashSet<IToken>> _compiledProperties = new Dictionary<INode, HashSet<IToken>>();

            private Stack<HashSet<IToken>> _defaultGraphVarTokens = new Stack<HashSet<IToken>>();
            private Stack<HashSet<IToken>> _namedGraphVarTokens = new Stack<HashSet<IToken>>();
            private Stack<HashSet<IToken>> _compilationGraphVarTokens = new Stack<HashSet<IToken>>();

            internal CommandRewriter(TransactionSupportStrategy transactionalEventsListener, bool simulateIsolation = true)
            {
                _monitor = transactionalEventsListener;
                _simulateIsolation = simulateIsolation;

                _namedGraphs.Add(TransactionLog.TRANSACTION_LOG_URI);
                _defaultGraphVarTokens.Push(new HashSet<IToken>());
                _namedGraphVarTokens.Push(new HashSet<IToken>());
                _compilationGraphVarTokens.Push(new HashSet<IToken>());

            }

            #region Rewriting methods

            /// <summary>
            /// 
            /// </summary>
            /// TODO handle the all the query/update types
            internal void Rewrite(SparqlCommandUnit command)
            {
                _command = command;
                // TODO minor optimisation : handle this at the end of the rewriting so unused graphs are trimed out (i.e. graphs that are used only in compiled property paths)
                //_namedGraphs.UnionWith(_command.DefaultGraphs.Union(_command.NamedGraphs));

                if (_command.CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
                {
                    SparqlUpdateCommand sparqlUpdate = _command.UpdateCommand;
                    // TODO tranform all updates into a BaseModifyCommand
                    switch (_command.UpdateCommand.CommandType)
                    {
                        case SparqlUpdateCommandType.Delete:
                        case SparqlUpdateCommandType.Insert:
                        case SparqlUpdateCommandType.Load:
                        case SparqlUpdateCommandType.Modify:
                            break;
                        case SparqlUpdateCommandType.Add:
                            SparqlParameterizedString addTemplate = new SparqlParameterizedString("INSERT { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri USING NAMED @sourceUri WHERE { GRAPH @sourceUri { ?s ?p ?o . } }");
                            addTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).DestinationUri));
                            addTemplate.SetParameter("sourceUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).SourceUri));
                            sparqlUpdate = new SparqlUpdateParser().ParseFromString(addTemplate).Commands.First();
                            break;
                        case SparqlUpdateCommandType.Clear:
                            SparqlParameterizedString clearTemplate = new SparqlParameterizedString("DELETE { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri WHERE { GRAPH @targetUri { ?s ?p ?o . } . }");
                            clearTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(((ClearCommand)sparqlUpdate).TargetUri));
                            sparqlUpdate = new SparqlUpdateParser().ParseFromString(clearTemplate).Commands.First();
                            break;
                        case SparqlUpdateCommandType.Drop:
                            SparqlParameterizedString dropTemplate = new SparqlParameterizedString("DELETE { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri WHERE { GRAPH @targetUri { ?s ?p ?o . } . }");
                            dropTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(((DropCommand)sparqlUpdate).TargetUri));
                            sparqlUpdate = new SparqlUpdateParser().ParseFromString(dropTemplate).Commands.First();
                            // TODO add a real drop command on commit
                            break;
                        case SparqlUpdateCommandType.Copy:
                            SparqlParameterizedString copyTemplate = new SparqlParameterizedString("DELETE { GRAPH @targetUri { ?ts ?tp ?to . } } INSERT { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri USING NAMED @sourceUri WHERE { OPTIONAL { GRAPH @targetUri { ?ts ?tp ?to . } } . OPTIONAL { GRAPH @sourceUri { ?s ?p ?o . } } }");
                            copyTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).DestinationUri));
                            copyTemplate.SetParameter("sourceUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).SourceUri));
                            sparqlUpdate = new SparqlUpdateParser().ParseFromString(copyTemplate).Commands.First();
                            break;
                        case SparqlUpdateCommandType.Create:
                            sparqlUpdate = null;
                            break;
                        case SparqlUpdateCommandType.InsertData:
                            sparqlUpdate = new ModifyCommand(null, ((InsertDataCommand)sparqlUpdate).DataPattern, new GraphPattern());
                            break;
                        case SparqlUpdateCommandType.DeleteData:
                            sparqlUpdate = new ModifyCommand(((DeleteDataCommand)sparqlUpdate).DataPattern, null, new GraphPattern());
                            break;
                        case SparqlUpdateCommandType.Move:
                            SparqlParameterizedString moveTemplate = new SparqlParameterizedString("DELETE { GRAPH @targetUri { ?ts ?tp ?to . } GRAPH @sourceUri { ?s ?p ?o . } } INSERT { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri USING NAMED @sourceUri WHERE { OPTIONAL { GRAPH @targetUri { ?ts ?tp ?to . } } . OPTIONAL { GRAPH @sourceUri { ?s ?p ?o . } } }");
                            moveTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).DestinationUri));
                            moveTemplate.SetParameter("sourceUri", RDFHelper.CreateUriNode(((CopyCommand)sparqlUpdate).SourceUri));
                            sparqlUpdate = new SparqlUpdateParser().ParseFromString(moveTemplate).Commands.First();
                            break;
                        default:
                            sparqlUpdate = null;
                            break;
                    }
                    if (sparqlUpdate != null)
                    {
                        if (sparqlUpdate is BaseModificationCommand)
                        {
                            sparqlUpdate = RewriteModificationCommand((BaseModificationCommand)sparqlUpdate);
                        }
                        else if (sparqlUpdate is LoadCommand)
                        {
                            sparqlUpdate = RewriteLoadCommand((LoadCommand)sparqlUpdate);
                        }
                        _command.UpdateCommand = sparqlUpdate;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    SparqlQuery rewrittenQuery = RewriteQuery(_command.Query);
                    _command.Query = rewrittenQuery;
                }
                BuildDependencies();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="query"></param>
            /// <returns></returns>
            /// TODO handle the DESCRIBE case (that one will be tricky :( )
            private SparqlQuery RewriteQuery(SparqlQuery query)
            {
                SparqlQuery rewrittenQuery = query.Copy();
                GraphPattern rewrittenPattern = RewriteReadGraphPattern(rewrittenQuery.RootGraphPattern);
                GraphPattern commandPattern = AddGraphBindings(new HashSet<GraphPattern>() { rewrittenPattern });
                commandPattern.AddGraphPattern(rewrittenPattern);
                rewrittenQuery.RootGraphPattern = new GraphPattern();
                rewrittenQuery.RootGraphPattern.AddGraphPattern(commandPattern);
                if (!query.IsSubQuery)
                {
                    foreach (Uri graphUri in _defaultGraphs)
                    {
                        rewrittenQuery.AddDefaultGraph(graphUri);
                    }
                    foreach (Uri graphUri in _namedGraphs)
                    {
                        rewrittenQuery.AddNamedGraph(graphUri);
                    }
                }
                return rewrittenQuery;
            }

            /* TODO add the written graphs meta triples : 
             *  => @outputGraphVar AffectsGraphUri sourceGraph
             *  => @outputGraphVar a trans:PendingAssertionsGraph | trans:PendingRemovalssGraph
             */
            private BaseModificationCommand RewriteModificationCommand(BaseModificationCommand update)
            {
                // Normalize the update command through translation of the With keyword
                IToken withGraphToken = null;
                if (update.GraphUri != null)
                {
                    withGraphToken = new UriToken("<" + update.GraphUri.ToString() + ">", 0, 0, 0);
                    update.AddUsingNamedUri(update.GraphUri);
                }
                GraphPattern wherePattern = (update.GraphUri != null) ? update.WherePattern.WithinGraph(withGraphToken) : update.WherePattern;
                GraphPattern deletePattern = (update.GraphUri != null && update.DeletePattern != null && !update.DeletePattern.IsGraph) ? update.DeletePattern.WithinGraph(withGraphToken) : update.DeletePattern;
                GraphPattern insertPattern = (update.GraphUri != null && update.InsertPattern != null && !update.InsertPattern.IsGraph) ? update.InsertPattern.WithinGraph(withGraphToken) : update.InsertPattern;

                // Then perform the rewriting
                wherePattern = RewriteReadGraphPattern(wherePattern);
                if (!_command.CommandType.HasFlag(SparqlExecutableType.SparqlInternal))
                {
                    GraphPattern sourceDeletePattern = deletePattern;
                    GraphPattern sourceInsertPattern = insertPattern;
                    deletePattern = new GraphPattern();
                    insertPattern = new GraphPattern();
                    RewriteUpdateTemplate(sourceDeletePattern, deletePattern, insertPattern, false);
                    RewriteUpdateTemplate(sourceInsertPattern, deletePattern, insertPattern, true);
                }
                GraphPattern commandPattern = AddGraphBindings(new HashSet<GraphPattern>() { deletePattern, insertPattern, wherePattern });
                commandPattern.AddGraphPattern(wherePattern);
                GraphPattern rootGraphPattern = new GraphPattern();
                rootGraphPattern.AddGraphPattern(commandPattern);
                ModifyCommand rewrittenCommand = new ModifyCommand(deletePattern, insertPattern, rootGraphPattern);
                foreach (Uri graphUri in _defaultGraphs)
                {
                    rewrittenCommand.AddUsingNamedUri(graphUri);
                }
                foreach (Uri graphUri in _namedGraphs)
                {
                    rewrittenCommand.AddUsingNamedUri(graphUri);
                }
                return rewrittenCommand;
            }

            private LoadCommand RewriteLoadCommand(LoadCommand update)
            {
                SparqlParameterizedString clearBeforeLoadTemplate = new SparqlParameterizedString("DELETE { GRAPH @targetUri { ?s ?p ?o . } } USING NAMED @targetUri WHERE { GRAPH @targetUri { ?s ?p ?o . } . }");
                clearBeforeLoadTemplate.SetParameter("targetUri", RDFHelper.CreateUriNode(update.TargetUri));
                //_command.AddPreProcessingUnit(clearBeforeLoadTemplate);
                SparqlCommandUnit clearBeforeLoad = _command.Context.CreateUnit(new SparqlUpdateParser().ParseFromString(clearBeforeLoadTemplate).Commands.First(), SparqlExecutableType.SparqlUpdate);
                _command.ExecutionStarted += delegate(Object sender, SparqlExecutableEventArgs args)
                {
                    clearBeforeLoad.Execute();
                };
                Uri targetUri = UriFactory.Create(String.Format(PENDING_ASSERTIONS_GRAPH_PREFIX_TEMPLATE, _command.ID) + "#" + update.TargetUri.ToString());
                return new LoadCommand(update.SourceUri, targetUri);
            }

            /// <summary>
            /// Expands a graph pattern for simulated transactional support
            /// </summary>
            /// <example>
            /// 
            /// </example> 
            /// <param name="gp">The graph pattern to expand</param>
            /// <returns>the expanded graph pattern</returns>
            /// <remarks>TODO notify the TransactionLog class for graphs read</remarks>
            private GraphPattern RewriteReadGraphPattern(GraphPattern gp)
            {
                // SERVICE graph patterns are not to be rewritten
                if (gp.IsService) return gp;
                GraphPattern output;// = gp.Clone(true);
                if (gp.IsGraph || gp.IsSubQuery || !gp.HasModifier)
                { // If gp is neither a GraphGraphPattern nor a Bgp we use a shallow copy of gp and add gp rewritten childGraphPattern to it
                    output = new GraphPattern();
                    if (gp.IsGraph)
                    {
                        _activeGraph = gp.GraphSpecifier;
                        // Register the graph to the required graph tokens set
                        switch (gp.GraphSpecifier.TokenType)
                        {
                            case Token.VARIABLE:
                                _namedGraphVarTokens.Peek().Add(gp.GraphSpecifier);
                                break;
                            case Token.URI:
                                _activeGraph = gp.GraphSpecifier;
                                break;
                            default:
                                throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
                        }
                    }
                    foreach (ITriplePattern pattern in gp.TriplePatterns.ToList())
                    {
                        HandleITriplePattern(output, gp, pattern);
                    }
                }
                else
                {
                    output = gp.Clone(true);
                }
                // Rewrites the child graphPatterns
                foreach (GraphPattern cgp in gp.ChildGraphPatterns)
                {
                    GraphPattern rCgp = RewriteReadGraphPattern(cgp);
                    output.AddGraphPattern(rCgp);
                }
                // Since we replace triplePatterns with GraphPatterns, we need to push UnplacedXX in a separate GraphPattern to preserve their original order
                if (gp.UnplacedAssignments.Any() || gp.UnplacedFilters.Any())
                {
                    GraphPattern ensurePatternBreak = new GraphPattern();
                    foreach (BindPattern bp in gp.UnplacedAssignments)
                    {
                        ensurePatternBreak.AddAssignment(bp);
                    }
                    foreach (ISparqlFilter fp in gp.UnplacedFilters)
                    {
                        ensurePatternBreak.AddFilter(fp);
                    }
                    output.AddGraphPattern(ensurePatternBreak);
                }
                return output;
            }

            /// <summary>
            /// Transforms a INSERT/DELETE GraphPattern template
            /// </summary>
            /// <param name="_command"></param>
            /// <param name="template"></param>
            /// <param name="forInsert"></param>
            /// <returns></returns>
            private void RewriteUpdateTemplate(GraphPattern template, GraphPattern deletePattern, GraphPattern insertPattern, bool forInsert)
            {
                if (template == null) return;
                if (template.ActiveGraph == null && template.TriplePatterns.Count > 0) throw new NotSupportedException("Cannot currently write to the default graph.");
                if (template.HasChildGraphPatterns)
                { // Handles the RootGraphPattern
                    foreach (GraphPattern cgp in template.ChildGraphPatterns)
                    {
                        RewriteUpdateTemplate(cgp, deletePattern, insertPattern, forInsert);
                    }
                }
                else
                { // Handles insert and delete triplepatterns
                    _activeGraph = template.ActiveGraph;
                    // TODO check whether we can use variables that map to the default graph
                    if (IsDefaultGraph(_activeGraph)) throw new NotSupportedException("Cannot currently update the default graph.");

                    IToken outputGraphToken, assertionsGraphToken, removalsGraphToken;
                    switch (_activeGraph.TokenType)
                    {
                        case Token.VARIABLE:
                            outputGraphToken = _activeGraph;
                            break;
                        case Token.URI:
                            outputGraphToken = _activeGraph;
                            break;
                        default:
                            throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
                    }
                    _activeGraph = outputGraphToken;
                    String assertionsGraphUri = String.Format(TransactionSupportStrategy.PENDING_ASSERTIONS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Context.ID });
                    assertionsGraphToken = GetGraphAssignement(_activeGraph, "pendingA", assertionsGraphUri);
                    String removalsGraphUri = String.Format(TransactionSupportStrategy.PENDING_REMOVALS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Context.ID });
                    removalsGraphToken = GetGraphAssignement(_activeGraph, "pendingR", removalsGraphUri);
                    // Make pendingAdditions and pendingRemovals disjoint
                    foreach (TriplePattern tp in template.TriplePatterns)
                    {
                        if (forInsert)
                        {
                            insertPattern.AddGraphPattern(tp.AsGraphPattern().WithinGraph(assertionsGraphToken));
                            deletePattern.AddGraphPattern(tp.AsGraphPattern().WithinGraph(removalsGraphToken));
                        }
                        else
                        {
                            insertPattern.AddGraphPattern(tp.AsGraphPattern().WithinGraph(removalsGraphToken));
                            deletePattern.AddGraphPattern(tp.AsGraphPattern().WithinGraph(assertionsGraphToken));
                        }
                    }
                }
            }

            private void HandleITriplePattern(GraphPattern output, GraphPattern source, ITriplePattern pattern)
            {

                switch (pattern.PatternType)
                {
                    // TODO test subquery rewriting
                    case TriplePatternType.SubQuery:
                        // Provide correct graph variable mappings for the subquery;
                        _defaultGraphVarTokens.Push(new HashSet<IToken>());
                        _namedGraphVarTokens.Push(new HashSet<IToken>());
                        _compilationGraphVarTokens.Push(new HashSet<IToken>());

                        SparqlQuery subQuery = RewriteQuery(((SubQueryPattern)pattern).SubQuery);
                        output.AddTriplePattern(new SubQueryPattern(subQuery));

                        _defaultGraphVarTokens.Pop();
                        _namedGraphVarTokens.Pop();
                        _compilationGraphVarTokens.Pop();
                        break;

                    case TriplePatternType.Match:
                        RewriteTriplePattern(output, source, (TriplePattern)pattern);
                        break;

                    case TriplePatternType.Path:
                        HandlePropertyPathPattern(output, source, (PropertyPathPattern)pattern);
                        break;

                    case TriplePatternType.PropertyFunction:
                        HandlePropertyFunctionPattern(output, source, (PropertyFunctionPattern)pattern);
                        break;

                    default:
                        output.AddTriplePattern(pattern);
                        break;
                }
            }

            private void RewriteTriplePattern(GraphPattern output, GraphPattern source, ITriplePattern pattern)
            {
                IToken activeGraph = source.ActiveGraph;
                // Check wether the pattern use the default graph
                if (activeGraph == null)
                {
                    activeGraph = CreateLocalToken();
                    _defaultGraphVarTokens.Peek().Add(activeGraph);
                }

                // Variables for the rewritten pattern
                // Create the replacement pattern that provides full isolation
                GraphPattern bgp = pattern.AsGraphPattern();
                /* If requiring isolation, the replacement pattern is 
                 * ?cmd :namedGraph|:defaultGraph ?g
                 * BIND (... as ?cA) BIND (... as ?cR) BIND (... as ?pA) BIND (... as ?pR)
                 * { GRAPH ?g { ?s ?p ?o . MINUS { FILTER BOUND(?cA) GRAPH ?cA { ?s ?p ?o .} } MINUS { FILTER BOUND(?pR) GRAPH ?pR { ?s ?p ?o .} } } }
                 * UNION 
                 * { FILTER BOUND(?cR) GRAPH ?cR { ?s ?p ?o . MINUS { FILTER BOUND(?pR) GRAPH ?pR { ?s ?p ?o .} } } }
                 * UNION 
                 * { FILTER BOUND(?pA) GRAPH ?pA { ?s ?p ?o } }
                 */
                /* Or 
                 * ?cmd :namedGraph|:defaultGraph ?g
                 * BIND (... as ?cA) BIND (... as ?cR) BIND (... as ?pA) BIND (... as ?pR)
                 * { GRAPH ?g { ?s ?p ?o . FILTER (!BOUND(?cA) || NOT EXISTS {GRAPH ?cA { ?s ?p ?o .} }) FILTER (!BOUND(?pR) || NOT EXISTS { GRAPH ?pR { ?s ?p ?o .} }) } }
                 * UNION 
                 * { FILTER BOUND(?cR) GRAPH ?cR { ?s ?p ?o . FILTER (!BOUND(?pR) || NOT EXISTS { GRAPH ?pR { ?s ?p ?o .} }) } }
                 * UNION 
                 * { FILTER BOUND(?pA) GRAPH ?pA { ?s ?p ?o } }
                 */
                // TODO if performance issues, check which pattern yields best performances between MINUS and FILTER NOT EXISTS

                List<GraphPattern> unions = new List<GraphPattern>();

                IToken concurrentAssertionsGraph = null, concurrentRemovalsGraph = null;
                IToken pendingAssertionsGraph = null, pendingRemovalsGraph = null;
                String additionalGraphUri;
                GraphPattern trimmedGraph = bgp.Clone();

                bool requiresIsolation = _simulateIsolation; // DO NOT include this until concurrency is fully operational: && _monitor.RequiresIsolationFor(_command, activeGraph);
                bool isWriting = _monitor.RequiresUpdatesFor(_command, activeGraph);
                if (requiresIsolation)
                {
                    additionalGraphUri = String.Format(TransactionSupportStrategy.CONCURRENT_ASSERTIONS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Connection.ID });//_command.Connection.Uri.ToString() + ":concurrentAdditionsFor#";
                    concurrentAssertionsGraph = GetGraphAssignement(activeGraph, "concurrentA", additionalGraphUri);
                    additionalGraphUri = String.Format(TransactionSupportStrategy.CONCURRENT_REMOVALS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Connection.ID });//_command.Connection.Uri.ToString() + ":concurrentRemovalsFor#";
                    concurrentRemovalsGraph = GetGraphAssignement(activeGraph, "concurrentR", additionalGraphUri);
                }
                if (isWriting)
                {
                    additionalGraphUri = String.Format(TransactionSupportStrategy.PENDING_ASSERTIONS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Context.ID });//_command.Connection.Uri.ToString() + ":pendingAdditionsFor#";
                    pendingAssertionsGraph = GetGraphAssignement(activeGraph, "pendingA", additionalGraphUri);
                    additionalGraphUri = String.Format(TransactionSupportStrategy.PENDING_REMOVALS_GRAPH_PREFIX_TEMPLATE, new String[] { _command.Context.ID });//_command.Connection.Uri.ToString() + ":pendingRemovalsFor#";
                    pendingRemovalsGraph = GetGraphAssignement(activeGraph, "pendingR", additionalGraphUri);
                }

                if (requiresIsolation) trimmedGraph.AddGraphPattern(GetBoundGraphFilteredPattern(bgp, concurrentAssertionsGraph).WithinMinus());
                if (pendingRemovalsGraph != null) trimmedGraph.AddGraphPattern(GetBoundGraphFilteredPattern(bgp, pendingRemovalsGraph).WithinMinus());
                trimmedGraph = trimmedGraph.WithinGraph(activeGraph);
                unions.Add(trimmedGraph);

                if (requiresIsolation)
                {
                    GraphPattern trimmedConcurrentRemovals = bgp.Clone();
                    if (pendingRemovalsGraph != null) trimmedConcurrentRemovals.AddGraphPattern(GetBoundGraphFilteredPattern(bgp, pendingRemovalsGraph).WithinMinus());
                    trimmedConcurrentRemovals = GetBoundGraphFilteredPattern(trimmedConcurrentRemovals, concurrentRemovalsGraph);
                    unions.Add(trimmedConcurrentRemovals);
                }
                if (pendingAssertionsGraph != null)
                {
                    GraphPattern pendingAdditions = GetBoundGraphFilteredPattern(bgp, pendingAssertionsGraph);
                    unions.Add(pendingAdditions);
                }
                output.AddGraphPattern(unions.ToUnionGraphPattern());
            }

            private void HandlePropertyPathPattern(GraphPattern output, GraphPattern source, PropertyPathPattern pattern)
            {
                if (pattern.Path is Property)
                { // Nothing to do but the handle the simple TriplePattern rewriting
                    RewriteTriplePattern(output, source, pattern);
                }
                else if (pattern.Path is Cardinality)
                {
                    Property path = (Property)((Cardinality)pattern.Path).Path;
                    if (pattern.Path is ZeroOrOne)
                    { // This kind of path do not require preprocessing
                        RewriteTriplePattern(output, source, pattern);
                    }
                    else
                    {
                        IToken activeGraph = source.ActiveGraph;
                        IToken compilationScope;
                        if (IsDefaultGraph(activeGraph))
                        {
                            activeGraph = DEFAULT_GRAPH;
                            compilationScope = activeGraph;
                        }
                        else if (IsDirectGraph(activeGraph))
                        {
                            // The activeGraph is a Uri, we keep the scope...
                            compilationScope = activeGraph;
                        }
                        else
                        {
                            compilationScope = NAMED_GRAPH;
                        }
                        // Remap the property
                        String pptyIndexName;
                        if (_compiledPropertyVarMap.ContainsKey(path.Predicate))
                        {
                            pptyIndexName = _compiledPropertyVarMap[path.Predicate];
                        }
                        else
                        {
                            pptyIndexName = "Ppty" + (_pptyIndex++).ToString();
                            _compiledPropertyVarMap[path.Predicate] = pptyIndexName;
                        }

                        String compilationGraphPrefix = String.Format(TransactionSupportStrategy.COMPILATION_GRAPH_PREFIX_TEMPLATE, new String[] { _command.ID, pptyIndexName });
                        IToken compiledGraph = GetGraphAssignement(activeGraph, "compiled" + pptyIndexName, compilationGraphPrefix);

                        // Register the path compilation
                        if (!_compiledProperties.ContainsKey(path.Predicate))
                        {
                            _compiledProperties[path.Predicate] = new HashSet<IToken>();
                        }
                        _compiledProperties[path.Predicate].Add(compilationScope);
                        _compilationGraphVarTokens.Peek().Add(compiledGraph);

                        output.AddGraphPattern(pattern.AsGraphPattern().WithinGraph(compiledGraph));
                    }
                }
                else if (pattern.Path is SequencePath)
                {
                    SequencePath path = (SequencePath)pattern.Path;
                    VariablePattern pathVariableSplitter = new VariablePattern(CreateLocalToken().Value.Substring(1));
                    HandlePropertyPathPattern(output, source, new PropertyPathPattern(pattern.Subject, path.LhsPath, pathVariableSplitter));
                    HandlePropertyPathPattern(output, source, new PropertyPathPattern(pathVariableSplitter, path.RhsPath, pattern.Object));
                }
                else if (pattern.Path is InversePath)
                {
                    InversePath path = (InversePath)pattern.Path;
                    HandlePropertyPathPattern(output, source, new PropertyPathPattern(pattern.Object, path.Path, pattern.Subject));
                }
                else if (pattern.Path is NegatedSet)
                {
                    RewriteTriplePattern(output, source, pattern);
                }
                else if (pattern.Path is AlternativePath)
                {
                    AlternativePath path = (AlternativePath)pattern.Path;
                    List<GraphPattern> union = new List<GraphPattern>() { new GraphPattern(), new GraphPattern() };
                    HandlePropertyPathPattern(union[0], source, new PropertyPathPattern(pattern.Subject, path.LhsPath, pattern.Object));
                    HandlePropertyPathPattern(union[1], source, new PropertyPathPattern(pattern.Subject, path.RhsPath, pattern.Object));
                    output.AddGraphPattern(union.ToUnionGraphPattern());
                }
            }

            // We do nothing here, we will add a DistributedEvaluationRewriteStrategy class later
            private void HandlePropertyFunctionPattern(GraphPattern output, GraphPattern source, PropertyFunctionPattern pattern)
            {
                RewriteTriplePattern(output, source, pattern);
            }

            #endregion

            #region Utilities

            private IToken CreateLocalToken(String name = "")
            {
                return new VariableToken("?" + RDFHelper.NewVarName(name), 0, 0, 0);
            }

            private bool IsCompilationGraph(IToken graphToken)
            {
                return graphToken == null || (graphToken.TokenType == Token.VARIABLE && _compilationGraphVarTokens.Peek().Contains(graphToken));
            }

            private bool IsDefaultGraph(IToken graphToken)
            {
                return graphToken == null || (graphToken.TokenType == Token.VARIABLE && _defaultGraphVarTokens.Peek().Select(t => t.Value).Contains(graphToken.Value));
            }

            private bool IsDirectGraph(IToken graphToken)
            {
                return graphToken != null && graphToken.TokenType == Token.URI;
            }

            /// <summary>
            /// Wraps a BGP into a bound graph filter pattern
            /// </summary>
            /// <param name="bgp"></param>
            /// <param name="optionalGraph"></param>
            /// <returns></returns>
            private GraphPattern GetBoundGraphFilteredPattern(GraphPattern bgp, IToken optionalGraph)
            {
                GraphPattern output = new GraphPattern();
                output.AddTriplePattern(new FilterPattern(new BoundFilter(new VariableTerm(optionalGraph.Value.Substring(1)))));
                output.AddGraphPattern(bgp.WithinGraph(optionalGraph));
                return output;
            }


            private IToken GetGraphAssignement(IToken sourceGraph, String resultVarPrefix, String valuePrefix)
            {
                ISparqlExpression sourceTerm;
                String varName;
                switch (sourceGraph.TokenType)
                {
                    case Token.VARIABLE:
                        varName = sourceGraph.Value.Substring(1);
                        sourceTerm = new VariableTerm(varName);
                        break;
                    case Token.URI:
                        varName = CreateLocalToken("Iri").Value.Substring(1);
                        sourceTerm = new ConstantTerm(RDFHelper.CreateUriNode(UriFactory.Create(sourceGraph.Value)));
                        break;
                    case Token.FROM: // MUST be used only for property compiled graphs
                        varName = new DefaultKeywordToken(0, 0).Value;
                        sourceTerm = new ConstantTerm(varName.ToLiteral(RDFHelper.nodeFactory));
                        break;
                    default:
                        throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
                }
                String outputGraphVar = resultVarPrefix + "_" + varName;
                IToken outputGraphToken = new VariableToken("?" + resultVarPrefix + "_" + varName, 0, 0, 0);
                if (!_graphBindingsExpressions.ContainsKey(outputGraphVar))
                {
                    BindPattern graphBindingPattern = new BindPattern(outputGraphVar, new IriFunction(new ConcatFunction(new List<ISparqlExpression> { 
                        new ConstantTerm(valuePrefix.ToLiteral(RDFHelper.nodeFactory)),
                        new StrFunction(sourceTerm)
                    })));
                    _graphBindingsExpressions[outputGraphVar] = graphBindingPattern;
                }
                return outputGraphToken;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sourceVar"></param>
            /// <param name="sourceGraph"></param>
            /// <param name="bindingExpressions"></param>
            /// <returns></returns>
            /// TODO find a way to batch this;
            private IEnumerable<Uri> ComputeGraphBindings(String sourceVar, IEnumerable<Uri> sourceGraph, IEnumerable<BindPattern> bindingExpressions)
            {
                SparqlQuery localEvaluationQuery = new SparqlQueryParser().ParseFromString("SELECT DISTINCT * WHERE {}");
                if (sourceVar != null)
                {
                    List<String> varList = new List<String>() { sourceVar };
                    localEvaluationQuery.Bindings = new BindingsPattern(varList);
                    foreach (Uri graphUri in sourceGraph)
                    {
                        ((List<BindingTuple>)localEvaluationQuery.Bindings.Tuples).Add(new BindingTuple(varList, new List<PatternItem>() { new NodeMatchPattern(RDFHelper.CreateUriNode(graphUri)) }));
                    }
                }
                foreach (BindPattern bindingExpression in bindingExpressions)
                {
                    localEvaluationQuery.RootGraphPattern.AddAssignment(bindingExpression);
                }

                HashSet<Uri> boundGraphs = new HashSet<Uri>(RDFHelper.uriComparer);
                SparqlResultSet expandedGraphs = (SparqlResultSet)_command.UnderlyingStorage.Query(localEvaluationQuery.ToString());
                foreach (SparqlResult result in expandedGraphs)
                {
                    boundGraphs.UnionWith(result.Variables.Where(v => v != sourceVar && result.HasBoundValue(v)).Select(v => ((IUriNode)result.Value(v)).Uri));
                }
                return boundGraphs;
            }

            #endregion

            #region Command compilation

            // TODO handle the scoping of the additional graphs 
            //      => transactions updates graphs scoped to the connection
            //      => temporary graphs scoped to the command
            private GraphPattern AddGraphBindings(IEnumerable<GraphPattern> rewrittenPatterns)
            {
                Uri commandUri = _command.Uri;
                GraphPattern root = new GraphPattern().WithinGraph(TransactionLog.TRANSACTION_LOG_URI);
                // References used graphs
                IEnumerable<String> graphVariables = rewrittenPatterns.Where(gp => gp != null).SelectMany(gp => gp.GraphSpecifiers).Where(t => t.TokenType == Token.VARIABLE).Select(t => t.Value.Substring(1));//.Union(_graphBindingsExpressions.Values.SelectMany(bp => bp.Variables));
                IEnumerable<String> explicitGraphs = rewrittenPatterns.Where(gp => gp != null).SelectMany(gp => gp.GraphSpecifiers).Where(t => t.TokenType == Token.URI).Select(t => t.Value);

                IEnumerable<String> usedNamedGraphVariables = _namedGraphVarTokens.Peek().Select(t => t.Value.Substring(1)).Intersect(graphVariables);
                IEnumerable<String> usedDefaultGraphVariables = _defaultGraphVarTokens.Peek().Select(t => t.Value.Substring(1)).Intersect(graphVariables);
                IEnumerable<String> usedCompilationGraphVariables = _compilationGraphVarTokens.Peek().Select(t => t.Value.Substring(1)).Intersect(graphVariables);

                // Add graphs to the internal command dataset
                _namedGraphs.UnionWith(explicitGraphs.Select(s => UriFactory.Create(s)));
                if (usedNamedGraphVariables.Any())
                {
                    _namedGraphs.UnionWith(_command.NamedGraphs);
                    foreach (String varName in usedNamedGraphVariables)
                    {
                        root.AddTriplePattern(new TriplePattern(new NodeMatchPattern(RDFHelper.CreateUriNode(commandUri)), new NodeMatchPattern(DOTNETRDF_TRANS.PropertyHasNamedGraph), new VariablePattern(varName)));
                    }
                }
                if (usedDefaultGraphVariables.Any())
                {
                    _namedGraphs.UnionWith(_command.DefaultGraphs);
                    foreach (String varName in usedDefaultGraphVariables)
                    {
                        root.AddTriplePattern(new TriplePattern(new NodeMatchPattern(RDFHelper.CreateUriNode(commandUri)), new NodeMatchPattern(DOTNETRDF_TRANS.PropertyHasDefaultGraph), new VariablePattern(varName)));
                    }
                }

                // Add any additional graph binding expression
                foreach (String additionalGraph in _graphBindingsExpressions.Keys.Intersect(graphVariables))
                {
                    BindPattern bp = _graphBindingsExpressions[additionalGraph];
                    String sourceGraphVar = bp.Variables.Where(v => v != bp.VariableName).FirstOrDefault();
                    IToken sourceGraph = new VariableToken("?" + sourceGraphVar, 0, 0, 0);
                    IEnumerable<Uri> sourceGraphUris = null;
                    if (sourceGraphVar != null)
                    {
                        if (IsDefaultGraph(sourceGraph))
                        {
                            sourceGraphUris = _command.DefaultGraphs;
                        }
                        else
                        {
                            // Quite specific case : the graph that source the compilation is not directly referenced in the command
                            if (!usedNamedGraphVariables.Contains(sourceGraphVar))
                            {
                                root.AddTriplePattern(new TriplePattern(new NodeMatchPattern(RDFHelper.CreateUriNode(commandUri)), new NodeMatchPattern(DOTNETRDF_TRANS.PropertyHasNamedGraph), new VariablePattern(sourceGraphVar)));
                            }
                            sourceGraphUris = _command.NamedGraphs;
                        }
                    }
                    IEnumerable<Uri> additionalGraphs = ComputeGraphBindings(sourceGraphVar, sourceGraphUris, new List<BindPattern>() { bp });
                    // TODO refactor this to handle it directly in the cases
                    if (additionalGraph.Contains("pending"))
                    {
                        IEnumerable<Triple> pendingUpdateGraphs = additionalGraphs
                            .SelectMany(
                            graphUri => new HashSet<Triple>() {
                                new Triple(RDFHelper.CreateUriNode(graphUri), DOTNETRDF_TRANS.PropertyRequiredBy, RDFHelper.CreateUriNode(_command.Context.Uri)),
                                new Triple(RDFHelper.CreateUriNode(graphUri), DOTNETRDF_TRANS.PropertyAffectsGraph, RDFHelper.CreateUriNode(UriFactory.Create(graphUri.Fragment.Substring(1)))), // TODO replace this with (graphUri, updates, originalGraphUri)
                                new Triple(RDFHelper.CreateUriNode(graphUri), RDF.PropertyType, additionalGraph.Contains("pendingA") ? DOTNETRDF_TRANS.ClassPendingAssertionsGraph: DOTNETRDF_TRANS.ClassPendingRemovalsGraph)
                            });
                        _monitor.AddMetas(_command.Context.ID, pendingUpdateGraphs);
                    }
                    else if (additionalGraph.Contains("concurrent"))
                    {
                        IEnumerable<Triple> concurrentUpdateGraphs = additionalGraphs
                            .SelectMany(
                            graphUri => new HashSet<Triple>() {
                                new Triple(RDFHelper.CreateUriNode(graphUri), DOTNETRDF_TRANS.PropertyRequiredBy, RDFHelper.CreateUriNode(_command.Connection.Uri)),
                                new Triple(RDFHelper.CreateUriNode(graphUri), DOTNETRDF_TRANS.PropertyAffectsGraph, RDFHelper.CreateUriNode(UriFactory.Create(graphUri.Fragment.Substring(1)))), // TODO replace this with (graphUri, updates, originalGraphUri)
                                new Triple(RDFHelper.CreateUriNode(graphUri), RDF.PropertyType, additionalGraph.Contains("concurrentA") ? DOTNETRDF_TRANS.ClassPendingAssertionsGraph: DOTNETRDF_TRANS.ClassPendingRemovalsGraph)
                            });
                        _monitor.AddMetas(_command.Connection.ID, concurrentUpdateGraphs);
                    }
                    else
                    {
                        IEnumerable<Triple> temporaryGraphs = additionalGraphs
                            .SelectMany(
                            graphUri => new HashSet<Triple>() {
                                new Triple(RDFHelper.CreateUriNode(graphUri), DOTNETRDF_TRANS.PropertyRequiredBy, RDFHelper.CreateUriNode(_command.Uri)),
                                new Triple(RDFHelper.CreateUriNode(graphUri), RDF.PropertyType, DOTNETRDF_TRANS.ClassTemporaryGraph)
                            });
                        _monitor.AddMetas(_command.ID, temporaryGraphs);
                    }
                    _namedGraphs.UnionWith(additionalGraphs);
                    root.AddAssignment(bp);
                }
                return root;
            }

            /// <summary>
            /// Creates preprocessing units for the command.
            /// Units will : 
            ///     => update the transactionLog by attaching named/default graphs used by the rewritten sparql command
            ///     => create the required property compilation graphs (if any)
            /// </summary>
            private void BuildDependencies()
            {

                // Handles the command's source dataset
                List<Triple> commandMetas =
                    _command.DefaultGraphs.Select(graphUri => new Triple(RDFHelper.CreateUriNode(_command.Uri), DOTNETRDF_TRANS.PropertyHasDefaultGraph, RDFHelper.CreateUriNode(graphUri)))
                    .Union(
                        _command.NamedGraphs.Select(graphUri => new Triple(RDFHelper.CreateUriNode(_command.Uri), DOTNETRDF_TRANS.PropertyHasNamedGraph, RDFHelper.CreateUriNode(graphUri)))
                    ).ToList();
                _monitor.AddMetas(_command.ID, commandMetas);

                if (_command.CommandType.HasFlag(SparqlExecutableType.SparqlInternal)) return;

                // Handles the properties' compilation
                if (_compiledProperties.Count > 0)
                {
                    int patternIndex = 0;
                    HashSet<BindPattern> assignments = new HashSet<BindPattern>();
                    GraphPattern insertPattern = new GraphPattern();
                    List<GraphPattern> readPatterns = new List<GraphPattern>();
                    foreach (INode predicate in _compiledProperties.Keys)
                    {
                        String pptyIndexName = _compiledPropertyVarMap[predicate];
                        HashSet<IToken> sourceGraphs = _compiledProperties[predicate];
                        bool requiresDefaultGraphCompilation = sourceGraphs.Contains(DEFAULT_GRAPH);
                        bool requiresNamedGraphCompilation = sourceGraphs.Contains(NAMED_GRAPH);
                        if (!requiresNamedGraphCompilation)
                        {
                            foreach (IToken graph in sourceGraphs.Where(t => t.TokenType == Token.URI))
                            {
                                Uri graphUri = UriFactory.Create(graph.Value); // not sure we need this var
                                GraphPattern propertyPattern = new TriplePattern(
                                        new VariablePattern("s" + patternIndex.ToString()),
                                        new NodeMatchPattern(predicate),
                                        new VariablePattern("p" + patternIndex.ToString())
                                    ).AsGraphPattern();
                                patternIndex++;
                                String compilationGraphPrefix = String.Format(TransactionSupportStrategy.COMPILATION_GRAPH_PREFIX_TEMPLATE, new String[] { _command.ID, pptyIndexName });
                                IToken compiledGraph = GetGraphAssignement(graph, "compiled" + pptyIndexName, compilationGraphPrefix);
                                insertPattern.AddGraphPattern(propertyPattern.WithinGraph(compiledGraph));
                                assignments.Add(_graphBindingsExpressions[compiledGraph.Value.Substring(1)]);
                                readPatterns.Add(propertyPattern.WithinGraph(graph).WithinOptional());
                            }
                        }
                        else
                        {
                            GraphPattern propertyPattern = new TriplePattern(
                                    new VariablePattern("s" + patternIndex.ToString()),
                                    new NodeMatchPattern(predicate),
                                    new VariablePattern("p" + patternIndex.ToString())
                                ).AsGraphPattern();
                            patternIndex++;
                            IToken namedGraphToken = CreateLocalToken("namedGraph");
                            String compilationGraphPrefix = String.Format(TransactionSupportStrategy.COMPILATION_GRAPH_PREFIX_TEMPLATE, new String[] { _command.ID, pptyIndexName });
                            IToken compiledGraph = GetGraphAssignement(namedGraphToken, "compiled" + pptyIndexName, compilationGraphPrefix);
                            insertPattern.AddGraphPattern(propertyPattern.WithinGraph(compiledGraph));
                            assignments.Add(_graphBindingsExpressions[compiledGraph.Value.Substring(1)]);
                            readPatterns.Add(propertyPattern.WithinGraph(namedGraphToken).WithinOptional());
                        }
                        if (requiresDefaultGraphCompilation)
                        {
                            GraphPattern propertyPattern = new TriplePattern(
                                    new VariablePattern("s" + patternIndex.ToString()),
                                    new NodeMatchPattern(predicate),
                                    new VariablePattern("p" + patternIndex.ToString())
                                ).AsGraphPattern();
                            patternIndex++;
                            String compilationGraphPrefix = String.Format(TransactionSupportStrategy.COMPILATION_GRAPH_PREFIX_TEMPLATE, new String[] { _command.ID, pptyIndexName });
                            IToken compiledGraph = GetGraphAssignement(DEFAULT_GRAPH, "compiled" + pptyIndexName, compilationGraphPrefix);
                            insertPattern.AddGraphPattern(propertyPattern.WithinGraph(compiledGraph));
                            // TODO check why the named graph compilations are not dropped after command execution
                            assignments.Add(_graphBindingsExpressions[compiledGraph.Value.Substring(1)]);
                            readPatterns.Add(propertyPattern.WithinOptional());
                        }
                    }
                    GraphPattern rootPattern = new GraphPattern();
                    foreach (BindPattern assignment in assignments)
                    {
                        rootPattern.AddAssignment(assignment);
                    }
                    rootPattern.AddGraphPattern(readPatterns.ToUnionGraphPattern());
                    ModifyCommand precomputingCommand = new ModifyCommand(null, insertPattern, rootPattern);
                    foreach (Uri graphUri in _command.DefaultGraphs)
                    {
                        precomputingCommand.AddUsingUri(graphUri);
                    }
                    foreach (Uri graphUri in _command.NamedGraphs)
                    {
                        precomputingCommand.AddUsingNamedUri(graphUri);
                    }
                    SparqlCommandUnit compilationUnit = _command.Context.CreateUnit(precomputingCommand, SparqlExecutableType.SparqlInternal);
                    _command.ExecutionStarted += delegate(Object sender, SparqlExecutableEventArgs args)
                    {
                        compilationUnit.Execute();
                    };
                }
            }

            #endregion
        }

    }

}
