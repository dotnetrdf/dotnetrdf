namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<Uri, object>
    {
        private IDictionary<Uri, object> UriPairs
        {
            get
            {
                return PredicateNodes
                    .ToDictionary(
                        predicate => predicate.Uri,
                        predicate => this[predicate]);
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
                return UriPairs.Keys;
            }
        }

        public void Add(Uri predicate, object objects)
        {
            Add(Convert(predicate), objects);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri predicate, object objects)
        {
            return Contains(Convert(predicate), objects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri key)
        {
            return ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            UriPairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return UriPairs.GetEnumerator();
        }

        public bool Remove(Uri key)
        {
            return Remove(Convert(key));
        }

        public bool Remove(Uri predicate, object @object)
        {
            return Remove(Convert(predicate), @object);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri key, out object value)
        {
            return TryGetValue(Convert(key), out value);
        }

        private INode Convert(Uri key)
        {
            return DynamicHelper.ConvertPredicate(key, this.Graph, this.BaseUri);
        }
    }
}
