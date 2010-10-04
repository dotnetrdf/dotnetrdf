using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;

namespace Alexandria.Documents.GraphRegistry
{
    public class MongoDBGraphRegistry : BaseGraphRegistry
    {
        private IDocument<Document, Document> _doc;

        public MongoDBGraphRegistry(IDocument<Document, Document> doc)
        {
            this._doc = doc;
        }

        public override string GetGraphUri(string name)
        {
            throw new NotImplementedException();
        }

        public override bool RegisterGraph(string graphUri, string name)
        {
            Document mongoDoc = this._doc.BeginWrite(false);
            List<Document> graphs = (mongoDoc["graphs"] != null) ? (List<Document>)mongoDoc["graphs"] : new List<Document>();
            Document reg = new Document();
            reg["name"] = name;
            reg["uri"] = graphUri;
            graphs.Add(reg);
            mongoDoc["graphs"] = graphs;
            this._doc.EndWrite();
            return true;
        }

        public override bool UnregisterGraph(string graphUri, string name)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> DocumentNames
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<string> GraphUris
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<KeyValuePair<string, string>> DocumentToGraphMappings
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<KeyValuePair<string, string>> GraphToDocumentMappings
        {
            get { throw new NotImplementedException(); }
        }
    }
}
