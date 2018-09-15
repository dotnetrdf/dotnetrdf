namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        public object this[string key]
        {
            get => this[DynamicHelper.Convert(key, this)];
            set => this[DynamicHelper.Convert(key, this)] = value;
        }

        ICollection<string> IDictionary<string, object>.Keys => this.Triples.SubjectNodes.UriNodes().Select(u => DynamicHelper.ConvertToName(u, this.SubjectBaseUri)).ToArray();

        void IDictionary<string, object>.Add(string key, object value) => throw new NotImplementedException();

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => throw new NotImplementedException();

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => throw new NotImplementedException();

        bool IDictionary<string, object>.ContainsKey(string key) => throw new NotImplementedException();

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => throw new NotImplementedException();

        bool IDictionary<string, object>.Remove(string key) => throw new NotImplementedException();

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => throw new NotImplementedException();

        bool IDictionary<string, object>.TryGetValue(string key, out object value) => throw new NotImplementedException();
    }
}
