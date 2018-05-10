namespace Grom
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
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
            this.subjectBaseUri = subjectBaseUri ?? graph.BaseUri;
            this.predicateBaseUri = predicateBaseUri; // TODO: Default to subjectBaseUrI?
            this.collapseSingularArrays = collapseSingularArrays;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var subject = indexes[0];

            var subjectNode = Helper.ConvertIndex(subject, this.graph, this.subjectBaseUri);

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

            if (!this.TryGetIndex(null, indexes, out object result))
            {
                var subject = indexes[0];
                var subjectNode = Helper.ConvertIndex(subject, this.graph, this.subjectBaseUri);
                result = new NodeWrapper(subjectNode, this.predicateBaseUri, this.collapseSingularArrays);
            }

            var subjectWrapper = result as NodeWrapper;

            foreach (var p in value.GetType().GetProperties())
            {
                subjectWrapper.TrySetIndex(null, new[] { p.Name }, p.GetValue(value));
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
