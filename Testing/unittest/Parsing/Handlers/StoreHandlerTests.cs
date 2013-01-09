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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing.Handlers
{
    [TestClass]
    public class StoreHandlerTests
    {
        private void EnsureTestData(String testFile)
        {
            if (!File.Exists(testFile))
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                Graph h = new Graph();
                h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
                store.Add(h);

                store.SaveToFile(testFile);
            }
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void ParsingStoreHandlerBadInstantiation()
        {
            StoreHandler handler = new StoreHandler(null);
        }

        #region NQuads Tests

        [TestMethod]
        public void ParsingStoreHandlerNQuadsImplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerNQuadsImplicitActual));
        }

        private void ParsingStoreHandlerNQuadsImplicitActual()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();

            NQuadsParser parser = new NQuadsParser();
            parser.Load(store, "test.nq");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerNQuadsExplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerNQuadsExplicitActual));
        }

        private void ParsingStoreHandlerNQuadsExplicitActual()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();

            NQuadsParser parser = new NQuadsParser();
            parser.Load(new StoreHandler(store), "test.nq");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerNQuadsCounting()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerNQuadsCountingActual));
        }

        private void ParsingStoreHandlerNQuadsCountingActual()
        {
            this.EnsureTestData("test.nq");

            NQuadsParser parser = new NQuadsParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.nq");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.AreEqual(2, counter.GraphCount, "Expected 2 Graphs to be counted");
            Assert.AreEqual(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount, "Expected Triple Count to be sum of Triple Counts in two input Graphs");
        }

        [TestMethod]
        public void ParsingFileLoaderStoreHandlerCounting()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingFileLoaderStoreHandlerCountingActual));
        }

        private void ParsingFileLoaderStoreHandlerCountingActual()
        {
            this.EnsureTestData("test.nq");

            StoreCountHandler counter = new StoreCountHandler();
            FileLoader.LoadDataset(counter, "test.nq");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.AreEqual(2, counter.GraphCount, "Expected 2 Graphs to be counted");
            Assert.AreEqual(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount, "Expected Triple Count to be sum of Triple Counts in two input Graphs");
        }

        [TestMethod]
        public void ParsingFileLoaderStoreHandlerExplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingFileLoaderStoreHandlerExplicitActual));
        }

        private void ParsingFileLoaderStoreHandlerExplicitActual()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();
            FileLoader.LoadDataset(new StoreHandler(store), "test.nq");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        #endregion

        #region TriG Tests

        [TestMethod]
        public void ParsingStoreHandlerTriGImplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriGImplicitActual));
        }

        private void ParsingStoreHandlerTriGImplicitActual()
        {
            this.EnsureTestData("test.trig");

            TripleStore store = new TripleStore();

            TriGParser parser = new TriGParser();
            parser.Load(store, "test.trig");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerTriGExplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriGExplicitActual));
        }

        private void ParsingStoreHandlerTriGExplicitActual()
        {
            this.EnsureTestData("test.trig");

            TripleStore store = new TripleStore();

            TriGParser parser = new TriGParser();
            parser.Load(new StoreHandler(store), "test.trig");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerTriGCounting()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriGCountingActual));
        }

        private void ParsingStoreHandlerTriGCountingActual()
        {
            this.EnsureTestData("test.trig");

            TriGParser parser = new TriGParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.trig");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.AreEqual(2, counter.GraphCount, "Expected 2 Graphs to be counted");
            Assert.AreEqual(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount, "Expected Triple Count to be sum of Triple Counts in two input Graphs");
        }

        #endregion

        #region TriX Tests

        [TestMethod]
        public void ParsingStoreHandlerTriXImplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriXImplicitActual));
        }

        private void ParsingStoreHandlerTriXImplicitActual()
        {
            this.EnsureTestData("test.xml");

            TripleStore store = new TripleStore();

            TriXParser parser = new TriXParser();
            parser.Load(store, "test.xml");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerTriXExplicit()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriXExplicitActual));
        }

        private void ParsingStoreHandlerTriXExplicitActual()
        {
            this.EnsureTestData("test.xml");

            TripleStore store = new TripleStore();

            TriXParser parser = new TriXParser();
            parser.Load(new StoreHandler(store), "test.xml");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/configuration#")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://www.dotnetrdf.org/configuration#")];
            Assert.AreEqual(configOrig, config, "Configuration Vocab Graphs should have been equal");

            Assert.IsTrue(store.HasGraph(new Uri("http://www.dotnetrdf.org/leviathan#")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://www.dotnetrdf.org/leviathan#")];
            Assert.AreEqual(lvnOrig, lvn, "Leviathan Function Library Graphs should have been equal");

        }

        [TestMethod]
        public void ParsingStoreHandlerTriXCounting()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerTriXCountingActual));
        }

        private void ParsingStoreHandlerTriXCountingActual()
        {
            this.EnsureTestData("test.xml");

            TriXParser parser = new TriXParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.xml");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.AreEqual(2, counter.GraphCount, "Expected 2 Graphs to be counted");
            Assert.AreEqual(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount, "Expected Triple Count to be sum of Triple Counts in two input Graphs");
        }

        #endregion
    }
}
