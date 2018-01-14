namespace Grom
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;

    public partial class Node : DynamicObject
    {
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var predicates =
                from node in this.graphNode.Graph.GetTriplesWithSubject(this.graphNode)
                select GetPropertyName(node.Predicate as IUriNode);

            return predicates.Distinct();
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            // TODO: Support second int index for ordinal access?
            if (indexes.Length != 1)
            {
                throw new ArgumentException("Only one index", "indexes");
            }

            var indexObject = indexes[0];

            // TODO: also support int?
            if (!(indexObject is INode indexNode))
            {
                if (!(indexObject is Uri indexUri))
                {
                    if (indexObject is string indexString)
                    {
                        if (!Uri.TryCreate(indexString, UriKind.Absolute, out indexUri))
                        {
                            throw new FormatException("Illegal Uri.");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Only INode, Uri or string indexes", "indexes");
                    }
                }

                indexNode = this.graphNode.Graph.CreateUriNode(indexUri);
            }

            var propertyTriples = this.graphNode.Graph.GetTriplesWithSubjectPredicate(this.graphNode, indexNode);
            var nodes =
                from triple in propertyTriples
                select this.Convert(triple.Object);

            result = nodes.ToArray();
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.context.prefix == null)
            {
                throw new InvalidOperationException("Can't resolve property names without a prefix.");
            }

            var propertyName = binder.Name;
            var index = new Uri(this.context.prefix, propertyName);

            return this.TryGetIndex(null, new Uri[] { index }, out result);
        }

        private string GetPropertyName(IUriNode predicate)
        {
            if (this.context.prefix != null)
            {
                return this.context.prefix.MakeRelativeUri(predicate.Uri).ToString();
            }

            return predicate.ToString();
        }
    }
}