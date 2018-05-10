namespace Grom
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    public class NodeWrapper : DynamicObject, IEquatable<NodeWrapper>, IComparable<NodeWrapper>
    {
        private readonly INode graphNode;
        private readonly Uri baseUri;
        private readonly bool collapseSingularArrays;

        public NodeWrapper(INode graphNode, Uri baseUri = null, bool collapseSingularArrays = false)
        {
            this.graphNode = graphNode ?? throw new ArgumentNullException(nameof(graphNode));
            this.baseUri = baseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        #region DynamicObject
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0];

            var predicateNode = Helper.ConvertIndex(predicate, this.graphNode.Graph, this.baseUri);

            var propertyTriples = this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, predicateNode);

            var nodes =
                from triple in propertyTriples
                select this.ConvertObject(triple.Object);

            if (this.collapseSingularArrays && nodes.Count() == 1)
            {
                result = nodes.Single();
            }
            else
            {
                result = nodes.ToArray();
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            return this.TryGetIndex(null, new[] { binder.Name }, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var distinctPredicateNodes = this.graphNode.Graph
                .GetTriplesWithSubject(this.graphNode)
                .Select(triple => triple.Predicate as IUriNode)
                .Distinct();

            return Helper.GetDynamicMemberNames(distinctPredicateNodes, this.baseUri);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0];

            var predicateNode = Helper.ConvertIndex(predicate, this.graphNode.Graph, this.baseUri);

            var n = new Graph();
            n.Assert(this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, predicateNode));

            var n2 = new Graph();
            n2.Assert(this.ConvertValues(value).Select(node => new Triple(this.graphNode, predicateNode, node, this.graphNode.Graph)));

            var d = n.Difference(n2);
            if (!d.AreEqual)
            {
                foreach (var x in d.RemovedMSGs)
                {
                    this.graphNode.Graph.Retract(x.Triples);
                }
                foreach (var x in d.AddedMSGs)
                {
                    this.graphNode.Graph.Assert(x.Triples);
                }

                this.graphNode.Graph.Retract(d.RemovedTriples);
                this.graphNode.Graph.Assert(d.AddedTriples);
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            return this.TrySetIndex(null, new[] { binder.Name }, value);
        }
        #endregion

        private object ConvertObject(INode tripleObject)
        {
            var valuedNode = tripleObject.AsValuedNode();

            switch (valuedNode)
            {
                case IUriNode uriNode:
                case IBlankNode blankNode:
                    return new NodeWrapper(valuedNode, this.baseUri, this.collapseSingularArrays);

                case DoubleNode doubleNode:
                    return doubleNode.AsDouble();

                case FloatNode floatNode:
                    return floatNode.AsFloat();

                case DecimalNode decimalNode:
                    return decimalNode.AsDecimal();

                case BooleanNode booleanNode:
                    return booleanNode.AsBoolean();

                case DateTimeNode dateTimeNode:
                    return dateTimeNode.AsDateTimeOffset();

                case TimeSpanNode timeSpanNode:
                    return timeSpanNode.AsTimeSpan();

                case NumericNode numericNode:
                    return numericNode.AsInteger();

                default:
                    return valuedNode.ToString();
            }
        }

        private IEnumerable<INode> ConvertValues(object value)
        {
            if (value == null)
            {
                yield break;
            }

            if (value is string || !(value is IEnumerable enumerableValue)) // Strings are enumerable but not for our case
            {
                enumerableValue = value.AsEnumerable();
            }

            foreach (var item in enumerableValue)
            {
                yield return ConvertValue(item);
            }
        }

        private INode ConvertValue(object value)
        {
            if (value is NodeWrapper wrapperValue)
            {
                value = wrapperValue.graphNode;
            }

            if (value is Uri uriValue)
            {
                value = this.graphNode.Graph.CreateUriNode(uriValue);
            }

            if (this.TryConvertLiteralValue(value, out INode literalNode))
            {
                value = literalNode;
            }

            if (value is INode nodeValue)
            {
                return nodeValue;
            }

            throw new Exception("Unrecognized value type");
        }

        private bool TryConvertLiteralValue(object value, out INode literalNode)
        {
            switch (value)
            {
                case bool boolValue:
                    literalNode = new BooleanNode(this.graphNode.Graph, boolValue);
                    return true;

                case byte byteValue:
                    literalNode = new ByteNode(this.graphNode.Graph, byteValue);
                    return true;

                case DateTime dateTimeValue:
                    literalNode = new DateTimeNode(this.graphNode.Graph, dateTimeValue);
                    return true;

                case DateTimeOffset dateTimeOffsetValue:
                    literalNode = new DateTimeNode(this.graphNode.Graph, dateTimeOffsetValue);
                    return true;

                case decimal decimalValue:
                    literalNode = new DecimalNode(this.graphNode.Graph, decimalValue);
                    return true;

                case double doubleValue:
                    literalNode = new DoubleNode(this.graphNode.Graph, doubleValue);
                    return true;

                case float floatValue:
                    literalNode = new FloatNode(this.graphNode.Graph, floatValue);
                    return true;

                case long longValue:
                    literalNode = new LongNode(this.graphNode.Graph, longValue);
                    return true;

                case int intValue:
                    literalNode = new LongNode(this.graphNode.Graph, intValue);
                    return true;

                case string stringValue:
                    literalNode = new StringNode(this.graphNode.Graph, stringValue);
                    return true;

                case char charValue:
                    literalNode = new StringNode(this.graphNode.Graph, charValue.ToString());
                    return true;

                case TimeSpan timeSpanValue:
                    literalNode = new TimeSpanNode(this.graphNode.Graph, timeSpanValue);
                    return true;

                default:
                    literalNode = null;
                    return false;
            }
        }

        #region Object
        public override bool Equals(object other)
        {
            return this.Equals(other as NodeWrapper);
        }

        public override int GetHashCode()
        {
            return string.Concat(nameof(NodeWrapper), this.ToString()).GetHashCode();
        }

        public override string ToString()
        {
            return this.graphNode.ToString();
        }
        #endregion

        #region Operators
        public static bool operator ==(NodeWrapper a, NodeWrapper b)
        {
            if (a as object == null)
            {
                return b as object == null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(NodeWrapper a, NodeWrapper b)
        {
            return !(a == b);
        }

        public static bool operator >(NodeWrapper a, NodeWrapper b)
        {
            if (a as object == null)
            {
                return false;
            }

            return a.CompareTo(b) > 0;
        }

        public static bool operator <(NodeWrapper a, NodeWrapper b)
        {
            if (a as object == null)
            {
                return b as object != null;
            }

            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(NodeWrapper a, NodeWrapper b)
        {
            return a == b || a > b;
        }

        public static bool operator <=(NodeWrapper a, NodeWrapper b)
        {
            return a == b || a < b;
        }
        #endregion

        #region IEquatable
        public bool Equals(NodeWrapper other)
        {
            if (other as object == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.graphNode.Equals(other.graphNode);
        }
        #endregion

        #region IComparable
        public int CompareTo(NodeWrapper other)
        {
            if (other as object == null)
            {
                return 1;
            }

            if (this.Equals(other))
            {
                return 0;
            }

            return this.graphNode.CompareTo(other.graphNode);
        }
        #endregion
    }
}
