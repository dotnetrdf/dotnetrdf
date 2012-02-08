using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using VDS.RDF;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Datasets
{
    public class AlexandriaMongoDBDataset : AlexandriaDocumentDataset<Document, Document>
    {
        private AlexandriaMongoDBManager _mongoManager;

        public AlexandriaMongoDBDataset(AlexandriaMongoDBManager manager)
            : base(manager) 
        {
            this._mongoManager = manager;
        }

        protected override IEnumerable<Triple> GetAllTriples()
        {
            Document lookup, exists;

            switch (((MongoDBDocumentManager)this._mongoManager.DocumentManager).Schema)
            {
                case MongoDBSchemas.GraphCentric:
                    lookup = new Document();
                    exists = new Document();
                    exists["$exists"] = true;
                    lookup["graph"] = exists;
                    return new MongoDBGraphCentricEnumerable((MongoDBDocumentManager)this._mongoManager.DocumentManager, lookup, t => true);

                case MongoDBSchemas.TripleCentric:
                    lookup = new Document();
                    exists = new Document();
                    exists["$exists"] = true;
                    lookup["subject"] = exists;
                    lookup["predicate"] = exists;
                    lookup["object"] = exists;
                    return new MongoDBTripleCentricEnumerable((MongoDBDocumentManager)this._mongoManager.DocumentManager, lookup);

                default:
                    throw new NotSupportedException("Unknown Schemas for MongoDB are not supported");
            }
            
        }
    }
}
