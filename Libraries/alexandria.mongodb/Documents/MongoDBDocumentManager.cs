using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using MongoDB.Connections;
using VDS.Alexandria.Documents.Adaptors;
using VDS.Alexandria.Documents.GraphRegistry;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Documents
{
    public class MongoDBDocumentManager : BaseDocumentManager<Document,Document>
    {
        private Mongo _connection;
        private IMongoDatabase _db;
        private IGraphRegistry _registry;
        private String _collection;
        private MongoDBSchemas _schema = MongoDBSchemas.TripleCentric;

        /// <summary>
        /// Default Schema used if none is explicitly specified
        /// </summary>
        public const MongoDBSchemas DefaultSchema = MongoDBSchemas.TripleCentric;
        private const String GraphRegistryDocument = "graphs";
        /// <summary>
        /// Default Collection used if none is explicitly specified
        /// </summary>
        public const String DefaultCollection = "dotnetrdf";

        public MongoDBDocumentManager(MongoConfiguration config, String db, String collection, MongoDBSchemas schema)
            : base(null)
        {
            this._connection = new Mongo(config);
            this._db = this._connection.GetDatabase(db);
            this._connection.Connect();
            this._collection = collection;

            //Ensure the DB is setup correctly
            this._db.GetCollection(Collection);

            //Set up the Data Adaptor and Graph Registry
            this._schema = schema;
            switch (schema)
            {
                case MongoDBSchemas.GraphCentric:
                    this.DataAdaptor = new MongoDBGraphCentricAdaptor();
                    if (!this.HasDocument(GraphRegistryDocument))
                    {
                        if (!this.CreateDocument(GraphRegistryDocument))
                        {
                            throw new AlexandriaException("Unable to create the Required Graph Registry Document");
                        }
                    }
                    this._registry = new MongoDBGraphCentricRegistry(this.GetDocument(GraphRegistryDocument));
                    break;

                case MongoDBSchemas.TripleCentric:
                    this.DataAdaptor = new MongoDBTripleCentricAdaptor(this);
                    this._registry = new MongoDBTripleCentricGraphRegistry(this);
                    break;

                default:
                    throw new ArgumentException("Unknown MongoDB Schema", "schema");
            }
        }

        public MongoDBDocumentManager(MongoConfiguration config, String db, String collection)
            : this(config, db, collection, DefaultSchema) { }

        public MongoDBDocumentManager(MongoConfiguration config, String db)
            : this(config, db, DefaultCollection) { }

        public MongoDBDocumentManager(MongoConfiguration config, String db, MongoDBSchemas schema)
            : this(config, db, DefaultCollection, schema) { }

        public MongoDBDocumentManager(String db)
            : this(new MongoConfiguration(), db) { }

        public MongoDBDocumentManager(String db, MongoDBSchemas schema)
            : this(new MongoConfiguration(), db, schema) { }

        public MongoDBDocumentManager(String connectionString, String db)
            : this(MongoDBHelper.GetConfiguration(connectionString), db) { }

        public MongoDBDocumentManager(String connectionString, String db, MongoDBSchemas schema)
            : this(MongoDBHelper.GetConfiguration(connectionString), db, schema) { }

        public MongoDBDocumentManager(String connectionString, String db, String collection)
            : this(MongoDBHelper.GetConfiguration(connectionString), db, collection) { }

        public MongoDBDocumentManager(String connectionString, String db, String collection, MongoDBSchemas schema)
            : this(MongoDBHelper.GetConfiguration(connectionString), db, collection, schema) { }


        internal IMongoDatabase Database
        {
            get
            {
                return this._db;
            }
        }

        internal String Collection
        {
            get
            {
                return this._collection;
            }
        }

        internal MongoDBSchemas Schema
        {
            get
            {
                return this._schema;
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
            MongoDBDocument doc = new MongoDBDocument(name, this);
            if (!doc.Exists) return false;
            Document mongoDoc = doc.BeginRead();
            doc.EndRead();
            this._db[Collection].Remove(mongoDoc);

            //For Triple-Centric schema there are many documents to delete
            if (this._schema == MongoDBSchemas.TripleCentric)
            {
                Document deleteTriples = new Document();
                deleteTriples["graphuri"] = mongoDoc["uri"];
                this._db[Collection].Remove(deleteTriples);
            }

            return true;
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
