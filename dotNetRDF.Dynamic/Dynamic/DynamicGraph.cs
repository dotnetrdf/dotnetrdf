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

        private Uri SubjectBaseUri
        {
            get
            {
                return this.subjectBaseUri ?? this.BaseUri;
            }
        }

        private Uri PredicateBaseUri
        {
            get
            {
                return this.predicateBaseUri ?? this.SubjectBaseUri;
            }
        }

        private object this[string subject]
        {
            get
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                return this.GetDynamicNode(subject) ?? throw new Exception("index not found");
            }
            set
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                var targetNode = this.GetDynamicNodeOrCreate(subject);

                if (value == null)
                {
                    targetNode.Clear();
                }
                else
                {
                    foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
                    {
                        // TODO: What if value is s a node? Will we get properties for it? Shouldn't.
                        targetNode[entry.Key.ToString()] = entry.Value;
                    }
                }
            }
        }

        private object this[Uri subject]
        {
            get
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                return this.GetDynamicNode(subject) ?? throw new Exception("index not found");
            }
            set
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                var targetNode = this.GetDynamicNodeOrCreate(subject);

                if (value == null)
                {
                    targetNode.Clear();
                }
                else
                {
                    foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
                    {
                        // TODO: What if value is s a node? Will we get properties for it? Shouldn't.
                        targetNode[entry.Key.ToString()] = entry.Value;
                    }
                }
            }
        }

        private object this[INode subject]
        {
            get
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                return this.GetDynamicNode(subject) ?? throw new Exception("index not found");
            }
            set
            {
                if (subject == null)
                {
                    throw new ArgumentNullException("Can't work with null index", nameof(subject));
                }

                var targetNode = this.GetDynamicNodeOrCreate(subject);

                if (value == null)
                {
                    targetNode.Clear();
                }
                else
                {
                    foreach (DictionaryEntry entry in DynamicGraph.ConvertToDictionary(value))
                    {
                        // TODO: What if value is s a node? Will we get properties for it? Shouldn't.
                        targetNode[entry.Key.ToString()] = entry.Value;
                    }
                }
            }
        }

        public DynamicGraph(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null) : base(graph)
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new MetaDynamic(parameter, this);
        }

        IEnumerable<string> IDynamicObject.GetDynamicMemberNames()
        {
            return DynamicHelper.ConvertToNames(
                this.Triples
                    .SubjectNodes
                    .UriNodes(),
                this.SubjectBaseUri);
        }

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
