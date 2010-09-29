using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;

namespace Alexandria.Indexing
{
    public class FileIndexManager : IIndexManager
    {
        private FileDocumentManager _manager;

        public FileIndexManager(FileDocumentManager manager)
        {
            this._manager = manager;
        }

        public IEnumerable<VDS.RDF.Triple> GetTriples(string indexName)
        {
            throw new NotImplementedException();
        }

        public void AddToIndex(VDS.RDF.Triple t)
        {
            throw new NotImplementedException();
        }

        public void AddToIndex(IEnumerable<VDS.RDF.Triple> ts)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromIndex(VDS.RDF.Triple t)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromIndex(IEnumerable<VDS.RDF.Triple> ts)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
