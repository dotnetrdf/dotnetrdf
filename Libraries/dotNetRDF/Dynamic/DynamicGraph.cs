namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    // TODO: Remove subjectBaseUri in favour of just BaseUri?
    public partial class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        public Uri SubjectBaseUri => this.subjectBaseUri ?? this.BaseUri;

        public Uri PredicateBaseUri => this.predicateBaseUri ?? this.SubjectBaseUri;

        public DynamicGraph(IGraph graph = null, Uri subjectBaseUri = null, Uri predicateBaseUri = null) : base(graph ?? new Graph())
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }
    }
}
