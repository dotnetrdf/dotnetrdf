namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    public class DynamicNode : DynamicObject, IEquatable<DynamicNode>, IComparable<DynamicNode>
    {
        internal readonly INode graphNode;
        private readonly Uri baseUri;
        private readonly bool collapseSingularArrays;

        public DynamicNode(INode graphNode, Uri baseUri = null, bool collapseSingularArrays = false)
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

            var predicateNode = DynamicHelper.ConvertIndex(predicate, this.graphNode.Graph, this.baseUri);

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

            return DynamicHelper.GetDynamicMemberNames(distinctPredicateNodes, this.baseUri);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0];

            var predicateNode = DynamicHelper.ConvertIndex(predicate, this.graphNode.Graph, this.baseUri);

            var n2 = new Graph();
            var a = this.ConvertValues(value);

            try
            {
                n2.Assert(this.ConvertValues(value).Select(node => new Triple(this.graphNode, predicateNode, node, this.graphNode.Graph)));
            }
            catch (Exception e)
            {
                throw new Exception($"Can't convert property {predicate}", e);
            }

            var n = new Graph();
            n.Assert(this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, predicateNode));

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
                throw new InvalidOperationException($"Can't set member {binder.Name} without baseUri.");
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
                    return new DynamicNode(valuedNode, this.baseUri, this.collapseSingularArrays);

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
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var item in enumerableValue)
            {
                yield return ConvertValue(item);
            }
        }

        private INode ConvertValue(object value)
        {
            if (value is DynamicNode wrapperValue)
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

            throw new Exception($"Can't convert type {value.GetType()}");
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
            return this.Equals(other as DynamicNode);
        }

        public override int GetHashCode()
        {
            return string.Concat(nameof(DynamicNode), this.ToString()).GetHashCode();
        }

        public override string ToString()
        {
            return this.graphNode.ToString();
        }
        #endregion

        #region Operators
        public static bool operator ==(DynamicNode a, DynamicNode b)
        {
            if (a as object == null)
            {
                return b as object == null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(DynamicNode a, DynamicNode b)
        {
            return !(a == b);
        }

        public static bool operator >(DynamicNode a, DynamicNode b)
        {
            if (a as object == null)
            {
                return false;
            }

            return a.CompareTo(b) > 0;
        }

        public static bool operator <(DynamicNode a, DynamicNode b)
        {
            if (a as object == null)
            {
                return b as object != null;
            }

            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(DynamicNode a, DynamicNode b)
        {
            return a == b || a > b;
        }

        public static bool operator <=(DynamicNode a, DynamicNode b)
        {
            return a == b || a < b;
        }
        #endregion

        #region IEquatable
        public bool Equals(DynamicNode other)
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
        public int CompareTo(DynamicNode other)
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
