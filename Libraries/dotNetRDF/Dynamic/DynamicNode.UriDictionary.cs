namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<Uri, object>
    {
        public object this[Uri key]
        {
            get => this[DynamicHelper.Convert(key, this.Graph, this.BaseUri)];
            set => this[DynamicHelper.Convert(key, this.Graph, this.BaseUri)] = value;
        }

        ICollection<Uri> IDictionary<Uri, object>.Keys => this.Graph.GetTriplesWithSubject(this).Select(t => (t.Predicate as IUriNode).Uri).Distinct().ToArray();

        public void Add(Uri key, object value)
        {
            this.Add(DynamicHelper.Convert(key, this.Graph, this.BaseUri), value);
        }

        public void Add(KeyValuePair<Uri, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(Uri key, object value)
        {
            return this.Contains(DynamicHelper.Convert(key, this.Graph, this.BaseUri), value);
        }

        public bool Contains(KeyValuePair<Uri, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri key)
        {
            return this.ContainsKey(DynamicHelper.Convert(key, this.Graph, this.BaseUri));
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
            return this.Remove(DynamicHelper.Convert(key, this.Graph, this.BaseUri));
        }

        public bool Remove(Uri key, object value)
        {
            return this.Remove(DynamicHelper.Convert(key, this.Graph, this.BaseUri), value);
        }

        public bool Remove(KeyValuePair<Uri, object> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return this.TryGetValue(DynamicHelper.Convert(key, this.Graph, this.BaseUri), out value);
        }
    }
}
