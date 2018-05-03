/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace VDS.RDF.Writing
{
    public class GraphMLTests : IClassFixture<GraphMLFixture>
    {
        private GraphMLFixture fixture;

        public GraphMLTests(GraphMLFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Produces_a_graph_element_per_graph()
        {
            foreach (var graph in this.fixture.Input.Graphs)
            {
                var graphElement = this.fixture.GraphElementByBaseUri(graph.BaseUri);

                Assert.NotNull(graphElement);
            }
        }

        [Fact]
        public void Produces_an_edge_element_per_triple()
        {
            foreach (var graph in this.fixture.Input.Graphs)
            {
                var graphElement = this.fixture.GraphElementByBaseUri(graph.BaseUri);

                var expected = graph.Triples.Count();
                var actual = graphElement.Elements(XName.Get(GraphMLHelper.Edge, GraphMLHelper.NS)).Count();

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Produces_an_node_element_per_node()
        {
            foreach (var graph in this.fixture.Input.Graphs)
            {
                var graphElement = this.fixture.GraphElementByBaseUri(graph.BaseUri);

                var expected = graph.Nodes.Count();
                var actual = graphElement.Elements(XName.Get(GraphMLHelper.Node, GraphMLHelper.NS)).Count();

                Assert.Equal(expected, actual);
            }
        }

#if NET40
        [Fact]
        public void Output_conforms_to_XSD()
        {
            var reader = this.fixture.Output.CreateReader();

            reader.Settings.Schemas.Add(GraphMLHelper.NS, GraphMLHelper.XsdUri);
            reader.Settings.ValidationType = ValidationType.Schema;
            reader.Settings.ValidationEventHandler += (sender, e) => throw e.Exception;

            while (reader.Read()) { }
        }
#endif
    }

    public class GraphMLFixture
    {
        internal ITripleStore Input { get; private set; }

        internal XDocument Output { get; private set; }

        public GraphMLFixture()
        {
            this.Output = new XDocument();
            this.Input = GraphMLFixture.Load();

            using (var outputWriter = this.Output.CreateWriter())
            {
                new GraphMLWriter().Save(this.Input, outputWriter);
            }
        }

        private static ITripleStore Load()
        {
            var store = new TripleStore();

            var graph1 = new Graph();
            graph1.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            graph1.BaseUri = null;
            store.Add(graph1);

            var graph2 = new Graph();
            graph2.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(graph2);

            return store;
        }

        internal XElement GraphElementByBaseUri(Uri baseUri)
        {
            var graphElements = this.Output.Descendants(XName.Get(GraphMLHelper.Graph, GraphMLHelper.NS));

            if (baseUri == null)
            {
                return graphElements.Single(element => element.Attribute(GraphMLHelper.Id) == null);
            }
            else
            {
                return graphElements.Single(element => element.Attribute(GraphMLHelper.Id)?.Value == baseUri.AbsoluteUri);
            }
        }

    }
}
