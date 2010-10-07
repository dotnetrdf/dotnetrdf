using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;

namespace VDS.Alexandria.Documents
{
    public class MongoDBDocument : BaseDocument<Document, Document>
    {
        private MongoDBDocumentManager _manager;
        private Document _writeDoc;

        public MongoDBDocument(String name, MongoDBDocumentManager manager)
            : base(name, manager) 
        {
            this._manager = manager;
        }

        public override bool Exists
        {
            get 
            {
                Document query = new Document();
                query["name"] = this.Name;
                Document results = this._manager.Database[this._manager.Collection].FindOne(query);
                return (results != null);
            }
        }

        protected override Document BeginWriteInternal(bool append)
        {
            Document query = new Document();
            query["name"] = this.Name;
            Document current = this._manager.Database[this._manager.Collection].FindOne(query);
            if (current == null)
            {
                current = new Document();
                current["name"] = this.Name;
                this._manager.Database[this._manager.Collection].Save(current);
            }
            this._writeDoc = current;
            return current;
        }

        protected override void EndWriteInternal()
        {
            this._manager.Database[this._manager.Collection].Save(this._writeDoc);
            this._writeDoc = null;
        }

        protected override Document BeginReadInternal()
        {
            Document query = new Document();
            query["name"] = this.Name;
            Document current = this._manager.Database[this._manager.Collection].FindOne(query);
            if (current != null)
            {
                return current;
            }
            else
            {
                return null;
            }
        }

    }
}
