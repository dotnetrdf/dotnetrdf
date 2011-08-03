#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VDS.RDF.Writing.Serialization
{
    class GraphDeserializationInfo
    {
        private List<Triple> _triples;
        private List<KeyValuePair<String, String>> _namespaces;
        private Uri _baseUri;

        public GraphDeserializationInfo(SerializationInfo info, StreamingContext context)
        {
            this._triples = (List<Triple>)info.GetValue("triples", typeof(List<Triple>));
            this._namespaces = (List<KeyValuePair<String, String>>)info.GetValue("namespaces", typeof(List<KeyValuePair<String, String>>));
            String baseUri = info.GetString("base");
            if (!baseUri.Equals(String.Empty))
            {
                this._baseUri = new Uri(baseUri);
            }
        }

        public void Apply(IGraph g)
        {
            g.BaseUri = this._baseUri;
            g.Assert(this._triples);
            foreach (KeyValuePair<String, String> ns in this._namespaces)
            {
                g.NamespaceMap.AddNamespace(ns.Key, new Uri(ns.Value));
            }
        }
    }
}


#endif