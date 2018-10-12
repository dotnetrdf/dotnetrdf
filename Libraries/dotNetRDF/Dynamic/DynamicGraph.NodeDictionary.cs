namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public partial class DynamicGraph : IDictionary<INode, object>
    {
        public object this[INode key]
        {
            get
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (!this.TryGetValue(key, out var node))
                {
                    throw new KeyNotFoundException();
                }

                return node;
            }
            set
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                this.Remove(key);

                if (value is null)
                {
                    return;
                }

                this.Add(key, value);
            }
        }

        ICollection<INode> IDictionary<INode, object>.Keys => this.Triples.SubjectNodes.UriNodes().ToArray();

        public void Add(INode key, object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (this.ContainsKey(key))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            var targetNode = new DynamicNode(key.CopyNode(this._g), this.PredicateBaseUri);

            foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
            {
                targetNode[entry.Key] = entry.Value;
            }
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            if (item.Key is null)
            {
                // All statements have subject
                return false;
            }

            if (item.Value is null)
            {
                // All statements have object
                return false;
            }

            if (!this.TryGetValue(item.Key, out var value))
            {
                return false;
            }

            var node = (DynamicNode)value;

            foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(item.Value))
            {
                if (!node.Contains(entry.Key, entry.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsKey(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Triples.SubjectNodes.Contains(key);
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Retract(this.GetTriplesWithSubject(key).ToArray());
        }

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(INode key, out object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // TODO: CopyNode
            value = this.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(key))
                .Select(node => node.AsDynamic(this.PredicateBaseUri))
                .SingleOrDefault();

            return value != null;
        }

        private static IDictionary ConvertToDictionary(object value)
        {
            if (value is IDictionary valueDictionary)
            {
                return valueDictionary;
            }

            return DynamicGraph.GetProperties(value).ToDictionary(p => p.Name, p => p.GetValue(value, null));
        }

        private static IEnumerable<PropertyInfo> GetProperties(object value)
        {
            return value
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any());
        }
    }
}
