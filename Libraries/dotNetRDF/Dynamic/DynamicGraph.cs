namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    public partial class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        public Uri SubjectBaseUri
        {
            get
            {
                return this.subjectBaseUri ?? this.BaseUri;
            }
        }

        public Uri PredicateBaseUri
        {
            get
            {
                return this.predicateBaseUri ?? this.SubjectBaseUri;
            }
        }

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
