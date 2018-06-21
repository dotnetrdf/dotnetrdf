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
    public class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider, IDynamicObject
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        private Uri SubjectBaseUri =>
            this.subjectBaseUri ?? this.BaseUri;

        private Uri PredicateBaseUri =>
            this.predicateBaseUri ?? this.SubjectBaseUri;

        public DynamicGraph(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null) : base(graph)
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) =>
            new MetaDynamic(parameter, this);

        object IDynamicObject.GetIndex(object[] indexes)
        {
            if (indexes.Length != 1)
                throw new ArgumentException("Only one index", "indexes");

            var subject = indexes[0] ?? throw new ArgumentNullException("Can't work with null index", nameof(indexes)); ;

            return this.GetDynamicNode(subject) ?? throw new Exception("index not found");
        }

        object IDynamicObject.GetMember(string name) =>
            (this as IDynamicObject).GetIndex(new[] { name });

        void IDynamicObject.SetIndex(object[] indexes, object value)
        {
            if (indexes.Length != 1)
                throw new ArgumentException("Only one index", nameof(indexes));

            var subject = indexes[0] ?? throw new ArgumentNullException("Can't work with null index", nameof(indexes));
        
            var targetNode = this.GetDynamicNodeOrCreate(subject);

            if (value == null)
                this.RetractWithSubject(targetNode);
            else
                foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
                    (targetNode as IDynamicObject).SetIndex(new[] { entry.Key }, entry.Value);
        }

        void IDynamicObject.SetMember(string name, object value) =>
            (this as IDynamicObject).SetIndex(new[] { name }, value);

        IEnumerable<string> IDynamicObject.GetDynamicMemberNames() =>
            DynamicHelper.ConvertToNames(
                this.Triples
                    .SubjectNodes
                    .UriNodes(),
                this.SubjectBaseUri);

        private DynamicNode GetDynamicNode(object other)
        {
            var otherNode = DynamicHelper.ConvertToNode(other, this, this.SubjectBaseUri);

            return this.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(otherNode))
                .Select(node => node.AsDynamic(this.PredicateBaseUri))
                .SingleOrDefault();
        }

        private DynamicNode GetDynamicNodeOrCreate(object subjectIndex)
        {
            var indexNode = DynamicHelper.ConvertToNode(subjectIndex, this, this.SubjectBaseUri);

            return this.GetDynamicNode(indexNode) ?? indexNode.AsDynamic(this.PredicateBaseUri);
        }

        private void RetractWithSubject(INode node) =>
            this.Retract(
                this.GetTriplesWithSubject(node).ToArray());

        private static IDictionary ConvertToDictionary(object value)
        {
            if (!(value is IDictionary valueDictionary))
            {
                valueDictionary = new Dictionary<object, object>();

                foreach (var property in DynamicGraph.GetProperties(value))
                    valueDictionary[property.Name] = property.GetValue(value);
            }

            return valueDictionary;
        }

        private static IEnumerable<PropertyInfo> GetProperties(object value)
        {
            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(p => !p.GetIndexParameters().Any());

            if (!properties.Any())
                throw new ArgumentException($"Value type {value.GetType()} lacks readable public instance properties.", nameof(value));

            return properties;
        }
    }
}
