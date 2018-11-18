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
                var predicates =
                    from t in Graph.GetTriplesWithSubject(this)
                    select t.Predicate as IUriNode;

                return predicates.Distinct();
            }
        }

        private IDictionary<INode, object> NodePairs
        {
            get
            {
                return PredicateNodes
                    .ToDictionary(
                        predicate => predicate as INode,
                        predicate => this[predicate]);
            }
        }

        public object this[INode predicate]
        {
            get
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                return new DynamicObjectCollection(this, predicate);
            }

            set
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                Remove(predicate);

                if (!(value is null))
                {
                    Add(predicate, value);
                }
            }
        }

        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return PredicateNodes.ToArray();
            }
        }

        public void Add(INode predicate, object objects)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            Graph.Assert(ConvertToTriples(predicate, objects));
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(INode predicate, object @object)
        {
            if (predicate is null)
            {
                return false;
            }

            if (@object is null)
            {
                return false;
            }

            if (!TryGetValue(predicate, out var objects))
            {
                return false;
            }

            return ((DynamicObjectCollection)objects).Contains(@object);
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            if (item.Key is null)
            {
                return false;
            }

            if (item.Value is null)
            {
                return false;
            }

            if (!TryGetValue(item.Key, out var objects))
            {
                return false;
            }

            return ((DynamicObjectCollection)objects).Contains(item.Value);
        }

        public bool ContainsKey(INode predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return PredicateNodes.Contains(predicate);
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            NodePairs.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return NodePairs.GetEnumerator();
        }

        public bool Remove(INode predicate)
        {
            if (predicate is null)
            {
                return false;
            }

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, predicate).ToArray());
        }

        public bool Remove(INode predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            if (objects is null)
            {
                return false;
            }

            return Graph.Retract(ConvertToTriples(predicate, objects));
        }

        bool ICollection<KeyValuePair<INode, object>>.Remove(KeyValuePair<INode, object> item)
        {
            return Remove(item.Key, item.Value);
        }

        public bool TryGetValue(INode predicate, out object objects)
        {
            objects = null;

            if (predicate is null || !ContainsKey(predicate))
            {
                return false;
            }

            objects = this[predicate];
            return true;
        }

        private IEnumerable<Triple> ConvertToTriples(INode predicate, object value)
        {
            // Strings are enumerable but not for our case
            // RDF collections are also enumerable but have special treatment
            if (value is IRdfCollection || value is string || !(value is IEnumerable enumerable))
            {
                enumerable = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var @object in enumerable)
            {
                // TODO: Maybe this should throw on null
                if (!(@object is null))
                {
                    yield return new Triple(
                        this,
                        predicate,
                        DynamicHelper.ConvertObject(@object, Graph));
                }
            }
        }
    }
}
