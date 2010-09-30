using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    /// <summary>
    /// Manages an Alexandria Store which is stored in a directory in the filesystem
    /// </summary>
    public class AlexandriaFileManager : AlexandriaSha256HashingManager
    {
        public AlexandriaFileManager(FileDocumentManager manager)
            : base(manager, new FileIndexManager(manager)) { }

        public AlexandriaFileManager(String directory)
            : this(new FileDocumentManager(directory)) { }
    }
}
