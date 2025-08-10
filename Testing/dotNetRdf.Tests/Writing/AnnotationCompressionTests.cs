using FluentAssertions;
using System.IO;
using VDS.RDF.Writing.Contexts;
using Xunit;

namespace VDS.RDF.Writing;

public class AnnotationCompressionTests
{
    [Fact]
    public void TestLocateSimpleAnnotation()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Root.Create("http://example.org/"));
        var t = new Triple(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), g.CreateUriNode("ex:o"));
        var tAnnotation = new Triple(new TripleNode(t), g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"));
        g.Assert(t);
        g.Assert(tAnnotation);

        var context = new CompressingTurtleWriterContext(g, TextWriter.Null);
        WriterHelper.FindAnnotations(context);
        context.Annotations.Should().ContainKey(t);
        context.Annotations[t].Should().ContainSingle().Which.Should().Be(tAnnotation);
        context.TriplesDone.Should().ContainSingle().Which.Should().Be(tAnnotation);
    }

    [Fact]
    public void TestLocateMultipleAnnotations()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Root.Create("http://example.org/"));
        var t = new Triple(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), g.CreateUriNode("ex:o"));
        var tAnnotation = new Triple(new TripleNode(t), g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"));
        var tAnnotation2 = new Triple(new TripleNode(t), g.CreateUriNode("ex:p"), g.CreateUriNode("ex:o"));
        var notAnnotation = new Triple(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), new TripleNode(t));
        g.Assert(new [] {t, tAnnotation, tAnnotation2, notAnnotation});

        var context = new CompressingTurtleWriterContext(g, TextWriter.Null);
        WriterHelper.FindAnnotations(context);
        context.Annotations.Should().ContainKey(t);
        context.Annotations[t].Should().HaveCount(2).And.Contain(new[] { tAnnotation, tAnnotation2 });
        context.Annotations[t].Should().NotContain(notAnnotation);
        context.TriplesDone.Should().HaveCount(2).And.Contain(new[] { tAnnotation, tAnnotation2 });
    }
}
