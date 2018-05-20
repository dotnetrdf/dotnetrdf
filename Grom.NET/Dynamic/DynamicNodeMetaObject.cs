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

    public class DynamicNodeMetaObject : DynamicMetaObject
    {
        private readonly INode node;
        private readonly Uri baseUri;
        private readonly bool collapseSingularArrays;

        public DynamicNodeMetaObject(Expression parameter, INode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(parameter, BindingRestrictions.Empty, node)
        {
            this.node = node;
            this.baseUri = baseUri ?? node.Graph?.BaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var predicate = indexes[0].Value;

            var result = GetIndex(predicate);

            if (result == null)
            {
                return base.BindGetIndex(binder, indexes);
            }

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(result), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0].Value, value.Value);

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(value.Value), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException($"Can't set member {binder.Name} without baseUri.");
            }

            this.SetIndex(binder.Name, value.Value);

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(value.Value), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (this.baseUri == null)
            {
                throw new InvalidOperationException($"Can't get member {binder.Name} without baseUri.");
            }

            var result = this.GetIndex(binder.Name);

            if (result == null)
            {
                return base.BindGetMember(binder);
            }

            return new DynamicMetaObject(Expression.Convert(Expression.Constant(result), binder.ReturnType), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var predicates = this.node.Graph
                .GetTriplesWithSubject(this.node)
                .Select(triple => triple.Predicate as IUriNode)
                .Distinct();

            return DynamicHelper.ConvertToNames(predicates, this.baseUri);
        }

        private object GetIndex(object predicate)
        {
            if (this.node.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.node.Graph, this.baseUri);

            var triples = this.node.Graph.GetTriplesWithSubjectPredicate(this.node, predicateNode);

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

        internal void SetIndex(object predicate, object value)
        {
            if (this.node.Graph == null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            var difference = this.CalculateDifference(predicate, value);

            this.Apply(difference);
        }

        private GraphDiffReport CalculateDifference(object predicate, object value)
        {
            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.node.Graph, this.baseUri);

            var newStatements = this.ConvertToTriples(predicateNode, value);
            var currentStatements = this.node.Graph.GetTriplesWithSubjectPredicate(this.node, predicateNode);

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
                this.node.Graph.Retract(difference.RemovedMSGs.SelectMany(g => g.Triples));
                this.node.Graph.Assert(difference.AddedMSGs.SelectMany(g => g.Triples));

                this.node.Graph.Retract(difference.RemovedTriples);
                this.node.Graph.Assert(difference.AddedTriples);
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
                    return this.node.Graph.CreateUriNode(uriValue);

                case bool boolValue:
                    return new BooleanNode(this.node.Graph, boolValue);

                case byte byteValue:
                    return new ByteNode(this.node.Graph, byteValue);

                case DateTime dateTimeValue:
                    return new DateTimeNode(this.node.Graph, dateTimeValue);

                case DateTimeOffset dateTimeOffsetValue:
                    return new DateTimeNode(this.node.Graph, dateTimeOffsetValue);

                case decimal decimalValue:
                    return new DecimalNode(this.node.Graph, decimalValue);

                case double doubleValue:
                    return new DoubleNode(this.node.Graph, doubleValue);

                case float floatValue:
                    return new FloatNode(this.node.Graph, floatValue);

                case long longValue:
                    return new LongNode(this.node.Graph, longValue);

                case int intValue:
                    return new LongNode(this.node.Graph, intValue);

                case string stringValue:
                    return new StringNode(this.node.Graph, stringValue);

                case char charValue:
                    return new StringNode(this.node.Graph, charValue.ToString());

                case TimeSpan timeSpanValue:
                    return new TimeSpanNode(this.node.Graph, timeSpanValue);

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
                    return new DynamicUriNode(uriNode, this.baseUri, this.collapseSingularArrays);

                case IBlankNode blankNode:
                    return new DynamicBlankNode(blankNode, this.baseUri, this.collapseSingularArrays);

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
