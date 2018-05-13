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

        #region Constructors
        public DynamicNode(INode graphNode) : this(graphNode, null) { }

        public DynamicNode(INode graphNode, Uri baseUri) : this(graphNode, baseUri, false) { }

        public DynamicNode(INode graphNode, bool collapseSingularArrays) : this(graphNode, null, collapseSingularArrays) { }

        public DynamicNode(INode graphNode, Uri baseUri, bool collapseSingularArrays)
        {
            this.graphNode = graphNode ?? throw new ArgumentNullException(nameof(graphNode));
            this.baseUri = baseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }
        #endregion

        #region DynamicObject
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0];

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.graphNode.Graph, this.baseUri);

            var triples = this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, predicateNode);

            var nodeObjects = triples.Select(t => this.ConvertToObject(t.Object));

            if (this.collapseSingularArrays && nodeObjects.Count() == 1)
            {
                result = nodeObjects.Single();
            }
            else
            {
                result = nodeObjects.ToArray();
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException($"Can't get member {binder.Name} without baseUri.");
            }

            return this.TryGetIndex(null, new[] { binder.Name }, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0];

            var difference = this.CalculateDifference(predicate, value);

            this.Apply(difference);

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

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var predicates = this.graphNode.Graph
                .GetTriplesWithSubject(this.graphNode)
                .Select(triple => triple.Predicate as IUriNode)
                .Distinct();

            return DynamicHelper.ConvertToNames(predicates, this.baseUri);
        }
        #endregion

        #region Internals
        private GraphDiffReport CalculateDifference(object predicate, object value)
        {
            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.graphNode.Graph, this.baseUri);

            var newStatements = this.ConvertToTriples(predicateNode, value);
            var currentStatements = this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, predicateNode);

            using (var newState = new Graph())
            {
                newState.Assert(newStatements);

                using (var currentState = new Graph())
                {
                    currentState.Assert(currentStatements);

                    return currentState.Difference(newState);
                }
            }
        }

        private void Apply(GraphDiffReport difference)
        {
            if (!difference.AreEqual)
            {
                this.graphNode.Graph.Retract(difference.RemovedMSGs.SelectMany(g => g.Triples));
                this.graphNode.Graph.Assert(difference.AddedMSGs.SelectMany(g => g.Triples));

                this.graphNode.Graph.Retract(difference.RemovedTriples);
                this.graphNode.Graph.Assert(difference.AddedTriples);
            }
        }

        private object ConvertToObject(INode objectNode)
        {
            var valuedNode = objectNode.AsValuedNode();

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
                yield return new Triple(
                    subj: this.graphNode,
                    pred: predicateNode,
                    obj: this.ConvertToNode(item),
                    g: this.graphNode.Graph);
            }
        }

        private INode ConvertToNode(object value)
        {
            switch (value)
            {
                case DynamicNode dynamicValue:
                    return dynamicValue.graphNode;

                case INode nodeValue:
                    return nodeValue;

                case Uri uriValue:
                    return this.graphNode.Graph.CreateUriNode(uriValue);

                case bool boolValue:
                    return new BooleanNode(this.graphNode.Graph, boolValue);

                case byte byteValue:
                    return new ByteNode(this.graphNode.Graph, byteValue);

                case DateTime dateTimeValue:
                    return new DateTimeNode(this.graphNode.Graph, dateTimeValue);

                case DateTimeOffset dateTimeOffsetValue:
                    return new DateTimeNode(this.graphNode.Graph, dateTimeOffsetValue);

                case decimal decimalValue:
                    return new DecimalNode(this.graphNode.Graph, decimalValue);

                case double doubleValue:
                    return new DoubleNode(this.graphNode.Graph, doubleValue);

                case float floatValue:
                    return new FloatNode(this.graphNode.Graph, floatValue);

                case long longValue:
                    return new LongNode(this.graphNode.Graph, longValue);

                case int intValue:
                    return new LongNode(this.graphNode.Graph, intValue);

                case string stringValue:
                    return new StringNode(this.graphNode.Graph, stringValue);

                case char charValue:
                    return new StringNode(this.graphNode.Graph, charValue.ToString());

                case TimeSpan timeSpanValue:
                    return new TimeSpanNode(this.graphNode.Graph, timeSpanValue);

                default:
                    throw new Exception($"Can't convert type {value.GetType()}");
            }
        }
        #endregion

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
