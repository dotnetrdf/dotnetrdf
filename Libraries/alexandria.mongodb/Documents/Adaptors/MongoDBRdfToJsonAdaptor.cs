using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Alexandria.Utilities;

namespace Alexandria.Documents.Adaptors
{
    public class MongoDBRdfToJsonAdaptor : IDataAdaptor<Document,Document>
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
            StringParser.Parse(g, json, this._parser);
        }

        public void ToDocument(IGraph g, IDocument<Document, Document> document)
        {
            //Generate our JSON String
            String json = StringWriter.Write(g, this._writer);

            //Then convert this to a Document
            Document mongoDoc = document.BeginWrite(false);
            mongoDoc["graph"] = MongoDBHelper.JsonArrayToObjects(json);
            document.EndWrite();
        }

        public void AppendTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            throw new NotImplementedException();
        }

        public void DeleteTriples(IEnumerable<Triple> ts, IDocument<Document, Document> document)
        {
            throw new NotImplementedException();
        }
    }
}
