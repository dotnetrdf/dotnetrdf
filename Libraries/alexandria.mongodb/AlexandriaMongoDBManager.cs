using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    public class AlexandriaMongoDBManager : AlexandriaManager<Document, Document>
    {
        public AlexandriaMongoDBManager(MongoDBDocumentManager manager)
            : base(manager, new MongoDBIndexManager(manager)) { }

        public AlexandriaMongoDBManager(String db)
            : this(new MongoDBDocumentManager(db)) { }

        public AlexandriaMongoDBManager(MongoConfiguration config, String db)
            : this(new MongoDBDocumentManager(config, db)) { }

        public AlexandriaMongoDBManager(String connectionString, String db)
            : this(new MongoDBDocumentManager(connectionString, db)) { }
    }
}
