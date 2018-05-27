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

        public DynamicMetaObject GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

        public DynamicUriNode GetIndex(object[] indexes)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subjectIndex = indexes[0] ?? throw new ArgumentNullException("Can't work with null index", "indexes"); ;

            return this.GetIndex(subjectIndex) ?? throw new Exception("index not found");
        }

        public DynamicUriNode GetMember(string name)
        {
            if (this.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            return this.GetIndex(name) ?? throw new Exception("member not found");
        }

        public object SetIndex(object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            this.SetIndex(indexes[0], value);

            return value;
        }

        public object SetMember(string name, object value)
        {
            if (this.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            this.SetIndex(name, value);

            return value;
        }

        public IEnumerable<string> GetDynamicMemberNames()
        {
            var subjects = this
                .Triples
                .Select(triple => triple.Subject)
                .UriNodes()
                .Distinct();

            return DynamicHelper.ConvertToNames(subjects, this.subjectBaseUri);
        }

        private DynamicUriNode GetIndex(object subject)
        {
            var subjectNode = DynamicHelper.ConvertToNode(subject, this, this.subjectBaseUri);

            return this.Triples
                .SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(subjectNode))
                .Select(node => node.AsDynamic(this.predicateBaseUri, this.collapseSingularArrays))
                .SingleOrDefault();
        }

        private void SetIndex(object index, object value)
        {
            var targetNode = this.GetDynamicNodeByIndexOrCreate(index);

            if (value == null)
            {
                this.RetractWithSubject(targetNode);

                return;
            }

            var valueDictionary = DynamicGraph.ConvertToDictionary(value);
            var dynamicTarget = targetNode as dynamic;

            foreach (DictionaryEntry entry in valueDictionary)
            {
                dynamicTarget[entry.Key] = entry.Value;
            }
        }

        private DynamicUriNode GetDynamicNodeByIndexOrCreate(object subjectIndex)
        {
            var indexNode = DynamicHelper.ConvertToNode(subjectIndex, this, this.subjectBaseUri);

            return this.GetIndex(indexNode) ?? indexNode.AsDynamic(this.predicateBaseUri, this.collapseSingularArrays);
        }

        private void RetractWithSubject(DynamicUriNode targetNode)
        {
            var triples = this.GetTriplesWithSubject(targetNode).ToArray();

            this.Retract(triples);
        }

        private static IDictionary ConvertToDictionary(object value)
        {
            if (!(value is IDictionary valueDictionary))
            {
                valueDictionary = new Dictionary<object, object>();

                var properties = value.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Where(p => !p.GetIndexParameters().Any());

                if (!properties.Any())
                {
                    throw new ArgumentException($"Value type {value.GetType()} lacks readable public instance properties.", "value");
                }

                foreach (var property in properties)
                {
                    valueDictionary[property.Name] = property.GetValue(value);
                }
            }

            return valueDictionary;
        }
    }
}
