using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;
using VDS.Alexandria.Indexing;

namespace VDS.Alexandria
{
    public abstract class BaseAlexandriaManager : IGenericIOManager
    {
        protected internal abstract IIndexManager IndexManager
        {
            get;
        }

        public abstract void LoadGraph(IGraph g, Uri graphUri);

        public abstract void LoadGraph(IGraph g, string graphUri);

        public abstract void SaveGraph(IGraph g);

        public abstract void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        public abstract void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        public abstract void DeleteGraph(String graphUri);

        public abstract void DeleteGraph(Uri graphUri);

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

        public abstract void Dispose();
    }
}
