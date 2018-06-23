namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    partial class DynamicNode : IDictionary<object, object>
    {
        public object this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (this.Graph == null)
                {
                    throw new InvalidOperationException("Node must have graph");
                }

                return new DynamicObjectCollection(this, DynamicHelper.ConvertToNode(key, this.Graph, this.BaseUri));
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                this.Remove(key);
                this.Add(key, value);
            }
        }

        public ICollection<object> Keys
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToArray();
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Object).Distinct().Select(o => DynamicHelper.ConvertToObject(o, this.BaseUri)).ToArray();
            }
        }

        public int Count
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(object key, object value)
        {
            var predicateNode = DynamicHelper.ConvertToNode(key, this.Graph, this.BaseUri);

            this.Graph.Assert(this.ConvertToTriples(predicateNode, value));
        }

        public void Add(KeyValuePair<object, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.Graph.Retract(this.Graph.GetTriplesWithSubject(this).ToArray());
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(object key)
        {
            var predicateNode = DynamicHelper.ConvertToNode(key, this.Graph, this.BaseUri);

            return this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode).Any();
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            this.ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToDictionary(p => p as object, p => this[p]).GetEnumerator();
        }

        public bool Remove(object key)
        {
            var predicateNode = DynamicHelper.ConvertToNode(key, this.Graph, this.BaseUri);

            return this.Graph.Retract(this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode).ToArray());
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            var predicateNode = DynamicHelper.ConvertToNode(item.Key, this.Graph, this.BaseUri);
            var objectNode = this.ConvertToNode(item.Value);

            return this.Graph.Retract(this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode).WithObject(objectNode).ToArray());
        }

        public bool TryGetValue(object key, out object value)
        {
            var objects = this[key] as IEnumerable<object>;

            if (objects.Any())
            {
                value = objects;
                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerable<Triple> ConvertToTriples(INode predicateNode, object value)
        {
            if (value == null)
            {
                yield break;
            }

            if (value is string || !(value is IEnumerable enumerableValue)) // Strings are enumerable but not for our case
            {
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var item in enumerableValue)
            {
                // TODO: Maybe this should throw on null
                if (item != null)
                {
                    yield return new Triple(
                        subj: this.Node,
                        pred: predicateNode,
                        obj: this.ConvertToNode(item),
                        g: this.Node.Graph);
                }
            }
        }

        public bool Contains(object key, object value)
        {
            var predicateNode = DynamicHelper.ConvertToNode(key, this.Graph, this.BaseUri);
            var objectNode = this.ConvertToNode(value);

            return this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode).WithObject(objectNode).Any();
        }
    }
}
