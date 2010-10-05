using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using MongoDB;
using Alexandria.Documents;
using Alexandria.Utilities;

namespace Alexandria.Indexing
{
    public class MongoDBIndexManager : IIndexManager
    {
        private MongoDBDocumentManager _manager;

        public MongoDBIndexManager(MongoDBDocumentManager manager)
        {
            this._manager = manager;

            if (!this.IndexExists("name"))
            {
                Document nameIndex = new Document();
                nameIndex["name"] = 1;
                this._manager.Database[MongoDBDocumentManager.Collection].MetaData.CreateIndex(nameIndex, false);
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            Document lookup = new Document();
            Document exists = new Document();
            exists["$exists"] = true;
            lookup["graph." + subj.ToString()] = exists;

            return new MongoDBRdfJsonEnumerator(this._manager.Database[MongoDBDocumentManager.Collection], lookup, t => t.Subject.Equals(subj));
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
            foreach (INode s in ts.Select(t => t.Subject).Distinct())
            {
                if (!this.IndexExists(s))
                {
                    Document index = new Document();
                    index["graph." + s.ToString()] = 1;
                    this._manager.Database[MongoDBDocumentManager.Collection].MetaData.CreateIndex(index, false);
                }
            }
        }

        public void RemoveFromIndex(Triple t)
        {
            
        }

        public void RemoveFromIndex(IEnumerable<Triple> ts)
        {
            
        }

        private bool IndexExists(INode s)
        {
            String indexName = "graph" + s.ToString();
            return this.IndexExists(indexName);
        }

        private bool IndexExists(String indexName)
        {
            return this._manager.Database[MongoDBDocumentManager.Collection].MetaData.Indexes.Any(kvp => kvp.Value[indexName] != null);
        }

        public void Dispose()
        {
            
        }
    }
}
