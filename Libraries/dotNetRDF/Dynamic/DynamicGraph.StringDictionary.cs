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

        public object this[string subject]
        {
            get
            {
                if (subject is null)
                {
                    throw new ArgumentNullException(nameof(subject));
                }

                return this[Convert(subject)];
            }

            set
            {
                if (subject is null)
                {
                    throw new ArgumentNullException(nameof(subject));
                }

                this[Convert(subject)] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return StringPairs.Keys;
            }
        }

        public void Add(string subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicateAndObjects == null)
            {
                throw new ArgumentNullException(nameof(predicateAndObjects));
            }

            Add(Convert(subject), predicateAndObjects);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(string subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            if (predicateAndObjects is null)
            {
                return false;
            }

            return Contains(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool ContainsKey(string subject)
        {
            if (subject is null)
            {
                return false;
            }

            return ContainsKey(Convert(subject));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            StringPairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return StringPairs.GetEnumerator();
        }

        public bool Remove(string subject)
        {
            if (subject is null)
            {
                return false;
            }

            return Remove(Convert(subject));
        }

        public bool Remove(string subject, object predicateAndObjects)
        {
            if (subject is null)
            {
                return false;
            }

            return Remove(Convert(subject), predicateAndObjects);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(string subject, out object predicateAndObjects)
        {
            predicateAndObjects = null;

            if (subject is null)
            {
                return false;
            }

            return TryGetValue(Convert(subject), out predicateAndObjects);
        }

        private Uri Convert(string subject)
        {
            return DynamicHelper.ConvertPredicate(subject, this);
        }
    }
}
