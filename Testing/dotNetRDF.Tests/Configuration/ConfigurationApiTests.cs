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
using System.Text;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Configuration
{

    public class ConfigurationApiTests
    {
        [Fact]
        public void ConfigurationCircularReference()
        {
            String graph = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:fromGraph _:b .
_:b a dnr:Graph ;
  dnr:fromGraph _:a .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            Assert.Throws<DotNetRdfConfigurationException>(() => ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")));
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [Fact]
        public void ConfigurationImports4()
        {
            //Repeated Imports
            String graph1 = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:Graph ;
  dnr:usingTripleCollection <ex:collection> .

[] dnr:imports ""ConfigurationImports4-b.ttl"" , ""ConfigurationImports4-c.ttl"", ""ConfigurationImports4-c.ttl"" . ";

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
            Assert.NotNull(result);
            Assert.Equal(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [Fact]
        public void ConfigurationImportsCircular1()
        {
            String graph1 = @"[] <http://www.dotnetrdf.org/configuration#imports> ""ConfigurationImportsCircular1-b.ttl"" . ";
            String graph2 = @"[] <http://www.dotnetrdf.org/configuration#imports> ""ConfigurationImportsCircular1-a.ttl"" . ";

            File.WriteAllText("ConfigurationImportsCircular1-a.ttl", graph1);
            File.WriteAllText("ConfigurationImportsCircular1-b.ttl", graph2);

            IGraph g = ConfigurationLoader.LoadConfiguration("ConfigurationImportsCircular1-a.ttl");
            Assert.Equal(2, g.Triples.Count);
        }
    }
}
