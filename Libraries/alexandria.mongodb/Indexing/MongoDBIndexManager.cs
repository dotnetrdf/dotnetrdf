using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;
using MongoDB;
using Alexandria.Documents;
using Alexandria.Utilities;

namespace Alexandria.Indexing
{
    public class MongoDBIndexManager : IIndexManager
    {
        private MongoDBDocumentManager _manager;
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private static String[] RequiredIndices = new String[]
        {
            "name",
            "graph.subject",
            "graph.predicate",
            "graph.object"
        };

        public MongoDBIndexManager(MongoDBDocumentManager manager)
        {
            this._manager = manager;

            foreach (String index in RequiredIndices)
            {
                Document indexDoc = new Document();
                indexDoc[index] = 1;
                this._manager.Database[MongoDBDocumentManager.Collection].MetaData.CreateIndex(indexDoc, false);
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);

            if (subj.NodeType == NodeType.Blank)
            {
                String id = ((BlankNode)subj).InternalID;
                return new MongoDBRdfJsonEnumerator(this._manager.Database[MongoDBDocumentManager.Collection], lookup, t => t.Subject.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(id));
            }
            else
            {
                return new MongoDBRdfJsonEnumerator(this._manager.Database[MongoDBDocumentManager.Collection], lookup, t => t.Subject.Equals(subj));
            }
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
