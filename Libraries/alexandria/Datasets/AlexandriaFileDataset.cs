using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Alexandria.Datasets
{
    public class AlexandriaFileDataset : AlexandriaDocumentDataset<StreamReader, TextWriter>
    {

        public AlexandriaFileDataset(AlexandriaFileManager manager)
            : base(manager) { }
    }
}
