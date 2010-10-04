using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using MongoDB.Connections;
using Alexandria.Documents.Adaptors;
using Alexandria.Documents.GraphRegistry;
using Alexandria.Utilities;

namespace Alexandria.Documents
{
    public class MongoDBDocumentManager : BaseDocumentManager<Document,Document>
    {
        private Mongo _connection;
        private IMongoDatabase _db;
        private IGraphRegistry _registry;

        internal const String Collection = "dotnetrdf";
        private const String GraphRegistryDocument = "graphs";

        public MongoDBDocumentManager(MongoConfiguration config, String db)
            : base(new MongoDBRdfJsonAdaptor())
        {
            this._connection = new Mongo(config);
            this._db = this._connection.GetDatabase(db);
            this._connection.Connect();

            //Ensure the DB is setup correctly
            this._db.GetCollection(Collection);

            if (!this.HasDocument(GraphRegistryDocument))
            {
                if (!this.CreateDocument(GraphRegistryDocument))
                {
                    throw new AlexandriaException("Unable to create the Required Graph Registry Document");
                }
            }
            this._registry = new MongoDBGraphRegistry(this.GetDocument(GraphRegistryDocument));
        }

        public MongoDBDocumentManager(String db)
            : this(new MongoConfiguration(), db) { }

        public MongoDBDocumentManager(String connectionString, String db)
            : this(MongoDBHelper.GetConfiguration(connectionString), db) { }

        internal IMongoDatabase Database
        {
            get
            {
                return this._db;
            }
        }

        protected override bool HasDocumentInternal(string name)
        {
            MongoDBDocument doc = new MongoDBDocument(name, this);
            return doc.Exists;
        }

        protected override bool CreateDocumentInternal(string name)
        {
            MongoDBDocument doc = new MongoDBDocument(name, this);
            doc.BeginWrite(true);
            doc.EndWrite();
            return true;
        }

        protected override bool DeleteDocumentInternal(string name)
        {
            throw new NotImplementedException();
        }

        protected override IDocument<Document, Document> GetDocumentInternal(string name)
        {
            MongoDBDocument doc = new MongoDBDocument(name, this);
            if (doc.Exists)
            {
                return doc;
            }
            else
            {
                throw new AlexandriaException("The requested Document " + name + " is not present in this Store");
            }
        }

        public override IGraphRegistry GraphRegistry
        {
            get 
            {
                return this._registry;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            this._connection.Disconnect();
        }
    }
}
