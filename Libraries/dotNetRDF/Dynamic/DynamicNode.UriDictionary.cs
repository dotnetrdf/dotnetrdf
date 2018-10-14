namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<Uri, object>
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
                return Graph.GetTriplesWithSubject(this).Select(t => (t.Predicate as IUriNode).Uri).Distinct().ToArray();
            }
        }

        public void Add(Uri key, object value)
        {
            Add(Convert(key), value);
        }

        public void Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri key, object value)
        {
            return Contains(Convert(key), value);
        }

        public bool Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri key)
        {
            return ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<Uri, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToDictionary(p => (p as IUriNode).Uri, p => this[p]).GetEnumerator();
        }

        public bool Remove(Uri key)
        {
            return Remove(Convert(key));
        }

        public bool Remove(Uri key, object value)
        {
            return Remove(Convert(key), value);
        }

        public bool Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return TryGetValue(Convert(key), out value);
        }

        private INode Convert(Uri key)
        {
            return DynamicHelper.Convert(key, this.Graph, this.BaseUri);
        }
    }
}
