namespace Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using VDS.RDF;

    // TODO: Remove subjectBaseUri in favour of just BaseUri?
    public partial class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        private Uri SubjectBaseUri => this.subjectBaseUri ?? this.BaseUri;

        private Uri PredicateBaseUri => this.predicateBaseUri ?? this.SubjectBaseUri;

        public DynamicGraph(IGraph graph = null, Uri subjectBaseUri = null, Uri predicateBaseUri = null) : base(graph ?? new Graph())
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

        private static IDictionary ConvertToDictionary(object value)
        {
            if (!(value is IDictionary valueDictionary))
            {
                valueDictionary = new Dictionary<object, object>();

                foreach (var property in DynamicGraph.GetProperties(value))
                {
                    valueDictionary[property.Name] = property.GetValue(value);
                }
            }

            return valueDictionary;
        }

        private static IEnumerable<PropertyInfo> GetProperties(object value)
        {
            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(p => !p.GetIndexParameters().Any());

            if (!properties.Any())
            {
                throw new ArgumentException($"Value type {value.GetType()} lacks readable public instance properties.", nameof(value));
            }

            return properties;
        }
    }
}
