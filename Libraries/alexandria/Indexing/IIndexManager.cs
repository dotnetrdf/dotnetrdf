using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace Alexandria.Indexing
{
    /// <summary>
    /// Interface for Index Manager
    /// </summary>
    public interface IIndexManager : IDisposable
    {
        IEnumerable<Triple> GetTriples(String indexName);

        void AddToIndex(Triple t);

        void AddToIndex(IEnumerable<Triple> ts);

        void RemoveFromIndex(Triple t);

        void RemoveFromIndex(IEnumerable<Triple> ts);
    }
}
