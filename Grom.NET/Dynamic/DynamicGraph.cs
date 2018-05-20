namespace Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;

    public class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider
    {
        internal readonly Uri subjectBaseUri;
        internal readonly Uri predicateBaseUri;
        internal readonly bool collapseSingularArrays;

        public DynamicGraph(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null, bool collapseSingularArrays = false) : base(graph)
        {
            this.subjectBaseUri = subjectBaseUri ?? this.BaseUri;
            this.predicateBaseUri = predicateBaseUri ?? this.subjectBaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicGraphMetaObject(parameter, this);
        }
    }
}
