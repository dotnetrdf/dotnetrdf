namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        private IDictionary<Uri, object> UriDictionary => this;

        private IEnumerable<KeyValuePair<string, object>> StringPairs
        {
            get
            {
                return
                    from subject in UriSubjectNodes
                    select new KeyValuePair<string, object>(
                        DynamicHelper.ConvertToName(subject, PredicateBaseUri),
                        new DynamicNode(
                            subject,
                            predicateBaseUri));
            }
        }

        public object this[string key]
        {
            get
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return this[Convert(key)];
            }

            set
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                this[Convert(key)] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                var keys =
                    from pair in StringPairs
                    select pair.Key;

                return keys.ToArray();
            }
        }

        public void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Add(Convert(key), value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            if (item.Key is null)
            {
                return false;
            }

            if (item.Value is null)
            {
                return false;
            }

            return UriDictionary.Contains(Convert(item));
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            StringPairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return StringPairs.GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Remove(Convert(key));
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            if (item.Key is null)
            {
                return false;
            }

            return UriDictionary.Remove(Convert(item));
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return TryGetValue(Convert(key), out value);
        }

        private KeyValuePair<Uri, object> Convert(KeyValuePair<string, object> item)
        {
            return new KeyValuePair<Uri, object>(Convert(item.Key), item.Value);
        }

        private Uri Convert(string key)
        {
            return DynamicHelper.ConvertPredicate(key, this);
        }
    }
}
