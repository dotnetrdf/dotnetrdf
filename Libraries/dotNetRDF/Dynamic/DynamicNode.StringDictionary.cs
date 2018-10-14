namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<string, object>
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
            Add(Convert(key), value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(string key, object value)
        {
            return Contains(Convert(key), value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(Convert(key));
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
            return Remove(Convert(key));
        }

        public bool Remove(string key, object value)
        {
            return Remove(Convert(key), value);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValue(Convert(key), out value);
        }

        private Uri Convert(string key)
        {
            return DynamicHelper.Convert(key, this.Graph);
        }
    }
}
