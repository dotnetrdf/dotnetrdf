using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;
using MongoDB;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Indexing
{
    public class MongoDBGraphCentricIndexManager : IIndexManager
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

        public MongoDBGraphCentricIndexManager(MongoDBDocumentManager manager)
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
            for (int i = 1; i < RequiredIndices.Length; i++)
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
            fullIndex["graph.subject"] = 1;
            fullIndex["graph.predicate"] = 1;
            fullIndex["graph.object"] = 1;
            this._manager.Database[this._manager.Collection].MetaData.CreateIndex(fullIndex, false);
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Subject.Equals(subj));
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            Document lookup = new Document();
            lookup["graph.predicate"] = this._formatter.Format(pred);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Predicate.Equals(pred));
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            Document lookup = new Document();
            lookup["graph.object"] = this._formatter.Format(obj);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Object.Equals(obj));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);
            lookup["graph.predicate"] = this._formatter.Format(pred);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Subject.Equals(subj) && t.Predicate.Equals(pred));
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            Document lookup = new Document();
            lookup["graph.predicate"] = this._formatter.Format(pred);
            lookup["graph.object"] = this._formatter.Format(obj);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Predicate.Equals(pred) && t.Object.Equals(obj));
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);
            lookup["graph.object"] = this._formatter.Format(obj);
            return new MongoDBGraphCentricEnumerable(this._manager, lookup, t => t.Subject.Equals(subj) && t.Object.Equals(obj));
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(t.Subject);
            lookup["graph.predicate"] = this._formatter.Format(t.Predicate);
            lookup["graph.object"] = this._formatter.Format(t.Object);

            return new MongoDBGraphCentricEnumerable(this._manager, lookup, x => x.Subject.Equals(t.Subject) && x.Predicate.Equals(t.Predicate) && x.Object.Equals(t.Object));
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
