using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.Alexandria.Documents;

namespace VDS.Alexandria.Utilities
{
    public class MongoDBTripleCentricEnumerator : IEnumerator<Triple>
    {
        private MongoDBDocumentManager _manager;
        private Document _query;
        private Triple _current;

        private IEnumerator<Document> _docs;

        public MongoDBTripleCentricEnumerator(MongoDBDocumentManager manager, Document query)
        {
            this._manager = manager;
            this._query = query;
        }

        public Triple Current
        {
            get 
            {
                if (this._docs == null) throw new InvalidOperationException("Enumerator is positioned before the start of the collection");
                if (this._current == null) throw new InvalidOperationException("Enumerator is positioned after the end of the collection");
                Triple temp = this._current;
                this._current = null;
                return temp;
            }
        }

        public void Dispose()
        {
            if (this._docs != null)
            {
                this._docs.Dispose();
                this._docs = null;
            }
            this._current = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            {
                return this.Current; 
            }
        }

        public bool MoveNext()
        {
            if (this._docs == null)
            {
                //Need to create the cursor
                this._docs = this._manager.Database[this._manager.Collection].Find(this._query).Documents.GetEnumerator();
            }

            if (this._docs.MoveNext())
            {
                Document currDoc = this._docs.Current;

                //Determine the Graph to use
                IGraph g;
                if (currDoc["graphuri"] != null)
                {
                    String uri = (String)currDoc["graphuri"];
                    if (uri != null && !uri.Equals(String.Empty))
                    {
                        g = this._manager.GraphFactory[new Uri(uri)];
                    } 
                    else 
                    {
                        g = this._manager.GraphFactory[null];
                    }
                } 
                else 
                {
                    g = this._manager.GraphFactory[null];
                }

                //Parse the Triple Parts
                INode subj = JsonNTriplesParser.TryParseNodeValue(g, (String)currDoc["subject"]);
                INode pred = JsonNTriplesParser.TryParseNodeValue(g, (String)currDoc["predicate"]);
                INode obj = JsonNTriplesParser.TryParseNodeValue(g, (String)currDoc["object"]);

                this._current = new Triple(subj, pred, obj);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("The Reset() operation is not supported by the MongoDB Triple-Centric Enumerator");
        }
    }

    public class MongoDBTripleCentricEnumerable : IEnumerable<Triple>
    {
        private MongoDBDocumentManager _manager;
        private Document _query;

        public MongoDBTripleCentricEnumerable(MongoDBDocumentManager manager, Document query)
        {
            this._manager = manager;
            this._query = query;
        }

        public IEnumerator<Triple> GetEnumerator()
        {
            return new MongoDBTripleCentricEnumerator(this._manager, this._query);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
