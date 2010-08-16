using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test
{
    [TestClass()]
    public class CachingTests
    {
        private static Uri test = new Uri("http://api.talis.com/stores/rvesse-dev1/meta?about=" + Uri.EscapeDataString("http://example.org/vehicles/FordFiesta"));

        [TestMethod()]
        public void UriLoaderCacheTest()
        {
            //Load the Graph
            Graph g = new Graph();
            UriLoader.Load(g, test);

            //Then reload the Graph which it should now come from the cache instead
            Graph h = new Graph();
            UriLoader.Load(h, test);

            Assert.AreEqual(g, h);
        }

        [TestMethod()]
        public void UriLoaderCustomCacheTest()
        {
            UriLoader.CacheDirectory = "E:\\Cache";

            this.UriLoaderCacheTest();
        }
    }
}
