namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    public class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider, ISimpleDynamicObject
    {
        private readonly Uri baseUri;
        private readonly bool collapseSingularArrays;

        public Uri Uri => this.node is IUriNode buriNode ? buriNode.Uri : throw new InvalidOperationException("is not a uri node");

        public string InternalID => this.node is IBlankNode blankNode ? blankNode.InternalID : throw new InvalidOperationException("is not a blank node");

        private Uri BaseUri
        {
            get
            {
                return this.baseUri ?? this.Graph?.BaseUri;
            }
        }

        public DynamicNode(INode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node)
        {
            this.baseUri = baseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

        object ISimpleDynamicObject.GetIndex(object[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            return this.GetIndex(indexes[0]);
        }

        object ISimpleDynamicObject.GetMember(string name)
        {

            return this.GetIndex(name);
        }

        object ISimpleDynamicObject.SetIndex(object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0], value);

            return value;
        }

        object ISimpleDynamicObject.SetMember(string name, object value)
        {
            if (this.BaseUri == null)
            {
                throw new InvalidOperationException($"Can't set member {name} without baseUri.");
            }

            this.SetIndex(name, value);

            return value;

        }

        IEnumerable<string> ISimpleDynamicObject.GetDynamicMemberNames()
        {
            var predicates = this.Graph
                .GetTriplesWithSubject(this)
                .Select(triple => triple.Predicate as IUriNode)
                .Distinct();

            return DynamicHelper.ConvertToNames(predicates, this.BaseUri);
        }

        private object GetIndex(object predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (this.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.Graph, this.BaseUri);

            var triples = this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode);

            var nodeObjects = triples.Select(t => this.ConvertToObject(t.Object));

            if (this.collapseSingularArrays && nodeObjects.Count() == 1)
            {
                return nodeObjects.Single();
            }
            else
            {
                return new DynamicObjectCollection(this, predicateNode, nodeObjects);
            }
        }

        private void SetIndex(object predicate, object value)
        {
            if (this.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.Graph, this.BaseUri);
            this.Graph.Retract(this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode).ToArray());
            var newStatements = this.ConvertToTriples(predicateNode, value);
            this.Graph.Assert(newStatements);
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
                if (item != null)
                {
                    yield return new Triple(
                        subj: this.node,
                        pred: predicateNode,
                        obj: this.ConvertToNode(item),
                        g: this.node.Graph);
                }
            }
        }

        private INode ConvertToNode(object value)
        {
            switch (value)
            {
                case INode nodeValue:
                    return nodeValue;

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
                    throw new Exception($"Can't convert type {value.GetType()}");
            }
        }

        private object ConvertToObject(INode objectNode)
        {
            var valuedNode = objectNode.AsValuedNode();

            switch (valuedNode)
            {
                case IUriNode uriNode:
                    return uriNode.AsDynamic(this.BaseUri, this.collapseSingularArrays);

                case IBlankNode blankNode:
                    return blankNode.AsDynamic(this.BaseUri, this.collapseSingularArrays);

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
    }
}
