namespace Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;

    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider
    {
        private readonly Uri baseUri;

        Uri IUriNode.Uri => this.Node is IUriNode uriNode ? uriNode.Uri : throw new InvalidOperationException("is not a uri node");

        string IBlankNode.InternalID => this.Node is IBlankNode blankNode ? blankNode.InternalID : throw new InvalidOperationException("is not a blank node");

        internal Uri BaseUri => this.baseUri ?? this.Graph?.BaseUri;

        public DynamicNode(INode node, Uri baseUri = null) : base(node) => this.baseUri = baseUri;

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);
    }
}
