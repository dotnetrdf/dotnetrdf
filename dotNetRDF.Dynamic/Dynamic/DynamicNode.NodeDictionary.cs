namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    partial class DynamicNode : IDictionary<INode, object>
    {
        public object this[INode key]
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

                return new DynamicObjectCollection(this, key);
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

        ICollection<INode> IDictionary<INode, object>.Keys
        {
            get
            {
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToArray();
            }
        }

        public void Add(INode key, object value)
        {
            this.Graph.Assert(this.ConvertToTriples(key, value));
        }

        public void Add(KeyValuePair<INode, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(INode key, object value)
        {
            var objectNode = this.ConvertToNode(value);

            return this.Graph.GetTriplesWithSubjectPredicate(this, key).WithObject(objectNode).Any();
        }

        public bool Contains(KeyValuePair<INode, object> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        public bool ContainsKey(INode key)
        {
            return this.Graph.GetTriplesWithSubjectPredicate(this, key).Any();
        }

        public void CopyTo(KeyValuePair<INode, object>[] array, int arrayIndex)
        {
            (this as IEnumerable<KeyValuePair<INode, object>>).ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<INode, object>> IEnumerable<KeyValuePair<INode, object>>.GetEnumerator()
        {
            return this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().ToDictionary(p => p, p => this[p]).GetEnumerator();
        }

        public bool Remove(INode key)
        {
            return this.Graph.Retract(this.Graph.GetTriplesWithSubjectPredicate(this, key).ToArray());
        }

        public bool Remove(INode key, object value)
        {
            var objectNode = this.ConvertToNode(value);

            return this.Graph.Retract(this.Graph.GetTriplesWithSubjectPredicate(this, key).WithObject(objectNode).ToArray());
        }

        public bool Remove(KeyValuePair<INode, object> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        public bool TryGetValue(INode key, out object value)
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

        private INode ConvertToNode(object value)
        {
            switch (value)
            {
                case INode nodeValue:
                    return nodeValue.CopyNode(this.Graph);

                case Uri uriValue:
                    return this.Graph.CreateUriNode(uriValue);

                case bool boolValue:
                    return new BooleanNode(this.Graph, boolValue);

                case byte byteValue:
                    return new ByteNode(this.Graph, byteValue);

                case DateTime dateTimeValue:
                    return new DateTimeNode(this.Graph, dateTimeValue);

                case DateTimeOffset dateTimeOffsetValue:
                    return new DateTimeNode(this.Graph, dateTimeOffsetValue);

                case decimal decimalValue:
                    return new DecimalNode(this.Graph, decimalValue);

                case double doubleValue:
                    return new DoubleNode(this.Graph, doubleValue);

                case float floatValue:
                    return new FloatNode(this.Graph, floatValue);

                case long longValue:
                    return new LongNode(this.Graph, longValue);

                case int intValue:
                    return new LongNode(this.Graph, intValue);

                case string stringValue:
                    return new StringNode(this.Graph, stringValue);

                case char charValue:
                    return new StringNode(this.Graph, charValue.ToString());

                case TimeSpan timeSpanValue:
                    return new TimeSpanNode(this.Graph, timeSpanValue);

                default:
                    throw new InvalidOperationException($"Can't convert type {value.GetType()}");
            }
        }
    }
}
