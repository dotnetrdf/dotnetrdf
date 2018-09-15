namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        public object this[Uri key]
        {
            get => this[DynamicHelper.Convert(key, this, this.SubjectBaseUri)];
            set => this[DynamicHelper.Convert(key, this, this.SubjectBaseUri)] = value;
        }

        ICollection<Uri> IDictionary<Uri, object>.Keys => this.Triples.SubjectNodes.UriNodes().Select(n => n.Uri).ToArray();

        void IDictionary<Uri, object>.Add(Uri key, object value) => throw new NotImplementedException();

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item) => throw new NotImplementedException();

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item) => throw new NotImplementedException();

        public bool ContainsKey(Uri key) => this.ContainsKey(DynamicHelper.Convert(key, this, this.SubjectBaseUri));

        void ICollection<KeyValuePair<Uri, object>>.CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex) => throw new NotImplementedException();

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator() => throw new NotImplementedException();

        public bool Remove(Uri key) => this.Remove(DynamicHelper.Convert(key, this, this.SubjectBaseUri));

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item) => throw new NotImplementedException();

        bool IDictionary<Uri, object>.TryGetValue(Uri key, out object value) => throw new NotImplementedException();
    }
}
