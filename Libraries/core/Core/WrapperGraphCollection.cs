using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract decorator for Graph Collections to make it easier to add new functionality to existing implementations
    /// </summary>
    public abstract class WrapperGraphCollection
        : BaseGraphCollection
    {
        protected BaseGraphCollection _graphs;

        public WrapperGraphCollection(BaseGraphCollection graphCollection)
        {
            if (graphCollection == null) throw new ArgumentNullException("graphCollection");
            this._graphs = graphCollection;
            this._graphs.GraphAdded += this.HandleGraphAdded;
            this._graphs.GraphRemoved += this.HandleGraphRemoved;
        }

        private void HandleGraphAdded(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphAdded(args.Graph);
        }

        private void HandleGraphRemoved(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphRemoved(args.Graph);
        }

        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            return this._graphs.Add(g, mergeIfExists);
        }

        public override bool Contains(Uri graphUri)
        {
            return this._graphs.Contains(graphUri);
        }

        public override int Count
        {
            get
            {
                return this._graphs.Count;
            }
        }

        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        public override IEnumerator<IGraph> GetEnumerator()
        {
            return this._graphs.GetEnumerator();
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._graphs.GraphUris;
            }
        }

        protected internal override bool Remove(Uri graphUri)
        {
            return this._graphs.Remove(graphUri);
        }

        public override IGraph this[Uri graphUri]
        {
            get
            {
                return this._graphs[graphUri];
            }
        }
    }
}
