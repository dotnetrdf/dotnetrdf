using System.IO;
using System.Xml;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;

namespace VDS.RDF.Writing.Contexts
{
    public class TriXWriterContext
        : BaseGraphStoreWriterContext
    {
        public TriXWriterContext(IGraphStore graphStore, INamespaceMapper namespaces, TextWriter output, XmlWriter writer)
            : base(graphStore, namespaces, output)
        {
            this.XmlWriter = writer;
            // TODO
            this.BlankNodeMapper = new BlankNodeOutputMapper();
        }

        public XmlWriter XmlWriter { get; private set; }

        public BlankNodeOutputMapper BlankNodeMapper { get; private set; }
    }
}
