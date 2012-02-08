using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Alexandria.Datasets
{
    public class AlexandriaFileDataset : AlexandriaDocumentDataset<StreamReader, TextWriter>
    {
        private AlexandriaFileManager _fileManager;

        public AlexandriaFileDataset(AlexandriaFileManager manager)
            : base(manager) 
        {
            this._fileManager = manager;
        }

        protected override IEnumerable<Triple> GetAllTriples()
        {
            return new FileEnumerable(this._fileManager);
        }
    }
}
