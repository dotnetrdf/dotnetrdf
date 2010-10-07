using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;

namespace VDS.Alexandria.Documents.GraphRegistry
{
    public class MongoDBGraphCentricRegistry : BaseGraphRegistry
    {
        private IDocument<Document, Document> _doc;

        public MongoDBGraphCentricRegistry(IDocument<Document, Document> doc)
        {
            this._doc = doc;
        }

        public override bool RegisterGraph(string graphUri, string name)
        {
            Document mongoDoc = this._doc.BeginWrite(false);
            List<Document> graphs = this.GetRegistryList(mongoDoc);
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
            Document mongoDoc = this._doc.BeginWrite(false);
            List<Document> graphs = this.GetRegistryList(mongoDoc);
            if (graphs.Count > 0)
            {
                graphs.RemoveAll(d => d["name"].Equals(name) && d["uri"].Equals(graphUri));
                mongoDoc["graphs"] = graphs;
            }
            this._doc.EndWrite();
            return true;
        }

        private List<Document> GetRegistryList(Document mongoDoc)
        {
            if (mongoDoc["graphs"] == null)
            {
                return new List<Document>();
            }
            else
            {
                Object temp = mongoDoc["graphs"];
                if (temp.GetType().Equals(typeof(List<Document>)))
                {
                    return (List<Document>)temp;
                }
                else if (temp.GetType().Equals(typeof(List<Object>)))
                {
                    return ((List<Object>)temp).Select(item => (Document)item).ToList();
                }
                else
                {
                    throw new AlexandriaException("Unable to access the Graph Registry successfully");
                }
            }
        }

        public override IEnumerable<string> DocumentNames
        {
            get 
            {
                Document mongoDoc = this._doc.BeginRead();
                List<Document> graphs = this.GetRegistryList(mongoDoc);
                this._doc.EndRead();

                return (from d in graphs
                        select d["name"].ToString());
            }
        }

        public override IEnumerable<string> GraphUris
        {
            get 
            {
                Document mongoDoc = this._doc.BeginRead();
                List<Document> graphs = this.GetRegistryList(mongoDoc);
                this._doc.EndRead();

                return (from d in graphs
                        select d["uri"].ToString()); 
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> DocumentToGraphMappings
        {
            get 
            {
                Document mongoDoc = this._doc.BeginRead();
                List<Document> graphs = this.GetRegistryList(mongoDoc);
                this._doc.EndRead();

                return (from d in graphs
                        select new KeyValuePair<String,String>(d["name"].ToString(), d["uri"].ToString())); 
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> GraphToDocumentMappings
        {
            get 
            {
                Document mongoDoc = this._doc.BeginRead();
                List<Document> graphs = this.GetRegistryList(mongoDoc);
                this._doc.EndRead();

                return (from d in graphs
                        select new KeyValuePair<String,String>(d["uri"].ToString(),d["name"].ToString())); 
            }
        }
    }
}
