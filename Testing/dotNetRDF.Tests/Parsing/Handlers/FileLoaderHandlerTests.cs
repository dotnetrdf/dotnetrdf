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

using System.IO;
using Xunit;

namespace VDS.RDF.Parsing.Handlers
{

    public class FileLoaderHandlerTests
    {
        private const string TestDataFile = "resources\\file_loader_handler_tests_temp.ttl";

        public FileLoaderHandlerTests()
        {
            if (File.Exists(TestDataFile))
            {
                File.Delete(TestDataFile);
            }
            
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(TestDataFile);
        }

        [Fact]
        public void ParsingFileLoaderGraphHandlerImplicitTurtle()
        {
            Graph g = new Graph();
            FileLoader.Load(g, TestDataFile);

            TestTools.ShowGraph(g);
            Assert.False(g.IsEmpty, "Graph should not be empty");
        }

        [Fact]
        public void ParsingFileLoaderGraphHandlerExplicitTurtle()
        {
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            FileLoader.Load(handler, TestDataFile);

            TestTools.ShowGraph(g);
            Assert.False(g.IsEmpty, "Graph should not be empty");
        }

        [Fact]
        public void ParsingFileLoaderCountHandlerTurtle()
        {
            Graph orig = new Graph();
            orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            CountHandler handler = new CountHandler();
            FileLoader.Load(handler, TestDataFile);

            Assert.Equal(orig.Triples.Count, handler.Count);
        }
    }
}
