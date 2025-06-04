using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF;

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

    public INode ActionNode => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:action"))
        .Select(t => t.Object).FirstOrDefault();

    public Uri Query => Graph.GetTriplesWithSubjectPredicate(ActionNode, Graph.CreateUriNode("qt:query"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public Uri Data => Graph.GetTriplesWithSubjectPredicate(ActionNode, Graph.CreateUriNode("qt:data"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public IEnumerable<Uri> GraphData => Graph.GetTriplesWithSubjectPredicate(ActionNode, Graph.CreateUriNode("qt:graphData"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri);
    
    public Uri UpdateData => Graph.GetTriplesWithSubjectPredicate(ActionNode, Graph.CreateUriNode("ut:data"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public Uri UpdateRequest => Graph.GetTriplesWithSubjectPredicate(ActionNode, Graph.CreateUriNode("ut:request"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public Uri UpdateResult => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:result"))
        .SelectMany(t => Graph.GetTriplesWithSubjectPredicate(t.Object, Graph.CreateUriNode("ut:data")))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public string EntailmentRegime => Graph
        .GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:entailmentRegime"))
        .Select(t => t.Object).OfType<ILiteralNode>().Select(t => t.Value).FirstOrDefault();

    public string HashAlgorithm => Graph
        .GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("rdfc:hashAlgorithm"))
        .Select(t => t.Object).OfType<ILiteralNode>().Select(lit => lit.Value).FirstOrDefault();

    public bool LaxCardinality => Graph.ContainsTriple(new Triple(TestNode,
        Graph.CreateUriNode("mf:resultCardinality"), Graph.CreateUriNode("mf:LaxCardinality")));

    public override string ToString()
    {
        return Id;
    }
}