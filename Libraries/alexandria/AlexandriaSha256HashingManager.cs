using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace Alexandria
{
    public abstract class AlexandriaSha256HashingManager : AlexandriaManager
    {
        private SHA256Managed _hash;

        public AlexandriaSha256HashingManager(IDocumentManager documentManager, IIndexManager indexManager)
            : base(documentManager, indexManager) { }

        protected internal override string GetDocumentName(string graphUri)
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
