using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Indexing;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria
{
    /// <summary>
    /// Selects the MongoDB schema that your data is stored in
    /// </summary>
    public enum MongoDBSchemas
    {
        /// <summary>
        /// Graph-centric schema (default)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Graph-centric schema stores each Graph in a single Document plus one additional Document to store the relations between Document Names and Graph URIs.  This schema requires less read/write operations but these are larger operations and is best for stores which are primarily used just for saving and retrieving graphs though query performance is adequate
        /// </para>
        /// </remarks>
        GraphCentric,
        /// <summary>
        /// Triple-centric schema
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Triple-centric schema stores each Triple in it's own Document plus a Document per Graph to store the relation between Document Name and Graph URI.  This schema requires more read/write operations but these are smaller operations, the advantage of this schema is that it gives better query performance
        /// </para>
        /// </remarks>
        TripleCentric
    }

    public class AlexandriaMongoDBManager : AlexandriaDocumentStoreManager<Document, Document>
    {
        public AlexandriaMongoDBManager(MongoDBDocumentManager manager)
            : base(manager, MongoDBHelper.GetIndexManager(manager)) { }

        public AlexandriaMongoDBManager(String db)
            : this(new MongoDBDocumentManager(db)) { }

        public AlexandriaMongoDBManager(String db, MongoDBSchemas schema)
            : this(new MongoDBDocumentManager(db, schema)) { }

        public AlexandriaMongoDBManager(MongoConfiguration config, String db)
            : this(new MongoDBDocumentManager(config, db)) { }

        public AlexandriaMongoDBManager(MongoConfiguration config, String db, MongoDBSchemas schema)
            : this(new MongoDBDocumentManager(config, db, schema)) { }

        public AlexandriaMongoDBManager(String connectionString, String db)
            : this(new MongoDBDocumentManager(connectionString, db)) { }

        public AlexandriaMongoDBManager(String connectionString, String db, MongoDBSchemas schema)
            : this(new MongoDBDocumentManager(connectionString, db, schema)) { }

        internal IDocumentManager<Document, Document> DocumentManager
        {
            get
            {
                return base.DocumentManager;
            }
        }

        internal IIndexManager IndexManager
        {
            get
            {
                return base.IndexManager;
            }
        }
    }
}
