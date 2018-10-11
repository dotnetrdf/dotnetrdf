namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        public object this[Uri key]
        {
            get => this[DynamicHelper.Convert(key, this, this.SubjectBaseUri)];
            set => this[DynamicHelper.Convert(key, this, this.SubjectBaseUri)] = value;
        }

        ICollection<Uri> IDictionary<Uri, object>.Keys => this.Triples.SubjectNodes.UriNodes().Select(n => n.Uri).ToArray();

        public void Add(Uri key, object value)
        {
            this.Add(DynamicHelper.Convert(key, this, this.SubjectBaseUri), value);
        }

        public void Add(KeyValuePair<Uri, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<Uri, object> item)
        {
            return ((IDictionary<INode, object>)this).Contains(new KeyValuePair<INode, object>(DynamicHelper.Convert(item.Key, this, this.SubjectBaseUri), item.Value));
        }

        public bool ContainsKey(Uri key)
        {
            return this.ContainsKey(DynamicHelper.Convert(key, this, this.SubjectBaseUri));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(Uri key)
        {
            return this.Remove(DynamicHelper.Convert(key, this, this.SubjectBaseUri));
        }

        public bool Remove(KeyValuePair<Uri, object> item)
        {
            return ((IDictionary<INode, object>)this).Remove(new KeyValuePair<INode, object>(DynamicHelper.Convert(item.Key, this, this.SubjectBaseUri), item.Value));
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return this.TryGetValue(DynamicHelper.Convert(key, this, this.SubjectBaseUri), out value);
        }
    }
}
