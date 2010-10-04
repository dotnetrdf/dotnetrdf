using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;

namespace Alexandria.Indexing
{
    public class MongoDBIndexManager : IIndexManager
    {
        private MongoDBDocumentManager _manager;

        public MongoDBIndexManager(MongoDBDocumentManager manager)
        {
            this._manager = manager;
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return Enumerable.Empty<Triple>();
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            return Enumerable.Empty<Triple>();
        }

        public void AddToIndex(Triple t)
        {
            
        }

        public void AddToIndex(IEnumerable<Triple> ts)
        {
            
        }

        public void RemoveFromIndex(Triple t)
        {
            
        }

        public void RemoveFromIndex(IEnumerable<Triple> ts)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
