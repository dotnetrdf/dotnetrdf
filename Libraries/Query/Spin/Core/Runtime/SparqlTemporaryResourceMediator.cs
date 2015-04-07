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
    internal delegate void ReleasedEventHandler(Object sender);

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
    /// TODO allow to create a direct instance for potential garbage collection in case of crash recovery
    ///     => make the class unabstract
    /// TODO refactor this to provide better code isolation
    public abstract class SparqlTemporaryResourceMediator
        : IDisposable
    {
        internal event ReleasedEventHandler Released;

        public const String BASE_URI = "tmp:dotnetrdf.org";
        public const String NS_URI = BASE_URI + ":";

        private readonly String _id = Guid.NewGuid().ToString().Replace("-", "");

        // TODO use a graph instead to allow for simpler additions and management ?
        //private HashSet<String> _tempGraphs = new HashSet<String>();

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
                return UriFactory.Create(SparqlTemporaryResourceMediator.NS_URI + _id);
            }
        }

        internal abstract IQueryableStorage UnderlyingStorage { get; }

        internal abstract SparqlTemporaryResourceMediator ParentContext { get; }

        internal void RaiseReleased()
        {
            ReleasedEventHandler handler = Released;
            if (handler!=null) Released.Invoke(this);
        }

        public virtual void Dispose()
        {
            RaiseReleased();
        }
    }
}
