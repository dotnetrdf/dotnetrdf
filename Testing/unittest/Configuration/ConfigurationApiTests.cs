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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class ConfigurationApiTests
    {
        [TestMethod,ExpectedException(typeof(DotNetRdfConfigurationException))]
        public void ConfigurationCircularReference()
        {
            String graph = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:fromGraph _:b .
_:b a dnr:Graph ;
  dnr:fromGraph _:a .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            ConfigurationLoader.LoadObject(g, g.GetBlankNode("a"));
        }

        [TestMethod]
        public void ConfigurationImports1()
        {
            //Single Import
            String graph1 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:usingTripleCollection <ex:collection> .

[] dnr:imports ""ConfigurationImports1-b.ttl"" . ";

            String graph2 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" .";

            File.WriteAllText("ConfigurationImports1-a.ttl", graph1);
            File.WriteAllText("ConfigurationImports1-b.ttl", graph2);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImports1-a.ttl");

            TestTools.ShowGraph(g);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [TestMethod]
        public void ConfigurationImports2()
        {
            //Chained Import
            String graph1 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:usingTripleCollection <ex:collection> .

[] dnr:imports ""ConfigurationImports2-b.ttl"" . ";

            String graph2 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <ex:innerCollection> .

[] dnr:imports ""ConfigurationImports2-c.ttl"" .";

            String graph3 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:innerCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.SubTreeIndexedTripleCollection"" .";

            File.WriteAllText("ConfigurationImports2-a.ttl", graph1);
            File.WriteAllText("ConfigurationImports2-b.ttl", graph2);
            File.WriteAllText("ConfigurationImports2-c.ttl", graph3);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImports2-a.ttl");

            TestTools.ShowGraph(g);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [TestMethod]
        public void ConfigurationImports3()
        {
            //Multiple Imports
            String graph1 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:usingTripleCollection <ex:collection> .

[] dnr:imports ""ConfigurationImports3-b.ttl"" , ""ConfigurationImports3-c.ttl"" . ";

            String graph2 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <ex:innerCollection> .";

            String graph3 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:innerCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.SubTreeIndexedTripleCollection"" .";

            File.WriteAllText("ConfigurationImports3-a.ttl", graph1);
            File.WriteAllText("ConfigurationImports3-b.ttl", graph2);
            File.WriteAllText("ConfigurationImports3-c.ttl", graph3);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImports3-a.ttl");

            TestTools.ShowGraph(g);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [TestMethod]
        public void ConfigurationImports4()
        {
            //Repeated Imports
            String graph1 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:usingTripleCollection <ex:collection> .

[] dnr:imports ""ConfigurationImports3-b.ttl"" , ""ConfigurationImports3-c.ttl"", ""ConfigurationImports3-c.ttl"" . ";

            String graph2 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <ex:innerCollection> .";

            String graph3 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
<ex:innerCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.SubTreeIndexedTripleCollection"" .";

            File.WriteAllText("ConfigurationImports4-a.ttl", graph1);
            File.WriteAllText("ConfigurationImports4-b.ttl", graph2);
            File.WriteAllText("ConfigurationImports4-c.ttl", graph3);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImports4-a.ttl");

            TestTools.ShowGraph(g);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [TestMethod]
        public void ConfigurationImportsCircular1()
        {
            String graph1 = @"[] <http://www.dotnetrdf.org/configuration#imports> ""ConfigurationImportsCircular1-b.ttl"" . ";
            String graph2 = @"[] <http://www.dotnetrdf.org/configuration#imports> ""ConfigurationImportsCircular1-a.ttl"" . ";

            File.WriteAllText("ConfigurationImportsCircular1-a.ttl", graph1);
            File.WriteAllText("ConfigurationImportsCircular1-b.ttl", graph2);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImportsCircular1-a.ttl");
            Assert.AreEqual(2, g.Triples.Count);
        }
    }
}
