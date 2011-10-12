using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using VDS.Alexandria.Indexing;

namespace VDS.Alexandria
{
    public abstract class BaseAlexandriaManager 
        : IQueryableGenericIOManager
    {
        protected internal abstract IIndexManager IndexManager
        {
            get;
        }

        public abstract void LoadGraph(IGraph g, Uri graphUri);

        public abstract void LoadGraph(IGraph g, String graphUri);

        public abstract void LoadGraph(IRdfHandler handler, Uri graphUri);

        public abstract void LoadGraph(IRdfHandler handler, String graphUri);

        public abstract void SaveGraph(IGraph g);

        public virtual IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
        }

        public abstract void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        public abstract void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        public abstract void DeleteGraph(String graphUri);

        public abstract void DeleteGraph(Uri graphUri);

        public abstract bool DeleteSupported
        {
            get;
        }

        public abstract IEnumerable<Uri> ListGraphs();

        public abstract bool ListGraphsSupported
        {
            get;
        }

        public abstract Object Query(String sparqlQuery);

        public abstract void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery);

        public abstract bool UpdateSupported
        {
            get;
        }

        public abstract bool IsReady
        {
            get;
        }

        public abstract bool IsReadOnly
        {
            get;
        }

        public abstract void Flush();

        public abstract void Dispose();
    }
}
