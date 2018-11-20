namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph : IDictionary<Uri, object>
    {
        private IDictionary<Uri, object> UriPairs
        {
            get
            {
                return UriNodes
                    .ToDictionary(
                        subject => subject.Uri,
                        subject => this[subject]);
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

        public void Add(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            Add(Convert(subject), predicateAndObjects);
        }

        void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            return Contains(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(Uri subject)
        {
            if (subject is null)
            {
                return false;
            }

            return ContainsKey(Convert(subject));
        }

        public void CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
        {
            UriPairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
        {
            return UriPairs.GetEnumerator();
        }

        public bool Remove(Uri subject)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return Remove(Convert(subject));
        }

        public bool Remove(Uri subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            return Remove(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(Uri subject, out object predicateAndObjects)
        {
            predicateAndObjects = null;

            if (subject is null)
            {
                return false;
            }

            return TryGetValue(Convert(subject), out predicateAndObjects);
        }

        private INode Convert(Uri subject)
        {
            return DynamicHelper.ConvertPredicate(subject, this, this.SubjectBaseUri);
        }
    }
}
