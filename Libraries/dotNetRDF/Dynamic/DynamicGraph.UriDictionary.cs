namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        private IDictionary<INode, object> NodeDictionary => this;

        private IEnumerable<KeyValuePair<Uri, object>> UriPairs
        {
            get
            {
                return
                    from key in UriSubjectNodes
                    select new KeyValuePair<Uri, object>(
                        key.Uri,
                        new DynamicNode(
                            key,
                            predicateBaseUri));
            }
        }

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
                var keys =
                    from pair in UriPairs
                    select pair.Key;

                return keys.ToArray();
            }
        }

        public void Add(Uri key, object value)
        {
            Add(Convert(key), value);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return NodeDictionary.Contains(Convert(item));
        }

        public bool ContainsKey(Uri key)
        {
            return ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            UriPairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return UriPairs.GetEnumerator();
        }

        public bool Remove(Uri key)
        {
            return Remove(Convert(key));
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return NodeDictionary.Remove(Convert(item));
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return TryGetValue(Convert(key), out value);
        }

        private KeyValuePair<INode, object> Convert(KeyValuePair<Uri, object> item)
        {
            return new KeyValuePair<INode, object>(Convert(item.Key), item.Value);
        }

        private INode Convert(Uri key)
        {
            return DynamicHelper.Convert(key, this, this.SubjectBaseUri);
        }
    }
}
