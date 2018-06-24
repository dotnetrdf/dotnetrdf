namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using VDS.RDF;

    partial class DynamicNode : IDictionary<string, object>
    {
        public object this[string key]
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

        public ICollection<string> Keys
        {
            get
            {
                // TODO: Shorten string as much as possible
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().Select(p => DynamicHelper.ConvertToName(p as IUriNode, this.BaseUri)).ToArray();
            }
        }

        public void Add(string key, object value)
        {
            this.Add(Convert(key), value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(string key, object value)
        {
            return this.Contains(Convert(key), value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(string key)
        {
            return this.ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<string, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate.ToString()).Distinct().ToDictionary(p => p, p => this[p]).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this.Remove(Convert(key));
        }

        public bool Remove(string key, object value)
        {
            return this.Remove(Convert(key), value);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.TryGetValue(Convert(key), out value);
        }

        private Uri Convert(string key)
        {
            if (!this.TryResolveQName(key, out Uri uri))
            {
                if (!Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out uri))
                {
                    throw new FormatException("Illegal Uri.");
                }
            }

            return uri;
        }

        private bool TryResolveQName(string index, out Uri indexUri)
        {
            // TODO: This is naive
            if (!Regex.IsMatch(index, @"^\w*:\w+$"))
            {
                indexUri = null;
                return false;
            }

            indexUri = this.Graph?.ResolveQName(index);
            return true;
        }
    }
}
