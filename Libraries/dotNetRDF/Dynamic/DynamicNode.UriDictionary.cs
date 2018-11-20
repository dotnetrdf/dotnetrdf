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

        public object this[Uri predicate]
        {
            get
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                return this[Convert(predicate)];
            }

            set
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                this[Convert(predicate)] = value;
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
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Add(Convert(predicate), objects);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            return Contains(Convert(predicate), objects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return ContainsKey(Convert(predicate));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            UriPairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return UriPairs.GetEnumerator();
        }

        public bool Remove(Uri predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return Remove(Convert(predicate));
        }

        public bool Remove(Uri predicate, object @object)
        {
            if (predicate is null)
            {
                return false;
            }

            return Remove(Convert(predicate), @object);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri predicate, out object objects)
        {
            objects = null;

            if (predicate is null)
            {
                return false;
            }

            return TryGetValue(Convert(predicate), out objects);
        }

        private INode Convert(Uri key)
        {
            return DynamicHelper.ConvertPredicate(key, this.Graph, this.BaseUri);
        }
    }
}
