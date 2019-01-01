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
                return PredicateNodes.Cast<INode>().ToList();
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

            if (objects is IRdfCollection collection)
            {
                var list = new List<object>();
                foreach (var item in collection)
                {
                    list.Add(item);
                }

                var root = Graph.AssertList(list, x => DynamicHelper.ConvertObject(x, Graph));
                Graph.Assert(this, predicate, root);
            }
            else
            {
                Graph.Assert(ConvertToTriples(predicate, objects));
            }
        }

        void ICollection<KeyValuePair<INode, object>>.Add(KeyValuePair<INode, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(INode predicate, object objects)
        {
            if (predicate is null)
            {
                return false;
            }

            if (objects is null)
            {
                return false;
            }

            if (objects is IRdfCollection collection)
            {
                return FindListRoots(collection).Any();
            }
            else
            {
                var g = new Graph();
                g.Assert(ConvertToTriples(predicate.CopyNode(g), objects));
                return Graph.HasSubGraph(g);
            }
        }

        bool ICollection<KeyValuePair<INode, object>>.Contains(KeyValuePair<INode, object> item)
        {
            return Contains(item.Key, item.Value);
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

            return Graph.Retract(Graph.GetTriplesWithSubjectPredicate(this, predicate).ToList());
        }

        // TODO: Fails because wrong conversion of RDF collection objects
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

            if (objects is IRdfCollection collection)
            {
                var collectionTriples = Graph.GetTriplesWithSubjectPredicate(this, predicate).Where(t => t.Object.IsListRoot(Graph) && SameList(t.Object, collection));
                foreach (var collectionTriple in collectionTriples.ToList())
                {
                    Graph.RetractList(collectionTriple.Object);
                    Graph.Retract(collectionTriple);
                }

                return collectionTriples.Any();
            }
            else
            {
                return Graph.Retract(ConvertToTriples(predicate, objects));
            }
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
            if (value is string || value is IRdfCollection || !(value is IEnumerable enumerable))
            {
                enumerable = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var @object in enumerable)
            {
                // TODO: Maybe this should throw on null
                if (!(@object is null))
                {
                    // TODO: This is a mess
                    yield return new Triple(
                        this.CopyNode(predicate.Graph),
                        predicate,
                        DynamicHelper.ConvertObject(@object, predicate.Graph));
                }
            }
        }





        public IEnumerable<INode> FindListRoots(IRdfCollection items)
        {
            return ListRoots().Where(n => SameList(n, items));
        }

        public IEnumerable<INode> ListRoots()
        {
            return Graph.Triples.SubjectNodes.BlankNodes().Where(n => n.IsListRoot(Graph));
        }

        private bool SameList(INode root, IRdfCollection items)
        {
            if (root.Graph is null)
            {
                throw new InvalidOperationException("Root node must have Graph");
            }

            var originalList = root.Graph.GetListItems(root).GetEnumerator();
            var list = items.GetEnumerator();

            while (true)
            {
                var originalListMoved = originalList.MoveNext();
                var listMoved = list.MoveNext();

                // different list lengths
                if (originalListMoved != listMoved)
                {
                    return false;
                }

                // both finished
                if (!originalListMoved)
                {
                    return true;
                }

                // items differ
                if (!originalList.Current.Equals(DynamicHelper.ConvertObject(list.Current, root.Graph)))
                {
                    return false;
                }
            }
        }

    }
}
