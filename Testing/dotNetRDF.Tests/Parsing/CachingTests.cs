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
using VDS.RDF.XunitExtensions;
using System.Reflection;

namespace VDS.RDF.Parsing
{
    public class CachingTests
    {
        private static Uri test = new Uri("http://www.dotnetrdf.org/configuration#");

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingUriLoaderCache()
        {
            //Load the Graph
            Graph g = new Graph();
            UriLoader.Load(g, test);

            //Then reload the Graph which it should now come from the cache instead
            Graph h = new Graph();
            UriLoader.Load(h, test);

            Assert.Equal(g, h);
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingUriLoaderCustomCache()
        {
            String original = UriLoader.CacheDirectory;
            try
            {
                UriLoader.CacheDirectory = Path.GetDirectoryName(typeof(CachingTests).GetTypeInfo().Assembly.Location);

                this.ParsingUriLoaderCache();
            }
            finally
            {
                UriLoader.CacheDirectory = original;
            }
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingUriLoaderUriSantisation()
        {
            Uri a = new Uri(ConfigurationLoader.ClassTripleStore);
            Uri b = new Uri(ConfigurationLoader.ClassGraph);

            Console.WriteLine("URI A: " + a.AbsoluteUri + " is equivalent to " + Tools.StripUriFragment(a).AbsoluteUri);
            Console.WriteLine("URI B:" + b.AbsoluteUri + " is equivalent to " + Tools.StripUriFragment(b).AbsoluteUri);

            Assert.Equal(Tools.StripUriFragment(a).AbsoluteUri, Tools.StripUriFragment(b).AbsoluteUri);

            Graph g = new Graph();
            UriLoader.Load(g, a);

            Assert.True(UriLoader.IsCached(a), "Content should have been cached as a result of loading from the URI");

            Graph h = new Graph();
            UriLoader.Load(h, b);

            Assert.Equal(g, h);
        }

        [SkippableFact]
        public void ParsingUriLoaderResponseUriCaching()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                Options.UriLoaderTimeout = 45000;
                Uri soton = new Uri("http://dbpedia.org/resource/Southampton");
                Uri sotonPage = new Uri("http://dbpedia.org/page/Southampton.html");
                Uri sotonData = new Uri("http://dbpedia.org/data/Southampton.xml");

                Graph g = new Graph();
                UriLoader.Load(g, soton);

                Assert.True(UriLoader.IsCached(soton), "Resource URI should have been cached");
                Assert.True(UriLoader.IsCached(sotonData), "Data URI should have been cached");
                Assert.False(UriLoader.IsCached(sotonPage), "Page URI should not have been cached");
            }
            finally
            {
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }
    }
}
