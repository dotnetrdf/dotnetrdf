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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Writing
{

    public class SchemaWriter
    {
        [Fact]
        public void WritingHtmlSchemaWriter()
        {
            //Load the Graph from within the Assembly
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StreamReader(typeof(IGraph).GetTypeInfo().Assembly.GetManifestResourceStream("VDS.RDF.Configuration.configuration.ttl"), Encoding.UTF8));

            //Now generate the HTML file
            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            writer.Save(g, "configSchema.html");
        }

        [Fact]
        public void WritingHtmlSchemaWriterAnonClasses()
        {
            //Create an example Graph
            Graph g = new Graph();
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("rdfs:class"));

            TestTools.ShowGraph(g);

            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.False(strWriter.ToString().Contains("type"), "Should not have documented any classes");
        }

        [Fact]
        public void WritingHtmlSchemaWriterUnionOfRanges()
        {
            //Create an example Graph
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
            g.NamespaceMap.AddNamespace("owl", UriFactory.Create(NamespaceMapper.OWL));
            INode testProperty = g.CreateUriNode("ex:property");
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfProperty = g.CreateUriNode("rdf:Property");
            INode rdfsRange = g.CreateUriNode("rdfs:range");
            INode union = g.CreateBlankNode();
            INode unionOf = g.CreateUriNode("owl:unionOf");
            INode testItem1 = g.CreateUriNode("ex:one");
            INode testItem2 = g.CreateUriNode("ex:two");

            g.Assert(testProperty, rdfType, rdfProperty);
            g.Assert(testProperty, rdfsRange, union);
            g.Assert(union, unionOf, g.AssertList(new INode[] { testItem1, testItem2 }));

            TestTools.ShowGraph(g);

            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.True(strWriter.ToString().Contains("ex:one"), "Should have documented ex:one as a range");
            Assert.True(strWriter.ToString().Contains("ex:two"), "Should have documented ex:two as a range");
        }
    }
}
