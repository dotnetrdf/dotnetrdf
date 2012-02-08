using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Documents.Adaptors
{
    public class MongoDBGraphCentricAdaptor : IDataAdaptor<Document,Document>
    {
        private JsonNTriplesParser _parser = new JsonNTriplesParser();
        private JsonNTriplesWriter _writer = new JsonNTriplesWriter();

        public void ToGraph(IGraph g, IDocument<Document, Document> document)
        {
            //Get our JSON String
            Document mongoDoc = document.BeginRead();
            String json = MongoDBHelper.DocumentListToJsonArray(mongoDoc["graph"]);
            document.EndRead();
            if (json.Equals(String.Empty)) return;

            //Then parse this
            this._parser.Load(g, new StringReader(json));
        }

        public void ToHandler(IRdfHandler handler, IDocument<Document, Document> document)
        {
            //Get our JSON String
            Document mongoDoc = document.BeginRead();
            String json = MongoDBHelper.DocumentListToJsonArray(mongoDoc["graph"]);
            document.EndRead();
            if (json.Equals(String.Empty)) return;

            //Then parse this
            this._parser.Load(handler, new StringReader(json));
        }

        public void ToDocument(IGraph g, IDocument<Document, Document> document)
        {
            //Generate our JSON String
            String json = VDS.RDF.Writing.StringWriter.Write(g, this._writer);

            //Then convert this to a Document
            Document mongoDoc = document.BeginWrite(false);
            mongoDoc["graph"] = MongoDBHelper.JsonArrayToObjects(json);
            document.EndWrite();
        }

        public void AppendTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            //Generate our JSON String
            Graph g = new Graph();
            g.Assert(ts);
            String json = VDS.RDF.Writing.StringWriter.Write(g, this._writer);

            //Then convert this to a Document
            Document mongoDoc = document.BeginWrite(false);
            Object[] temp = MongoDBHelper.JsonArrayToObjects(json);
            if (mongoDoc["graph"] != null)
            {
                List<Object> existing = MongoDBHelper.DocumentListToObjectList(mongoDoc["graph"]);
                existing.AddRange(temp);
                mongoDoc["graph"] = existing.ToArray();
            }
            else
            {
                //If it was null this was a new Document
                mongoDoc["graph"] = temp;
            }
            document.EndWrite();
        }

        public void DeleteTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            //Generate the JSON String
            Graph g = new Graph();
            g.Assert(ts);
            String json = VDS.RDF.Writing.StringWriter.Write(g, this._writer);
            Object[] toDelete = MongoDBHelper.JsonArrayToObjects(json);

            //Then load the existing Triples from the Document
            Document mongoDoc = document.BeginWrite(false);
            if (mongoDoc["graph"] != null)
            {
                //Only need to do something if the Graph is non-empty
                List<Object> existing = MongoDBHelper.DocumentListToObjectList(mongoDoc["graph"]);
                foreach (Object obj in toDelete)
                {
                    existing.Remove(obj);
                }
                mongoDoc["graph"] = existing.ToArray();
            }
            document.EndWrite();
        }
    }
}
