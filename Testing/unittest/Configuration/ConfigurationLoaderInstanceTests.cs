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
using NUnit.Framework;

namespace VDS.RDF.Configuration
{
    [TestFixture]
    public class ConfigurationLoaderInstanceTests
    {
        private const string TestConfigGraph = ConfigLookupTests.Prefixes + @"
@base <http://example.com/> .

_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection <indexedCollection> .
<indexedCollection> a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

        [Test]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromBlankNode()
        {
            // given
            Graph g = new Graph();
            g.LoadFromString(TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>("a");

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is ThreadSafeTripleCollection);
        }

        [Test]
        public void CanCreateInstanceFromExistingGraphAndLoadObjectFromUri()
        {
            // given
            Graph g = new Graph();
            g.LoadFromString(TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/indexedCollection"));

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is TreeIndexedTripleCollection);
        }

        [Test]
        public void CanCreateInstanceFromGraphFileAndLoadObjectFromUri()
        {
            // given
            File.WriteAllText("configuration.ttl", TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader("configuration.ttl");
            var collection = configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/indexedCollection"));

            // then
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection is TreeIndexedTripleCollection);
        }

        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Resource <http://example.com/notSuchObject> was not found is configuration graph")]
        public void ShouldThrowWhenUriNodeIsNotFound()
        {
            // given
            File.WriteAllText("configuration.ttl", TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader("configuration.ttl");

            // then
            configuration.LoadObject<BaseTripleCollection>(new Uri("http://example.com/notSuchObject"));
        }

        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Resource _:store was not found is configuration graph")]
        public void ShouldThrowWhenBlankNodeIsNotFound()
        {
            // given
            File.WriteAllText("configuration.ttl", TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader("configuration.ttl");

            // then
            configuration.LoadObject<BaseTripleCollection>("store");
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void ShouldThrowWhenTryingToLoadWrongType()
        {
            // given
            File.WriteAllText("configuration.ttl", TestConfigGraph);

            // when
            var configuration = new ConfigurationLoader("configuration.ttl");

            // then
            configuration.LoadObject<TripleStore>(new Uri("http://example.com/indexedCollection"));
        } 
    }
}
