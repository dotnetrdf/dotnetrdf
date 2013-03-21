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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Configuration
{
    [TestClass]
    public class ConfigurationLoaderInstanceTests
    {
        [TestMethod]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromBlankNode()
        {
            // given
            const string graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>("a");

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is ThreadSafeTripleCollection);
        }

        [TestMethod]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromUri()
        {
            // given
            const string graph = ConfigLookupTests.Prefixes + @"
@base <http://example.com/> .

<collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <indexedCollection> .
<indexedCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/indexedCollection"));

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is TreeIndexedTripleCollection);
        }

        [TestMethod]
        public void CanCreateInstanceFromGraphFileAndLoadObjectFromUri()
        {
            // given
            const string graph = ConfigLookupTests.Prefixes + @"
@base <http://example.com/> .

<collection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <indexedCollection> .
<indexedCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            File.WriteAllText("configuration.ttl", graph);

            // when
            var configuration = new ConfigurationLoader("configuration.ttl");
            var collection = configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/indexedCollection"));

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is TreeIndexedTripleCollection);
        }
    }
}
