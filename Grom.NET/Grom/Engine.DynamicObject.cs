namespace Grom
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using VDS.RDF;

    public partial class Engine : DynamicObject
    {
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return
                from node in this.GetNodes().OfType<IUriNode>()
                select this.GetPropertyName(node.Uri);
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

                indexNode = this.graph.GetUriNode(indexUri);
            }

            if (this.graph.Nodes.Contains(indexNode))
            {
                result = this.GetByNode(indexNode);

                return true;
            }

            throw new InvalidOperationException("Can't find that node.");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.prefix == null)
            {
                throw new InvalidOperationException("Can't resolve property names without a prefix.");
            }

            var subjectId = binder.Name;
            var index = new Uri(this.prefix, subjectId);

            return this.TryGetIndex(null, new Uri[] { index }, out result);
        }

        internal Node GetByNode(INode graphNode)
        {
            return this.Single(node => node.graphNode.Equals(graphNode));
        }

        private string GetPropertyName(Uri uri)
        {
            if (this.prefix != null)
            {
                return this.prefix.MakeRelativeUri(uri).ToString();
            }

            return uri.ToString();
        }
    }
}
