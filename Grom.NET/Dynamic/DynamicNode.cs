namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider, IDynamicObject
    {
        private readonly Uri baseUri;

        Uri IUriNode.Uri
        {
            get
            {
                return this.Node is IUriNode uriNode ? uriNode.Uri : throw new InvalidOperationException("is not a uri node");
            }
        }

        string IBlankNode.InternalID
        {
            get
            {
                return this.Node is IBlankNode blankNode ? blankNode.InternalID : throw new InvalidOperationException("is not a blank node");
            }
        }

        internal Uri BaseUri
        {
            get
            {
                return this.baseUri ?? this.Graph?.BaseUri;
            }
        }

        public DynamicNode(INode node, Uri baseUri = null) : base(node)
        {
            this.baseUri = baseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaDynamic(parameter, this);
        }

        object IDynamicObject.GetMember(string name)
        {
            return this[name];
        }

        void IDynamicObject.SetMember(string name, object value)
        {
            if (this.BaseUri == null)
            {
                throw new InvalidOperationException($"Can't set member {name} without baseUri.");
            }

            this[name] = value;
        }

        IEnumerable<string> IDynamicObject.GetDynamicMemberNames()
        {
            return DynamicHelper.ConvertToNames(
                this.Graph
                    .GetTriplesWithSubject(this)
                    .Select(triple => triple.Predicate as IUriNode)
                    .Distinct(),
                this.BaseUri);
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
