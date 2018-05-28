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

    public abstract class DynamicNode : WrapperNode, IDynamicMetaObjectProvider, ISimpleDynamicObject
    {
        private readonly Uri baseUri;
        private readonly bool collapseSingularArrays;

        protected DynamicNode(INode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node)
        {
            this.baseUri = baseUri ?? node.Graph?.BaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

        object ISimpleDynamicObject.GetIndex(object[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            return this.GetIndex(indexes[0]) ?? throw new Exception("index not found");
        }

        object ISimpleDynamicObject.GetMember(string name)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException($"Can't get member {name} without baseUri.");
            }

            return this.GetIndex(name) ?? throw new Exception("member not found");
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
            if (this.baseUri == null)
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

            return DynamicHelper.ConvertToNames(predicates, this.baseUri);
        }

        private object GetIndex(object predicate)
        {
            if (this.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.Graph, this.baseUri);

            var triples = this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode);

            var nodeObjects = triples.Select(t => this.ConvertToObject(t.Object));

            if (this.collapseSingularArrays && nodeObjects.Count() == 1)
            {
                return nodeObjects.Single();
            }
            else
            {
                return nodeObjects.ToArray();
            }
        }

        private void SetIndex(object predicate, object value)
        {
            if (this.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var difference = this.CalculateDifference(predicate, value);

            this.Apply(difference);
        }

        private GraphDiffReport CalculateDifference(object predicate, object value)
        {
            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.Graph, this.baseUri);

            var newStatements = this.ConvertToTriples(predicateNode, value);
            var currentStatements = this.Graph.GetTriplesWithSubjectPredicate(this, predicateNode);

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
                    subj: this.node,
                    pred: predicateNode,
                    obj: this.ConvertToNode(item),
                    g: this.node.Graph);
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

        private void Apply(GraphDiffReport difference)
        {
            if (!difference.AreEqual)
            {
                this.Graph.Retract(difference.RemovedMSGs.SelectMany(g => g.Triples));
                this.Graph.Assert(difference.AddedMSGs.SelectMany(g => g.Triples));

                this.Graph.Retract(difference.RemovedTriples);
                this.Graph.Assert(difference.AddedTriples);
            }
        }

        private object ConvertToObject(INode objectNode)
        {
            var valuedNode = objectNode.AsValuedNode();

            switch (valuedNode)
            {
                case IUriNode uriNode:
                    return uriNode.AsDynamic(this.baseUri, this.collapseSingularArrays);

                case IBlankNode blankNode:
                    return blankNode.AsDynamic(this.baseUri, this.collapseSingularArrays);

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
