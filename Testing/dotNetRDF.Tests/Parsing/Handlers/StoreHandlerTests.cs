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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing.Handlers
{

    public class StoreHandlerTests
    {

        private void EnsureTestData(String testFile)
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://graphs/1");
            store.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://graphs/2");
            store.Add(h);

            store.SaveToFile(testFile);
        }

        [Fact]
        public void ParsingStoreHandlerBadInstantiation()
        {
            Assert.Throws<ArgumentNullException>(() => new StoreHandler(null));
        }

        #region NQuads Tests

        [Fact]
        public void ParsingStoreHandlerNQuadsImplicit()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();

            NQuadsParser parser = new NQuadsParser();
            parser.Load(store, "test.nq");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerNQuadsExplicit()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();

            NQuadsParser parser = new NQuadsParser();
            parser.Load(new StoreHandler(store), "test.nq");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerNQuadsCounting()
        {
            this.EnsureTestData("test.nq");

            NQuadsParser parser = new NQuadsParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.nq");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.Equal(2, counter.GraphCount);
            Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
        }

        [Fact]
        public void ParsingFileLoaderStoreHandlerCounting()
        {
            this.EnsureTestData("test.nq");

            StoreCountHandler counter = new StoreCountHandler();
            FileLoader.LoadDataset(counter, "test.nq");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.Equal(2, counter.GraphCount);
            Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
        }

        [Fact]
        public void ParsingFileLoaderStoreHandlerExplicit()
        {
            this.EnsureTestData("test.nq");

            TripleStore store = new TripleStore();
            FileLoader.LoadDataset(new StoreHandler(store), "test.nq");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }
        #endregion

        #region TriG Tests

        [Fact]
        public void ParsingStoreHandlerTriGImplicit()
        {
            this.EnsureTestData("test.trig");

            TripleStore store = new TripleStore();

            TriGParser parser = new TriGParser();
            parser.Load(store, "test.trig");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerTriGExplicit()
        {
            this.EnsureTestData("test.trig");

            TripleStore store = new TripleStore();

            TriGParser parser = new TriGParser();
            parser.Load(new StoreHandler(store), "test.trig");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerTriGCounting()
        {
            this.EnsureTestData("test.trig");

            TriGParser parser = new TriGParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.trig");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.Equal(2, counter.GraphCount);
            Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
        }

        #endregion

        #region TriX Tests

        [Fact]
        public void ParsingStoreHandlerTriXImplicit()
        {
            this.EnsureTestData("test.xml");

            TripleStore store = new TripleStore();

            TriXParser parser = new TriXParser();
            parser.Load(store, "test.xml");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerTriXExplicit()
        {
            this.EnsureTestData("test.xml");

            TripleStore store = new TripleStore();

            TriXParser parser = new TriXParser();
            parser.Load(new StoreHandler(store), "test.xml");

            Assert.True(store.HasGraph(new Uri("http://graphs/1")), "Configuration Vocab Graph should have been parsed from Dataset");
            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            IGraph config = store[new Uri("http://graphs/1")];
            Assert.Equal(configOrig, config);

            Assert.True(store.HasGraph(new Uri("http://graphs/2")), "Leviathan Function Library Graph should have been parsed from Dataset");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            IGraph lvn = store[new Uri("http://graphs/2")];
            Assert.Equal(lvnOrig, lvn);

        }

        [Fact]
        public void ParsingStoreHandlerTriXCounting()
        {
            this.EnsureTestData("test.xml");

            TriXParser parser = new TriXParser();
            StoreCountHandler counter = new StoreCountHandler();
            parser.Load(counter, "test.xml");

            Graph configOrig = new Graph();
            configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Graph lvnOrig = new Graph();
            lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

            Assert.Equal(2, counter.GraphCount);
            Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
        }

        #endregion
    }
}
