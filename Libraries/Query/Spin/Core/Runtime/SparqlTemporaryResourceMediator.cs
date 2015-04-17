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
    #region Events handlers

    internal delegate void ReleasedEventHandler(Object sender);

    #endregion

    /// <summary>
    /// A base class for resources that require or emit temporary graphs in the underlying storage
    /// </summary>
    /// <remarks>
    /// Under normal circumstances, this class should notify listeners to release its temporary graphs on Dispose if not sooner
    /// </remarks>
    /// TODO allow to create a direct instance for potential garbage collection in case of crash recovery
    public abstract class SparqlTemporaryResourceMediator
        : IDisposable
    {
        internal event ReleasedEventHandler Released;

        /// <summary>
        /// The base namespace prefix for temporary resources and vocabulary
        /// </summary>
        public const String BASE_URI = "tmp:dotnetrdf.org";

        /// <summary>
        /// The Uri prefix for temporary resources
        /// </summary>
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
