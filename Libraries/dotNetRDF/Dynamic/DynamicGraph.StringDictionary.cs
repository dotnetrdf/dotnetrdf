namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        public object this[string key]
        {
            get => this[DynamicHelper.Convert(key, this)];
            set => this[DynamicHelper.Convert(key, this)] = value;
        }

        ICollection<string> IDictionary<string, object>.Keys => this.Triples.SubjectNodes.UriNodes().Select(u => DynamicHelper.ConvertToName(u, this.SubjectBaseUri)).ToArray();

        public void Add(string key, object value)
        {
            this.Add(DynamicHelper.Convert(key, this), value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.Contains(new KeyValuePair<Uri, object>(DynamicHelper.Convert(item.Key, this), item.Value));
        }

        public bool ContainsKey(string key)
        {
            return this.ContainsKey(DynamicHelper.Convert(key, this));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            return this.Remove(DynamicHelper.Convert(key, this));
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.Remove(new KeyValuePair<Uri, object>(DynamicHelper.Convert(item.Key, this), item.Value));
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.TryGetValue(DynamicHelper.Convert(key, this), out value);
        }
    }
}
