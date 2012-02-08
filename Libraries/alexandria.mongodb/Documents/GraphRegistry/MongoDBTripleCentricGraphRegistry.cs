using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.Alexandria.Documents;
using VDS.RDF;

namespace VDS.Alexandria.Documents.GraphRegistry
{
    public class MongoDBTripleCentricGraphRegistry : BaseGraphRegistry
    {

        private MongoDBDocumentManager _manager;

        public MongoDBTripleCentricGraphRegistry(MongoDBDocumentManager manager)
        {
            this._manager = manager;
        }

        public override bool RegisterGraph(string graphUri, string name)
        {
            //Nothing to do, in this schema saving the Graph is sufficient to register it
            return true;
        }

        public override bool UnregisterGraph(string graphUri, string name)
        {
            //Nothing to do, in this schema deleting the Graph is sufficient to unregister it
            return true;
        }

        public override IEnumerable<string> DocumentNames
        {
            get 
            {
                Document exists = new Document();
                exists["$exists"] = true;
                Document lookup = new Document();
                lookup["name"] = exists;

                return (from doc in this._manager.Database[this._manager.Collection].Find(lookup).Documents
                        select (String)doc["name"]);
            }
        }

        public override IEnumerable<string> GraphUris
        {
            get 
            {
                Document exists = new Document();
                exists["$exists"] = true;
                Document lookup = new Document();
                lookup["uri"] = exists;

                return (from doc in this._manager.Database[this._manager.Collection].Find(lookup).Documents
                        select (String)doc["uri"]);
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> DocumentToGraphMappings
        {
            get 
            {
                Document exists = new Document();
                exists["$exists"] = true;
                Document lookup = new Document();
                lookup["name"] = exists;
                lookup["uri"] = exists;

                return (from doc in this._manager.Database[this._manager.Collection].Find(lookup).Documents
                        select new KeyValuePair<String, String>((String)doc["name"], (String)doc["uri"]));
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> GraphToDocumentMappings
        {
            get 
            {
                Document exists = new Document();
                exists["$exists"] = true;
                Document lookup = new Document();
                lookup["name"] = exists;
                lookup["uri"] = exists;

                return (from doc in this._manager.Database[this._manager.Collection].Find(lookup).Documents
                        select new KeyValuePair<String, String>((String)doc["uri"], (String)doc["name"]));
            }
        }
    }
}
