using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    // TODO provide for better/safer/simpler event handling
    internal delegate void DisposedEventHandler(Object sender);

    // TODO refactor this into a marker interface with events only and maybe sparql command remplates to do the work.
    // Each rewriter will be responsible on handling these events (even if it seems less performant?) :
    //  => ExecutionStarted
    //  => ExecutionInterrupted
    //  => ExecutionEnded
    //  => Disposed

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Under normal circumstances, this class should automatically release its temporary graphs at Dispose
    /// It should also be responsible to update the transaction log so temproray graphs can be garbage-collected manually on process failures
    /// => this implies updating the transaction log with meta informations like lastUpdates/lastRead...
    /// </remarks>
    /// TODO find a better name for this
    public abstract class BaseTemporaryGraphConsumer
        : IDisposable
    {
        internal event DisposedEventHandler Disposable;

        public const String BASE_URI = "tmp:dotnetrdf.org";
        public const String NS_URI = BASE_URI + ":";

        private readonly String _id = Guid.NewGuid().ToString().Replace("-", "");

        // TODO use a graph instead to allow for simpler additions and management ?
        private HashSet<String> _tempGraphs = new HashSet<String>();

        internal String ID
        {
            get
            {
                return _id;
            }
        }

        // TODO check whether we let the derived class define their Uris ?
        public Uri Uri
        {
            get
            {
                return UriFactory.Create(BaseTemporaryGraphConsumer.NS_URI + _id);
            }
        }

        internal abstract IQueryableStorage UnderlyingStorage { get; }

        internal abstract BaseTemporaryGraphConsumer ParentContext { get; }

        public IEnumerable<Uri> TemporaryGraphs
        {
            get
            {
                if (!(UnderlyingStorage is IUpdateableStorage)) return new HashSet<Uri>();
                return _tempGraphs.Select(s => UriFactory.Create(s));
            }
        }

//        // TODO check whether the IEnumerable<Triple> is recommendable or if we may have to define a TemporaryGraphClass instead ?
//        // TemporaryGraph might then derive from IGraph so we can use the API instead of SPARQL ?
//        private void AddTemporaryGraph(String graphUri, IEnumerable<Triple> metas = null)
//        {
//            if (String.IsNullOrEmpty(graphUri)) throw new ArgumentException("Temporary graph Uri cannot be null");
//            // TODO check whether we raise an exception or we ignore
//            if (!graphUri.StartsWith(NS_URI)) return;// throw new ArgumentException("Temporary graph Uri must begin with " + NS_URI);
//            if (!(UnderlyingStorage is IUpdateableStorage)) return;
//            IUpdateableStorage storage = (IUpdateableStorage)UnderlyingStorage;
//            _tempGraphs.Add(graphUri);

//            StringBuilder metasSB = new StringBuilder();
//            if (metas != null)
//            {
//                SparqlFormatter tripleFormatter = new SparqlFormatter();
//                foreach (Triple t in metas.Where(t => t.Subject is IUriNode && ((IUriNode)t.Subject).Uri.ToString() == graphUri))
//                {
//                    metasSB.AppendLine(tripleFormatter.Format(t));
//                }
//            }
//            BaseTemporaryGraphConsumer context = this;
//            while (context.ParentContext != null)
//            {
//                SparqlParameterizedString contextTriple = new SparqlParameterizedString("@context @hasScope @parentContext .");
//                contextTriple.SetParameter("context", RDFHelper.CreateUriNode(context.Uri));
//                contextTriple.SetParameter("hasScope", DOTNETRDF_TRANS.PropertyHasScope);
//                contextTriple.SetParameter("parentContext", RDFHelper.CreateUriNode(context.ParentContext.Uri));
//                metasSB.AppendLine(contextTriple.ToString());
//                context = context.ParentContext;
//            }

//            SparqlParameterizedString registerCommand = new SparqlParameterizedString(@"
//INSERT {
//    GRAPH @transactionLog {
//        ?tempGraphUri @requiredBy @consumerUri .
//        " + metasSB.ToString() + @"
//    }
//} WHERE {
//    BIND (IRI(STR(@tempGraphUri)) as ?tempGraphUri)
//}
//            ");
//            registerCommand.SetParameter("transactionLog", RDFHelper.CreateUriNode(TransactionLog.TRANSACTION_LOG_URI));
//            registerCommand.SetParameter("tempGraphUri", RDFHelper.CreateUriNode(UriFactory.Create(graphUri)));
//            registerCommand.SetParameter("requiredBy", DOTNETRDF_TRANS.PropertyRequiredBy);
//            registerCommand.SetParameter("consumerUri", RDFHelper.CreateUriNode(Uri));

//            storage.Update(registerCommand.ToString());
//        }

//        // TODO define how to pass additional graph properties
//        internal virtual void AddTemporaryGraph(Uri graphUri, IEnumerable<Triple> metas = null)
//        {
//            AddTemporaryGraph(graphUri.ToString(), metas);
//        }

//        internal virtual void AddTemporaryGraphs(IEnumerable<Uri> graphUris)
//        {
//            foreach (Uri graphUri in graphUris) AddTemporaryGraph(graphUri.ToString());
//        }

//        internal void Ping()
//        {
//        }

        internal virtual void CleanUp() {
            PerformCleaning(UnderlyingStorage);
        }

        internal void CleanUpAsync()
        {
            IQueryableStorage storage = UnderlyingStorage;
            new Task(() => { PerformCleaning(storage); }).Start();
        }

        // TODO tranform this as EventHandler
        internal virtual void PerformCleaning(IQueryableStorage storageReference)
        {
#if DEBUG
            return;
#endif 
            // TODO push that in a thread and perform global garbage collection ?
            if (!(storageReference is IUpdateableStorage)) return;
            IUpdateableStorage storage = (IUpdateableStorage)storageReference;
            StringBuilder usingNamedSB = new StringBuilder();
            foreach (String graphUri in _tempGraphs)
            {
                usingNamedSB.AppendLine("USING NAMED <" + graphUri + ">");
            }
            SparqlParameterizedString cleanCommand = new SparqlParameterizedString(@"
DELETE {
    GRAPH @transactionLog {
        @consumerUri ?p ?o .
        ?tempGraph @requiredBy @consumerUri .
        ?tempGraph ?tmpGraphP ?tmpGraphO .
    }
    GRAPH ?tempGraph {
        ?tmpGraphTS ?tmpGraphTP ?tmpGraphTO .
    }
}
USING NAMED @transactionLog 
" + usingNamedSB.ToString() + @"
WHERE {
    GRAPH @transactionLog {
        # To clear all consumer properties
        @consumerUri ?p ?o .
        # To remove graph dependency
        OPTIONAL {
            ?tempGraph @requiredBy @consumerUri .
            OPTIONAL {
                # the graph must not be referenced by another consumer
                FILTER (NOT EXISTS {
                    ?tempGraph @requiredBy ?concurrentConsumer .
                    FILTER (!sameTerm(@consumerUri, ?concurrentConsumer))
                })
                # to clear the temp graph properties
                ?tempGraph ?tmpGraphP ?tmpGraphO 
                # to clear the temp graph contents
                GRAPH ?tempGraph {
                    ?tmpGraphTS ?tmpGraphTP ?tmpGraphTO .
                } 
            }
        }
    }
}
");
            cleanCommand.SetParameter("transactionLog", RDFHelper.CreateUriNode(TransactionLog.TRANSACTION_LOG_URI));
            cleanCommand.SetParameter("requiredBy", DOTNETRDF_TRANS.PropertyRequiredBy);
            cleanCommand.SetParameter("hasScope", DOTNETRDF_TRANS.PropertyHasScope);
            cleanCommand.SetParameter("consumerUri", RDFHelper.CreateUriNode(Uri));

            storage.Update(cleanCommand.ToString());
        }

        internal void RaiseDisposable()
        {
            DisposedEventHandler handler = Disposable;
            if (handler!=null) Disposable.Invoke(this);
        }

        public virtual void Dispose()
        {
            RaiseDisposable();
        }
    }
}
