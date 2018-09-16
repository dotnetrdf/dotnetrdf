namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

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
                    throw new ArgumentNullException("Can't work with null index", nameof(key));
                }

                this.Remove(key);

                if (value is null)
                {
                    return;
                }

                this.Add(key, value);
            }
        }

        ICollection<INode> IDictionary<INode, object>.Keys => this.Triples.SubjectNodes.UriNodes().Cast<INode>().ToList().AsReadOnly();

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
                throw new ArgumentException("An item with the same key has already been added.");
            }

            var targetNode = new DynamicNode(key.CopyNode(this._g), this.PredicateBaseUri);

            // TODO: What if value is s a node? Will we get properties for it? Shouldn't.
            foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
            {
                // TODO: Handle key properly
                targetNode[entry.Key.ToString()] = entry.Value;
            }
        }

        public void Add(KeyValuePair<INode, object> item) => this.Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<INode, object> item) => throw new NotImplementedException();

        public bool ContainsKey(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Triples.SubjectNodes.Contains(key);
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex) => throw new System.NotImplementedException();

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator() => throw new System.NotImplementedException();

        public bool Remove(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Retract(this.GetTriplesWithSubject(key).ToArray());
        }

        public bool Remove(KeyValuePair<INode, object> item) => throw new NotImplementedException();

        public bool TryGetValue(INode key, out object value)
        {
            value = this.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(key))
                .Select(node => node.AsDynamic(this.PredicateBaseUri))
                .SingleOrDefault();

            return value != null;
        }
    }
}
