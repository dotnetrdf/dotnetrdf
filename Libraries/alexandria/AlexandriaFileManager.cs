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
    public class AlexandriaFileManager : AlexandriaManager
    {
        private SHA256Managed _hash;

        public AlexandriaFileManager(FileDocumentManager manager)
            : base(manager, new FileIndexManager(manager)) { }

        public AlexandriaFileManager(String directory)
            : this(new FileDocumentManager(directory)) { }

        protected override string GetDocumentName(string graphUri)
        {
            //Only instantiate the SHA256 class when we first use it
            if (_hash == null) _hash = new SHA256Managed();

            if (graphUri.Equals(String.Empty) || graphUri == null) return "default-graph";

            Byte[] input = Encoding.UTF8.GetBytes(graphUri);
            Byte[] output = _hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}
