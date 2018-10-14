namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        private IDictionary<Uri, object> UriDictionary => this;

        public object this[string key]
        {
            get
            {
                return this[this.Convert(key)];
            }

            set
            {
                this[this.Convert(key)] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                var keys =
                    from node in UriSubjectNodes
                    select DynamicHelper.ConvertToName(
                        node,
                        this.SubjectBaseUri);

                return keys.ToArray();
            }
        }

        public void Add(string key, object value)
        {
            Add(Convert(key), value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return UriDictionary.Contains(Convert(item));
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(Convert(key));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            return Remove(Convert(key));
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return UriDictionary.Remove(Convert(item));
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValue(Convert(key), out value);
        }

        private KeyValuePair<Uri, object> Convert(KeyValuePair<string, object> item)
        {
            return new KeyValuePair<Uri, object>(Convert(item.Key), item.Value);
        }

        private Uri Convert(string key)
        {
            return DynamicHelper.Convert(key, this);
        }
    }
}
