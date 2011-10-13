using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VDS.Alexandria.Documents.GraphRegistry
{
    public abstract class BaseGraphRegistry : IGraphRegistry, IDisposable
    {
        private SHA256Managed _hash;

        public virtual string GetDocumentName(string graphUri)
        {
            //Only instantiate the SHA256 class when we first use it
            if (_hash == null) _hash = new SHA256Managed();

            if (String.IsNullOrEmpty(graphUri)) return "default-graph";

            Byte[] input = Encoding.UTF8.GetBytes(graphUri);
            Byte[] output = _hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        public virtual String GetGraphUri(String name)
        {
            return this.DocumentToGraphMappings.Where(kvp => kvp.Key.Equals(name)).Select(kvp => kvp.Value).FirstOrDefault().ToSafeString();
        }

        public abstract bool RegisterGraph(String graphUri, String name);

        public abstract bool UnregisterGraph(String graphUri, String name);

        public abstract IEnumerable<String> DocumentNames
        {
            get;
        }

        public abstract IEnumerable<String> GraphUris
        {
            get;
        }

        public abstract IEnumerable<KeyValuePair<String, String>> DocumentToGraphMappings
        {
            get;
        }

        public abstract IEnumerable<KeyValuePair<String, String>> GraphToDocumentMappings
        {
            get;
        }

        public virtual void Dispose()
        {
            this._hash = null;
        }
    }
}
