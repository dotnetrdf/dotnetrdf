using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Query.Inference;

public class StaticRdfsReasonerTests
{
    [Fact]
    public void ItHandlesCircularSubpropertyOf()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "circular_subproperty.ttl"));
        var reasoner = new StaticRdfsReasoner();
        reasoner.Initialise(g);
        reasoner.Apply(g, g);
        INode obj= g.GetUriNode(new Uri("http://example.com/o"));
        INode subj = g.GetUriNode(new Uri("http://example.com/s"));
        Assert.NotNull(subj);
        Assert.NotNull(obj);
        INode[] expectedNodes =
        [
            g.CreateUriNode(new Uri("http://example.com/ontology/predicate#a")),
            g.CreateUriNode(new Uri("http://example.com/ontology/predicate#b")),
            g.CreateUriNode(new Uri("http://example.com/source/predicate#c")),
            g.CreateUriNode(new Uri("http://example.com/ontology/predicate#test"))
        ];
        IList<INode> predicates = g.GetTriplesWithSubjectObject(subj, obj).Select(t => t.Predicate).ToList();
        foreach (INode expectedNode in expectedNodes)
        {
            Assert.Contains(expectedNode, predicates);
        }
    }

    [Fact]
    public void ItHandlesCircularSubClassOf()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "circular_subclass.ttl"));
        var reasoner = new StaticRdfsReasoner();
        reasoner.Initialise(g);
        reasoner.Apply(g, g);
        INode inst = g.GetUriNode(new Uri("http://example.org/test"));
        INode rdfType = g.CreateUriNode("rdf:type");
        INode[] expectedClasses =
        [
            g.CreateUriNode(new Uri("http://example.org/ontology/A")),
            g.CreateUriNode(new Uri("http://example.org/ontology/B")),
            g.CreateUriNode(new Uri("http://example.org/ontology/C"))
        ];
        IList<INode> actualClasses = g.GetTriplesWithSubjectPredicate(inst, rdfType).Select(t => t.Object).ToList();
        foreach (INode expectedClass in expectedClasses)
        {
            Assert.Contains(expectedClass, actualClasses);
        }
    }

    [Fact]
    public void ItHandlesPolymorphicSubClassOf()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "polymorphic_subclass.ttl"));
        var reasoner = new StaticRdfsReasoner();
        reasoner.Initialise(g);
        reasoner.Apply(g, g);
        INode inst = g.GetUriNode(new Uri("http://example.org/test"));
        INode rdfType = g.CreateUriNode("rdf:type");
        INode[] expectedClasses = 
        [
            g.CreateUriNode(new Uri("http://example.org/ontology/A")),
            g.CreateUriNode(new Uri("http://example.org/ontology/B")),
            g.CreateUriNode(new Uri("http://example.org/ontology/C"))
        ];
        IList<INode> actualClasses = g.GetTriplesWithSubjectPredicate(inst, rdfType).Select(t => t.Object).ToList();
        Assert.Equal(expectedClasses.Length, actualClasses.Count);
        foreach (INode expectedClass in expectedClasses)
        {
            Assert.Contains(expectedClass, actualClasses);
        }
    }

    [Fact]
    public void ItHandlesPolymorphicSubPropertyOf()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "polymorphic_subproperty.ttl"));
        var reasoner = new StaticRdfsReasoner();
        reasoner.Initialise(g);
        reasoner.Apply(g, g);
        INode subj = g.GetUriNode(new Uri("http://example.org/s"));
        INode obj = g.GetUriNode(new Uri("http://example.org/o"));
        INode[] expectedPredicates = [
            g.CreateUriNode(new Uri("http://example.org/ontology/a")),
            g.CreateUriNode(new Uri("http://example.org/ontology/b")),
            g.CreateUriNode(new Uri("http://example.org/ontology/c")),
        ];
        IList<INode> actualPredicates = g.GetTriplesWithSubjectObject(subj, obj).Select(t=>t.Predicate).ToList();
        Assert.Equal(expectedPredicates.Length, actualPredicates.Count);
        foreach (INode expectedPredicate in expectedPredicates)
        {
            Assert.Contains(expectedPredicate, actualPredicates);
        }
    }

}