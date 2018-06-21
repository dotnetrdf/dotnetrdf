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

    public class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider, IDynamicObject
    {
        private readonly Uri baseUri;

        //public object this[object index]
        //{
        //    get
        //    {

        //        /* return the specified index here */
        //    }
        //    set
        //    {
        //        /* set the specified index to value here */
        //    }
        //}

        Uri IUriNode.Uri =>
            this.Node is IUriNode uriNode ?
                uriNode.Uri :
                throw new InvalidOperationException("is not a uri node");

        string IBlankNode.InternalID =>
            this.Node is IBlankNode blankNode ?
                blankNode.InternalID :
                throw new InvalidOperationException("is not a blank node");

        internal Uri BaseUri =>
            this.baseUri ?? this.Graph?.BaseUri;

        public DynamicNode(INode node, Uri baseUri = null) : base(node) =>
            this.baseUri = baseUri;

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) =>
            new MetaDynamic(parameter, this);

        object IDynamicObject.GetIndex(object[] indexes) =>
            new DynamicObjectCollection(this, this.ConvertToNode(indexes));

        object IDynamicObject.GetMember(string name) =>
            (this as IDynamicObject).GetIndex(new[] { name });

        void IDynamicObject.SetIndex(object[] indexes, object value)
        {
            var predicateNode = this.ConvertToNode(indexes);

            this.RetractWithPredicate(predicateNode);
            this.AssertWithPreicateObjects(predicateNode, value);
        }

        void IDynamicObject.SetMember(string name, object value)
        {
            if (this.BaseUri == null)
                throw new InvalidOperationException($"Can't set member {name} without baseUri.");

            (this as IDynamicObject).SetIndex(new[] { name }, value);
        }

        IEnumerable<string> IDynamicObject.GetDynamicMemberNames() =>
            DynamicHelper.ConvertToNames(
                this.Graph
                    .GetTriplesWithSubject(this)
                    .Select(triple => triple.Predicate as IUriNode)
                    .Distinct(),
                this.BaseUri);

        private IUriNode ConvertToNode(object[] indexes)
        {
            if (indexes.Length != 1)
                throw new ArgumentException("Only one index", nameof(indexes));

            var predicate = indexes[0] ?? throw new ArgumentNullException("Can't work with null index", nameof(indexes));

            if (this.Graph == null)
                throw new InvalidOperationException("Node must have graph");

            var predicateNode = DynamicHelper.ConvertToNode(predicate, this.Graph, this.BaseUri);

            return predicateNode;
        }

        private void RetractWithPredicate(IUriNode predicate) =>
            this.Graph.Retract(
                this.Graph.GetTriplesWithSubjectPredicate(this, predicate).ToArray());

        private void AssertWithPreicateObjects(IUriNode predicate, object value) =>
            this.Graph.Assert(
                this.ConvertToTriples(predicate, value));

        private IEnumerable<Triple> ConvertToTriples(INode predicateNode, object value)
        {
            if (value == null)
                yield break;

            if (value is string || !(value is IEnumerable enumerableValue)) // Strings are enumerable but not for our case
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one

            foreach (var item in enumerableValue)
                // TODO: Maybe this should throw on null
                if (item != null)
                    yield return new Triple(
                        subj: this.Node,
                        pred: predicateNode,
                        obj: this.ConvertToNode(item),
                        g: this.Node.Graph);
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
