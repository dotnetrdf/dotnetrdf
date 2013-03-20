using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);
            
            // when
            var configuration = new ConfigurationLoader(g);
            var collection = configuration.LoadObject<TripleCollection>("a");

            // then
            Assert.IsNotNull(collection);
        }
    }
}
