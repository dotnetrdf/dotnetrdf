namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode : IDictionary<INode, object>
    {
        private IEnumerable<IUriNode> PredicateNodes
        {
            get
            {
                return Graph
                    .GetTriplesWithSubject(this)
                    .Select(t => t.Predicate as IUriNode)
                    .Distinct();
            }
        }

        private IDictionary<INode, object> Pairs
        {
            get
            {
                return PredicateNodes.ToDictionary(
                    predicate => predicate as INode,
                    predicate => this[predicate]);
            }
        }

        public object this[INode key]
        {
            get
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (!TryGetValue(key, out var objects))
                {
                    throw new KeyNotFoundException();
                }

                return objects;
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

        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return PredicateNodes.ToArray();
            }
        }

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

            if (ContainsKey(key))
            {
                throw new ArgumentException("An item with the same key has already been added.", nameof(key));
            }

            Graph.Assert(this.ConvertToTriples(key, value));
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
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

            if (!TryGetValue(item.Key, out var value))
            {
                return false;
            }

            var objects = (DynamicObjectCollection)value;

            return objects.Contains(item.Value);
        }

        public bool ContainsKey(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Graph.GetTriplesWithSubjectPredicate(this, key).Any();
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            Pairs.ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return Pairs.GetEnumerator();
        }

        public bool Remove(INode key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, key).ToArray());
        }

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            if (item.Key is null)
            {
                throw new ArgumentNullException("key");
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, item.Key).WithObject(DynamicHelper.ConvertObject(item.Value, Graph)).ToArray());
        }

        public bool TryGetValue(INode key, out object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var statementExists = Graph.GetTriplesWithSubjectPredicate(this, key).Any();

            if (!statementExists)
            {
                value = null;
            }
            else
            {
                value = new DynamicObjectCollection(this, key);
            }

            return !(value is null);
        }

        private IEnumerable<Triple> ConvertToTriples(INode predicateNode, object value)
        {
            // Strings are enumerable but not for our case
            // RDF collections are also enumerable but have special treatment
            if (value is IRdfCollection || value is string || !(value is IEnumerable enumerableValue))
            {
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var item in enumerableValue)
            {
                // TODO: Maybe this should throw on null
                if (!(item is null))
                {
                    yield return new Triple(
                        subj: Node,
                        pred: predicateNode,
                        obj: DynamicHelper.ConvertObject(item, Graph),
                        g: Node.Graph);
                }
            }
        }
    }
}
