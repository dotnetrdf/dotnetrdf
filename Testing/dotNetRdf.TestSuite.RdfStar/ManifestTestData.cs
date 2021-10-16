using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.TestSuite.RdfStar
{
    public class ManifestTestData
    {
        public readonly Manifest Manifest;
        public readonly IGraph Graph;
        public readonly IUriNode TestNode;

        public ManifestTestData(Manifest manifest, IUriNode testNode)
        {
            Manifest = manifest;
            Graph = manifest.Graph;
            TestNode = testNode;
        }

        public string Id => TestNode.Uri.AbsoluteUri;

        public string Name => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("mf:name"))
            .Select(t => t.Object).OfType<ILiteralNode>().Select(lit => lit.Value).FirstOrDefault();

        public Uri Type => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("rdf:type"))
            .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

        public Uri Action => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:action"))
            .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

        public Uri Result => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:result"))
            .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

        public override string ToString()
        {
            return Id;
        }
    }
}
