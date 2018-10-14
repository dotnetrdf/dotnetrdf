namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<Uri, object>
    {
        private IDictionary<INode, object> NodeDictionary => this;

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
                    from predicate in PredicateNodes
                    select predicate.Uri;

                return keys.ToArray();
            }
        }

        public void Add(Uri key, object value)
        {
            Add(Convert(key), value);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            NodeDictionary.Add(Convert(item));
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
            (this as IEnumerable<KeyValuePair<Uri, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return
                PredicateNodes
                .ToDictionary(
                    p => p.Uri,
                    p => this[p])
                .GetEnumerator();
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
            return DynamicHelper.Convert(key, this.Graph, this.BaseUri);
        }
    }
}
