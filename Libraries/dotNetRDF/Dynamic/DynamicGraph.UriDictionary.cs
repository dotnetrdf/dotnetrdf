namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        private IEnumerable<KeyValuePair<Uri, object>> UriPairs
        {
            get
            {
                return UriNodes
                    .ToDictionary(
                        subject => subject.Uri,
                        subject => this[subject]);
            }
        }

        public object this[Uri key]
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

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri subject, object predicate)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return Contains(Convert(subject), predicate);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Remove(Convert(key));
        }

        public bool Remove(Uri subject, object predicate)
        {
            if (subject is null)
            {
                return false;
            }

            return Remove(Convert(subject), predicate);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return TryGetValue(Convert(key), out value);
        }

        private INode Convert(Uri key)
        {
            return DynamicHelper.ConvertPredicate(key, this, this.SubjectBaseUri);
        }
    }
}
