using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;

namespace Alexandria.Documents
{
    public class MongoDBDocument : BaseDocument<Document, Document>
    {
        private IMongoDatabase _db;
        private Document _writeDoc;

        public MongoDBDocument(String name, MongoDBDocumentManager manager)
            : base(name, manager) 
        {
            this._db = manager.Database;
        }

        public override bool Exists
        {
            get 
            {
                Document query = new Document();
                query["name"] = this.Name;
                Document results = this._db[MongoDBDocumentManager.Collection].FindOne(query);
                return (results != null);
            }
        }

        protected override Document BeginWriteInternal(bool append)
        {
            Document query = new Document();
            query["name"] = this.Name;
            Document current = this._db[MongoDBDocumentManager.Collection].FindOne(query);
            if (current == null)
            {
                current = new Document();
                current["name"] = this.Name;
                this._db[MongoDBDocumentManager.Collection].Save(current);
            }
            this._writeDoc = current;
            return current;
        }

        protected override void EndWriteInternal()
        {
            this._db[MongoDBDocumentManager.Collection].Save(this._writeDoc);
            this._writeDoc = null;
        }

        protected override Document BeginReadInternal()
        {
            Document query = new Document();
            query["name"] = this.Name;
            Document current = this._db[MongoDBDocumentManager.Collection].FindOne(query);
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
