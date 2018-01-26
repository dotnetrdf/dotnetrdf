namespace Grom
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;

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

            var predicateNode = Helper.ConvertNode(predicate, this.graphNode.Graph, this.baseUri);

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
                // TODO: Add configuration option to be IEnumerable<NodeWrapper>?
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
