/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SchemaWriter
    {
        [TestMethod]
        public void WritingHtmlSchemaWriter()
        {
            //Load the Graph from within the Assembly
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StreamReader(Assembly.GetAssembly(typeof(IGraph)).GetManifestResourceStream("VDS.RDF.Configuration.configuration.ttl"), Encoding.UTF8));

            //Now generate the HTML file
            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            writer.Save(g, "configSchema.html");
        }

        [TestMethod]
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

            Assert.IsFalse(strWriter.ToString().Contains("type"), "Should not have documented any classes");
        }

        [TestMethod]
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

            Assert.IsTrue(strWriter.ToString().Contains("ex:one"), "Should have documented ex:one as a range");
            Assert.IsTrue(strWriter.ToString().Contains("ex:two"), "Should have documented ex:two as a range");
        }
    }
}
