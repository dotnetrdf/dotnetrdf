namespace Grom
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF;

    public class GraphWrapper : DynamicObject
    {
        private readonly IGraph graph;
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;
        private readonly bool collapseSingularArrays;

        public GraphWrapper(IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null, bool collapseSingularArrays = false)
        {
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.subjectBaseUri = subjectBaseUri ?? this.graph.BaseUri;
            this.predicateBaseUri = predicateBaseUri ?? this.subjectBaseUri;
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subjectNode = Helper.ConvertIndex(indexes[0], this.graph, this.subjectBaseUri);

            result = this.graph.Triples.SubjectNodes
                .UriNodes()
                .Where(node => node.Equals(subjectNode))
                .Select(node => new NodeWrapper(node, this.predicateBaseUri, this.collapseSingularArrays))
                .SingleOrDefault();

            return result != null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't get member without baseUri.");
            }

            return this.TryGetIndex(null, new[] { binder.Name }, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            if (value is IEnumerable && !(value is string || value is IDictionary))
            {
                throw new ArgumentException("Value cannot be enumerable.", "value");
            }

            var subjectIndex = indexes[0];

            if (!this.TryGetIndex(null, indexes, out object result))
            {
                var subjectNode = Helper.ConvertIndex(subjectIndex, this.graph, this.subjectBaseUri);
                result = new NodeWrapper(subjectNode, this.predicateBaseUri, this.collapseSingularArrays);
            }

            var subjectWrapper = result as NodeWrapper;

            if (!(value is IDictionary valueDictionary))
            {
                valueDictionary = new Dictionary<object, object>();

                var properties = value.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .Where(p => p.CanRead)
                    .Where(p => p.GetIndexParameters().Count() == 0);

                if (!properties.Any())
                {
                    throw new ArgumentException($"Value type {value.GetType()} for subject {subjectIndex} lacks readable public instance properties.", "value");
                }

                foreach (var property in properties)
                {
                    valueDictionary[property.Name] = property.GetValue(value);
                    //try
                    //{
                    //subjectWrapper.TrySetIndex(null, new[] { property.Name }, property.GetValue(value));
                    //}
                    //catch (Exception e)
                    //{
                    //    throw new Exception($"Can't set property {property.Name} for {subjectIndex}", e);
                    //}
                }
            }

            foreach (var key in valueDictionary.Keys)
            {
                subjectWrapper.TrySetIndex(null, new[] { key }, valueDictionary[key]);
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.subjectBaseUri == null)
            {
                throw new InvalidOperationException("Can't set member without baseUri.");
            }

            return this.TrySetIndex(null, new[] { binder.Name }, value);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var subjectUriNodes = this.graph.Triples.SubjectNodes.UriNodes();

            return Helper.GetDynamicMemberNames(subjectUriNodes, this.subjectBaseUri);
        }
    }
}
