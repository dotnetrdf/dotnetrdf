namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider
    {
        private readonly Uri baseUri;

        Uri IUriNode.Uri => this.Node is IUriNode uriNode ? uriNode.Uri : throw new InvalidOperationException("is not a uri node");

        string IBlankNode.InternalID => this.Node is IBlankNode blankNode ? blankNode.InternalID : throw new InvalidOperationException("is not a blank node");

        internal Uri BaseUri => this.baseUri ?? this.Graph?.BaseUri;

        public DynamicNode(INode node, Uri baseUri = null) : base(node)
        {
            this.baseUri = baseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaDynamic(parameter, this);
        }

        internal object this[object key]
        {
            get
            {
                switch (key)
                {
                    case string stringKey:
                        return this[stringKey];

                    case Uri uriKey:
                        return this[uriKey];

                    case INode nodeKey:
                        return this[nodeKey];

                    default:
                        throw new ArgumentException(nameof(key));
                }
            }
            set
            {
                switch (key)
                {
                    case string stringKey:
                        this[stringKey] = value;
                        break;

                    case Uri uriKey:
                        this[uriKey] = value;
                        break;

                    case INode nodeKey:
                        this[nodeKey] = value;
                        break;

                    default:
                        throw new Exception();
                }
            }
        }

        internal bool Contains(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    return this.Contains(stringKey, value);

                case Uri uriKey:
                    return this.Contains(uriKey, value);

                case INode nodeKey:
                    return this.Contains(nodeKey, value);

                default:
                    throw new Exception();
            }
        }
    }
}
