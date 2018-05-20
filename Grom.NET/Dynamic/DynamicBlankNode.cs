namespace Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;

    public class DynamicBlankNode : WrapperBlankNode, IDynamicMetaObjectProvider
    {
        internal readonly Uri baseUri;
        internal readonly bool collapseSingularArrays;

        public DynamicBlankNode(IBlankNode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node)
        {
            this.baseUri = baseUri ?? node.Graph?.BaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicNodeMetaObject(parameter, this, this.baseUri, this.collapseSingularArrays);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => base.ToString();
    }
}
