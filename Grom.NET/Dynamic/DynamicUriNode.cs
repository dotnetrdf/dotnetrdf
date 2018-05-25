namespace Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;

    public class DynamicUriNode : WrapperUriNode, IDynamicMetaObjectProvider, IDynamicMetaObjectProviderContainer
    {
        internal readonly Uri baseUri;
        internal readonly bool collapseSingularArrays;

        public DynamicUriNode(IUriNode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node)
        {
            this.InnerMetaObjectProvider = new DynamicNodeDispatcher(node, baseUri, collapseSingularArrays);
            this.baseUri = baseUri ?? node.Graph?.BaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) => new DelegatingMetaObject(parameter, this);

        public IDynamicMetaObjectProvider InnerMetaObjectProvider { get; private set; }
    }
}
