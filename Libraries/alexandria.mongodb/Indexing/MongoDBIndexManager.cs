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

            if (subj.NodeType == NodeType.Blank)
            {
                String id = ((BlankNode)subj).InternalID;
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(id));
            }
            else
            {
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.Equals(subj));
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            Document lookup = new Document();
            lookup["graph.predicate"] = this._formatter.Format(pred);

            if (pred.NodeType == NodeType.Blank)
            {
                String id = ((BlankNode)pred).InternalID;
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.NodeType == NodeType.Blank && ((BlankNode)t.Predicate).InternalID.Equals(id));
            }
            else
            {
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.Equals(pred));
            }
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            Document lookup = new Document();
            lookup["graph.object"] = this._formatter.Format(obj);

            if (obj.NodeType == NodeType.Blank)
            {
                String id = ((BlankNode)obj).InternalID;
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Object.NodeType == NodeType.Blank && ((BlankNode)t.Object).InternalID.Equals(id));
            }
            else
            {
                return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Object.Equals(obj));
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);
            lookup["graph.predicate"] = this._formatter.Format(pred);

            if (subj.NodeType == NodeType.Blank)
            {
                String subjID = ((BlankNode)subj).InternalID;
                if (pred.NodeType == NodeType.Blank)
                {
                    String predID = ((BlankNode)pred).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.NodeType == NodeType.Blank && t.Predicate.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(subjID) && ((BlankNode)t.Predicate).InternalID.Equals(predID));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(subjID) && t.Predicate.Equals(pred));
                }
            }
            else
            {
                if (pred.NodeType == NodeType.Blank)
                {
                    String id = ((BlankNode)pred).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.Equals(subj) && t.Predicate.NodeType == NodeType.Blank && ((BlankNode)t.Predicate).InternalID.Equals(id));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.Equals(subj) && t.Predicate.Equals(pred));
                }
            }
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            Document lookup = new Document();
            lookup["graph.predicate"] = this._formatter.Format(pred);
            lookup["graph.object"] = this._formatter.Format(obj);

            if (pred.NodeType == NodeType.Blank)
            {
                String predID = ((BlankNode)pred).InternalID;
                if (obj.NodeType == NodeType.Blank)
                {
                    String objID = ((BlankNode)obj).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank && ((BlankNode)t.Predicate).InternalID.Equals(predID) && ((BlankNode)t.Object).InternalID.Equals(objID));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.NodeType == NodeType.Blank && ((BlankNode)t.Predicate).InternalID.Equals(predID) && t.Object.Equals(obj));
                }
            }
            else
            {
                if (obj.NodeType == NodeType.Blank)
                {
                    String id = ((BlankNode)obj).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.Equals(pred) && t.Object.NodeType == NodeType.Blank && ((BlankNode)t.Object).InternalID.Equals(id));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Predicate.Equals(pred) && t.Object.Equals(obj));
                }
            }
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(subj);
            lookup["graph.object"] = this._formatter.Format(obj);

            if (subj.NodeType == NodeType.Blank)
            {
                String subjID = ((BlankNode)subj).InternalID;
                if (obj.NodeType == NodeType.Blank)
                {
                    String objID = ((BlankNode)obj).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.NodeType == NodeType.Blank && t.Object.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(subjID) && ((BlankNode)t.Object).InternalID.Equals(objID));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.NodeType == NodeType.Blank && ((BlankNode)t.Subject).InternalID.Equals(subjID) && t.Object.Equals(obj));
                }
            }
            else
            {
                if (obj.NodeType == NodeType.Blank)
                {
                    String id = ((BlankNode)obj).InternalID;
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.Equals(subj) && t.Object.NodeType == NodeType.Blank && ((BlankNode)t.Object).InternalID.Equals(id));
                }
                else
                {
                    return new MongoDBRdfJsonEnumerator(this._manager, lookup, t => t.Subject.Equals(subj) && t.Object.Equals(obj));
                }
            }
        }

        public IEnumerable<Triple> GetTriples(Triple t)
        {
            Document lookup = new Document();
            lookup["graph.subject"] = this._formatter.Format(t.Subject);
            lookup["graph.predicate"] = this._formatter.Format(t.Predicate);
            lookup["graph.object"] = this._formatter.Format(t.Object);

            Func<Triple, bool> subjFunc, predFunc, objFunc;
            if (t.Subject.NodeType == NodeType.Blank)
            {
                String subjID = ((BlankNode)t.Subject).InternalID;
                subjFunc = x => x.Subject.NodeType == NodeType.Blank && ((BlankNode)x.Subject).InternalID.Equals(subjID);
            }
            else
            {
                subjFunc = x => x.Subject.Equals(t.Subject);
            }
            if (t.Predicate.NodeType == NodeType.Blank)
            {
                String predID = ((BlankNode)t.Predicate).InternalID;
                predFunc = x => x.Predicate.NodeType == NodeType.Blank && ((BlankNode)x.Predicate).InternalID.Equals(predID);
            }
            else
            {
                predFunc = x => x.Predicate.Equals(t.Predicate);
            }
            if (t.Object.NodeType == NodeType.Blank)
            {
                String objID = ((BlankNode)t.Object).InternalID;
                objFunc = x => x.Object.NodeType == NodeType.Blank && ((BlankNode)x.Object).InternalID.Equals(objID);
            }
            else
            {
                objFunc = x => x.Object.Equals(t.Object);
            }

            return new MongoDBRdfJsonEnumerator(this._manager, lookup, x => subjFunc(x) && predFunc(x) && objFunc(x));
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

        public void Dispose()
        {
            //No dispose actions required - MongoDB handles all the indexing at the server end
        }
    }
}
