namespace Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;

    public class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider, IDynamicMetaObjectProviderContainer
    {
        internal readonly Uri subjectBaseUri;
        internal readonly Uri predicateBaseUri;
        internal readonly bool collapseSingularArrays;

        public DynamicGraph(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null, bool collapseSingularArrays = false) : base(graph)
        {
            this.InnerMetaObjectProvider = new DynamicGraphDispatcher(this);
            this.subjectBaseUri = subjectBaseUri ?? this.BaseUri;
            this.predicateBaseUri = predicateBaseUri ?? this.subjectBaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) => new DelegatingMetaObject(parameter, this);

        public IDynamicMetaObjectProvider InnerMetaObjectProvider { get; private set; }
    }
}
