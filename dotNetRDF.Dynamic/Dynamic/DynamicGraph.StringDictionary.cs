namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    public partial class DynamicGraph : IDictionary<string, object>
    {
        public object this[string subject]
        {
            get
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                return this.GetDynamicNode(subject) ?? throw new Exception("index not found");
            }
            set
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                var targetNode = this.GetDynamicNodeOrCreate(subject);

                if (value == null)
                {
                    targetNode.Clear();
                }
                else
                {
                    foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
                    {
                        // TODO: What if value is s a node? Will we get properties for it? Shouldn't.
                        targetNode[entry.Key.ToString()] = entry.Value;
                    }
                }
            }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return this.Triples.SubjectNodes.UriNodes().Select(u => DynamicHelper.ConvertToName(u , this.SubjectBaseUri)).ToArray();
            }
        }

        ICollection<object> IDictionary<string, object>.Values => throw new NotImplementedException();

        int ICollection<KeyValuePair<string, object>>.Count => throw new NotImplementedException();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => throw new NotImplementedException();

        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
