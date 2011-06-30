using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.Alexandria.Utilities;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.Documents.Adaptors
{
    public class MongoDBTripleCentricAdaptor : IDataAdaptor<Document, Document>
    {
        private MongoDBDocumentManager _manager;
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public MongoDBTripleCentricAdaptor(MongoDBDocumentManager manager)
        {
            this._manager = manager;
        }

        public void ToGraph(IGraph g, IDocument<Document, Document> document)
        {
            Document mongoDoc = document.BeginRead();
            Document lookup = new Document();
            lookup["graphuri"] = mongoDoc["uri"];
            document.EndRead();

            IEnumerable<Triple> ts = new MongoDBTripleCentricEnumerable(this._manager, lookup);
            foreach (Triple t in ts)
            {
                g.Assert(Tools.CopyTriple(t, g));
            }
        }

        public void ToHandler(IRdfHandler handler, IDocument<Document, Document> document)
        {
            Document mongoDoc = document.BeginRead();
            Document lookup = new Document();
            lookup["graphuri"] = mongoDoc["uri"];
            document.EndRead();

            IEnumerable<Triple> ts = new MongoDBTripleCentricEnumerable(this._manager, lookup);
            handler.Apply(ts);
        }

        public void ToDocument(IGraph g, IDocument<Document, Document> document)
        {
            Document mongoDoc = document.BeginWrite(false);
            //Assign URI if necessary
            String graphUri = (g.BaseUri == null) ? String.Empty : g.BaseUri.ToString();
            if (mongoDoc["uri"] == null)
            {
                mongoDoc["uri"] = graphUri;
            }
            document.EndWrite();

            //Then create and save a Document for every Triple in the Graph
            foreach (Triple t in g.Triples)
            {
                Document tripleDoc = new Document();
                tripleDoc["subject"] = this._formatter.Format(t.Subject);
                tripleDoc["predicate"] = this._formatter.Format(t.Predicate);
                tripleDoc["object"] = this._formatter.Format(t.Object);
                tripleDoc["graphuri"] = graphUri;

                //Only Save if not already in Store
                if (this._manager.Database[this._manager.Collection].FindOne(tripleDoc) == null)
                {
                    this._manager.Database[this._manager.Collection].Save(tripleDoc);
                }
            }
        }

        public void AppendTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            Document mongoDoc = document.BeginRead();
            String graphUri = (String)mongoDoc["uri"];
            document.EndRead();

            foreach (Triple t in ts)
            {
                Document tripleDoc = new Document();
                tripleDoc["subject"] = this._formatter.Format(t.Subject);
                tripleDoc["predicate"] = this._formatter.Format(t.Predicate);
                tripleDoc["object"] = this._formatter.Format(t.Object);
                tripleDoc["graphuri"] = graphUri;

                //Only Save if not already in Store
                if (this._manager.Database[this._manager.Collection].FindOne(tripleDoc) == null)
                {
                    this._manager.Database[this._manager.Collection].Save(tripleDoc);
                }
            }
        }

        public void DeleteTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            Document mongoDoc = document.BeginRead();
            String graphUri = (String)mongoDoc["uri"];
            document.EndRead();

            foreach (Triple t in ts)
            {
                Document tripleDoc = new Document();
                tripleDoc["subject"] = this._formatter.Format(t.Subject);
                tripleDoc["predicate"] = this._formatter.Format(t.Predicate);
                tripleDoc["object"] = this._formatter.Format(t.Object);
                tripleDoc["graphuri"] = graphUri;

                this._manager.Database[this._manager.Collection].Delete(tripleDoc);
            }
        }
    }
}
