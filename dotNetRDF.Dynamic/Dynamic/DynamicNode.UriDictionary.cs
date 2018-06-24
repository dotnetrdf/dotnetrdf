namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    partial class DynamicNode : IDictionary<Uri, object>
    {
        public object this[Uri key]
        {
            get
            {
                return this[Convert(key)];
            }
            set
            {
                this[Convert(key)] = value;
            }
        }

        ICollection<Uri> IDictionary<Uri, object>.Keys
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Select(t => (t.Predicate as IUriNode).Uri).Distinct().ToArray();
            }
        }

        public void Add(Uri key, object value)
        {
            this.Add(Convert(key), value);
        }

        public void Add(KeyValuePair<Uri, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(Uri key, object value)
        {
            return this.Contains(Convert(key), value);
        }

        public bool Contains(KeyValuePair<Uri, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri key)
        {
            return this.ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<Uri, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToDictionary(p => (p as IUriNode).Uri, p => this[p]).GetEnumerator();
        }

        public bool Remove(Uri key)
        {
            return this.Remove(Convert(key));
        }

        public bool Remove(Uri key, object value)
        {
            return this.Remove(Convert(key), value);
        }

        public bool Remove(KeyValuePair<Uri, object> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return this.TryGetValue(Convert(key), out value);
        }

        private INode Convert(Uri uri)
        {
            if (this.Graph == null)
            {
                throw new InvalidOperationException("missing graph");
            }

            if (!uri.IsAbsoluteUri)
            {
                if (this.BaseUri == null)
                {
                    throw new InvalidOperationException("Can't use relative uri without baseUri.");
                }

                if (this.BaseUri.AbsoluteUri.EndsWith("#"))
                {
                    var builder = new UriBuilder(this.BaseUri) { Fragment = uri.ToString() };

                    uri = builder.Uri;
                }
                else
                {
                    uri = new Uri(this.BaseUri, uri);
                }
            }

            return this.Graph.CreateUriNode(uri);
        }
    }
}
