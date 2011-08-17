using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.FullText.Indexing
{
    public abstract class BaseFullTextIndexer
        : IFullTextIndexer
    {
        ~BaseFullTextIndexer()
        {
            this.Dispose(false);
        }

        public abstract IndexingMode IndexingMode
        {
            get;
        }

        public abstract void Index(Triple t);

        public void Index(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                this.Index(t);
            }
        }

        public void Index(ISparqlDataset dataset)
        {
            foreach (Uri u in dataset.GraphUris)
            {
                IGraph g = dataset[u];
                this.Index(g);
            }
        }

        public abstract void Unindex(Triple t);

        public void Unindex(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                this.Unindex(t);
            }
        }

        public void Unindex(ISparqlDataset dataset)
        {
            foreach (Uri u in dataset.GraphUris)
            {
                IGraph g = dataset[u];
                this.Unindex(g);
            }
        }

        public virtual void Flush() { }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this.DisposeInternal();
        }

        protected virtual void DisposeInternal() { }
    }
}
