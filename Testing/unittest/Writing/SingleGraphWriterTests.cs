using System;
using System.Text;
using FluentAssertions;
using Moq;
using Xunit;

namespace VDS.RDF.Writing
{
    public class SingleGraphWriterTests
    {
        [Fact]
        public void ItInvokesTheStoreWriterSaveMethodWithAFileName()
        {
            var mockStoreWriter = new Mock<IStoreWriter>();
            var graph = new Graph();
            var writer = new SingleGraphWriter(mockStoreWriter.Object);
            writer.Save(graph, "test");
            mockStoreWriter.Verify(x=>x.Save(It.IsAny<ITripleStore>(), "test"), Times.Once);
        }

        [Fact]
        public void ItInvokesTheStoreWriterSaveMethodWithATextWriter()
        {
            var mockStoreWriter = new Mock<IStoreWriter>();
            var graph = new Graph();
            var writer = new SingleGraphWriter(mockStoreWriter.Object);
            var output = new System.IO.StringWriter();
            writer.Save(graph, output);
            mockStoreWriter.Verify(x => x.Save(It.Is<ITripleStore>(ts=>ts.Graphs.Count.Equals(1)), output), Times.Once);
        }

        [Fact]
        public void ItInvokesTheStoreWriterSaveMethodWithATextWriterAndLeaveOpenFlag()
        {
            var mockStoreWriter = new Mock<IStoreWriter>();
            var graph = new Graph();
            var writer = new SingleGraphWriter(mockStoreWriter.Object);
            var output = new System.IO.StringWriter();
            writer.Save(graph, output, true);
            mockStoreWriter.Verify(x => x.Save(It.Is<ITripleStore>(ts => ts.Graphs.Count.Equals(1)), output, true), Times.Once);
        }

        [Fact]
        public void ItWritesNQuadsOutputWithADefaultGraph()
        {
            var graph = new Graph();
            graph.Assert(new Triple(graph.CreateUriNode(new Uri("http://example.org/s")),
                graph.CreateUriNode(new Uri("http://example.org/p")),
                graph.CreateUriNode(new Uri("http://example.org/o"))));
            var buffer = new StringBuilder();
            var writer = new SingleGraphWriter(new NQuadsWriter());
            var output = new System.IO.StringWriter(buffer);
            writer.Save(graph, output);
            buffer.ToString().Should()
                .Contain("<http://example.org/s> <http://example.org/p> <http://example.org/o> .");
        }

        [Fact]
        public void ItWritesNQuadsOutputWithANamedGraph()
        {
            var graph = new Graph {BaseUri = new Uri("http://example.org/g")};
            graph.Assert(new Triple(graph.CreateUriNode(new Uri("http://example.org/s")),
                graph.CreateUriNode(new Uri("http://example.org/p")),
                graph.CreateUriNode(new Uri("http://example.org/o"))));
            var buffer = new StringBuilder();
            var writer = new SingleGraphWriter(new NQuadsWriter());
            var output = new System.IO.StringWriter(buffer);
            writer.Save(graph, output);
            buffer.ToString().Should()
                .Contain("<http://example.org/s> <http://example.org/p> <http://example.org/o> <http://example.org/g> .");
        }
    }
}
