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

        public Uri BaseUri => this.baseUri ?? this.Graph?.BaseUri;

        // TODO: Make sure all instantiations copy original node to appropriate host graph
        public DynamicNode(INode node, Uri baseUri = null) : base(node)
        {
            if (Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            this.baseUri = baseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }

        // TODO: Clean this convert mess
        public void Add(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    Add(stringKey, value);
                    return;

                case Uri uriKey:
                    Add(uriKey, value);
                    return;

                case INode nodeKey:
                    Add(nodeKey, value);
                    return;

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }
        }

        internal bool Contains(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    return Contains(stringKey, value);

                case Uri uriKey:
                    return Contains(uriKey, value);

                case INode nodeKey:
                    return Contains(nodeKey, value);

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }
        }

        internal bool Remove(object key, object value)
        {
            switch (key)
            {
                case string stringKey:
                    return Remove(stringKey, value);

                case Uri uriKey:
                    return Remove(uriKey, value);

                case INode nodeKey:
                    return Remove(nodeKey, value);

                default:
                    // TODO: Make more specific
                    throw new Exception();
            }

        }
    }
}
