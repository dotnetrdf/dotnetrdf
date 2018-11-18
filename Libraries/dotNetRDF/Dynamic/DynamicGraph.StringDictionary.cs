namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        private IDictionary<string, object> StringPairs
        {
            get
            {
                return UriNodes
                    .ToDictionary(
                        subject => DynamicHelper.ConvertToName(subject, BaseUri),
                        subject => this[subject]);
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
                return StringPairs.Keys;
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

        public bool Contains(string key, object value)
        {
            if (key is null)
            {
                return false;
            }

            if (value is null)
            {
                return false;
            }

            return Contains(Convert(key), value);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key, item.Value);
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
            StringPairs.CopyTo(array, arrayIndex);
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

        public bool Remove(string key, object value)
        {
            return Remove(Convert(key), value);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return TryGetValue(Convert(key), out value);
        }

        private Uri Convert(string key)
        {
            return DynamicHelper.ConvertPredicate(key, this);
        }
    }
}
