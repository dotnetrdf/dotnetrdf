namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider
    {
        private readonly Uri baseUri;

        // TODO: Make sure all instantiations copy original node to appropriate host graph
        public DynamicNode(INode node, Uri baseUri = null) : base(node)
        {
            if (Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            this.baseUri = baseUri;
        }

        public Uri BaseUri
        {
            get
            {
                return this.baseUri ?? this.Graph.BaseUri;
            }
        }

        Uri IUriNode.Uri
        {
            get
            {
                return this.Node is IUriNode uriNode ?
                    uriNode.Uri :
                    throw new InvalidOperationException("is not a uri node");
            }
        }

        string IBlankNode.InternalID
        {
            get
            {
                return this.Node is IBlankNode blankNode ?
                    blankNode.InternalID :
                    throw new InvalidOperationException("is not a blank node");
            }
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }
    }
}
