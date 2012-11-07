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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class CachingTests
    {
        private static Uri test = new Uri("http://www.dotnetrdf.org/configuration#");

        [TestMethod]
        public void ParsingUriLoaderCache()
        {
            //Load the Graph
            Graph g = new Graph();
            UriLoader.Load(g, test);

            //Then reload the Graph which it should now come from the cache instead
            Graph h = new Graph();
            UriLoader.Load(h, test);

            Assert.AreEqual(g, h);
        }

        [TestMethod]
        public void ParsingUriLoaderCustomCache()
        {
            String original = UriLoader.CacheDirectory;
            try
            {
                UriLoader.CacheDirectory = Environment.CurrentDirectory;

                this.ParsingUriLoaderCache();
            }
            finally
            {
                UriLoader.CacheDirectory = original;
            }
        }

        [TestMethod]
        public void ParsingUriLoaderUriSantisation()
        {
            Uri a = new Uri(ConfigurationLoader.ClassTripleStore);
            Uri b = new Uri(ConfigurationLoader.ClassGraph);

            Console.WriteLine("URI A: " + a.AbsoluteUri + " is equivalent to " + Tools.StripUriFragment(a).AbsoluteUri);
            Console.WriteLine("URI B:" + b.AbsoluteUri + " is equivalent to " + Tools.StripUriFragment(b).AbsoluteUri);

            Assert.AreEqual(Tools.StripUriFragment(a).AbsoluteUri, Tools.StripUriFragment(b).AbsoluteUri, "URIs stripped of their Fragment IDs should have been equal");

            Graph g = new Graph();
            UriLoader.Load(g, a);

            Assert.IsTrue(UriLoader.IsCached(a), "Content should have been cached as a result of loading from the URI");

            Graph h = new Graph();
            UriLoader.Load(h, b);

            Assert.AreEqual(g, h, "Two Graphs should be equal since they come from the same URI");
        }

        [TestMethod]
        public void ParsingUriLoaderResponseUriCaching()
        {
            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                Options.UriLoaderTimeout = 45000;
                Uri soton = new Uri("http://dbpedia.org/resource/Southampton");
                Uri sotonPage = new Uri("http://dbpedia.org/page/Southampton.html");
                Uri sotonData = new Uri("http://dbpedia.org/data/Southampton.xml");

                Graph g = new Graph();
                UriLoader.Load(g, soton);

                Assert.IsTrue(UriLoader.IsCached(soton), "Resource URI should have been cached");
                Assert.IsTrue(UriLoader.IsCached(sotonData), "Data URI should have been cached");
                Assert.IsFalse(UriLoader.IsCached(sotonPage), "Page URI should not have been cached");
            }
            finally
            {
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }
    }
}
