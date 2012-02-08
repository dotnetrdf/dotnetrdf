using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Utilities;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.Indexing
{
    public class MongoDBTripleCentricIndexManager : IIndexManager
    {
        private MongoDBDocumentManager _manager;
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private static String[] RequiredIndices = new String[]
        {
            "name",
            "uri",
            "graphuri",
            "subject",
            "predicate",
            "object"
        };

        public MongoDBTripleCentricIndexManager(MongoDBDocumentManager manager)
        {
            this._manager = manager;

            //Ensure Basic Indices
            foreach (String index in RequiredIndices)
            {
                Document indexDoc = new Document();
                indexDoc[index] = 1;
                this._manager.Database[this._manager.Collection].MetaData.CreateIndex(indexDoc, false);
            }

            //Ensure Compound Indices
            for (int i = 3; i < RequiredIndices.Length; i++)
            {
                for (int j = i; j < RequiredIndices.Length; j++)
                {
                    if (i == j) continue;
                    Document indexDoc = new Document();
                    indexDoc[RequiredIndices[i]] = 1;
                    indexDoc[RequiredIndices[j]] = 1;
                    this._manager.Database[this._manager.Collection].MetaData.CreateIndex(indexDoc, false);
                }
            }

            Document fullIndex = new Document();
            fullIndex["subject"] = 1;
            fullIndex["predicate"] = 1;
            fullIndex["object"] = 1;
            this._manager.Database[this._manager.Collection].MetaData.CreateIndex(fullIndex, false);
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            Document lookup = new Document();
            lookup["subject"] = this._formatter.Format(subj);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            Document lookup = new Document();
            lookup["predicate"] = this._formatter.Format(pred);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            Document lookup = new Document();
            lookup["object"] = this._formatter.Format(obj);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            Document lookup = new Document();
            lookup["subject"] = this._formatter.Format(subj);
            lookup["predicate"] = this._formatter.Format(pred);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            Document lookup = new Document();
            lookup["predicate"] = this._formatter.Format(pred);
            lookup["object"] = this._formatter.Format(obj);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            Document lookup = new Document();
            lookup["subject"] = this._formatter.Format(subj);
            lookup["object"] = this._formatter.Format(obj);
            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            Document lookup = new Document();
            lookup["subject"] = this._formatter.Format(t.Subject);
            lookup["predicate"] = this._formatter.Format(t.Predicate);
            lookup["object"] = this._formatter.Format(t.Object);

            return new MongoDBTripleCentricEnumerable(this._manager, lookup);
        }

        public void AddToIndex(Triple t)
        {
            //Nothing to do - MongoDB manages indexing itself
        }

        public void AddToIndex(IEnumerable<Triple> ts)
        {
            //Nothing to do - MongoDB manages indexing itself
        }

        public void RemoveFromIndex(Triple t)
        {
            //Nothing to do - MongoDB manages indexing itself
        }

        public void RemoveFromIndex(IEnumerable<Triple> ts)
        {
            //Nothing to do - MongoDB manages indexing itself
        }

        public void Flush()
        {
            //No flush actions required - MongoDB manages indexing itself
        }

        public void Dispose()
        {
            //No dispose actions required - MongoDB handles all the indexing at the server end
        }
    }
}
