using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;

namespace VDS.Alexandria.Datasets
{
    public class AlexandriaMongoDBDataset : AlexandriaDocumentDataset<Document, Document>
    {
        public AlexandriaMongoDBDataset(AlexandriaMongoDBManager manager)
            : base(manager) { }
    }
}
