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

#if !NO_FULLTEXT

using System;
using System.Text;
using System.Collections.Generic;
using DirInfo = System.IO.DirectoryInfo;
using System.Linq;
using System.Reflection;
using Xunit;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.FullText
{

    [Collection("FullText")]
    public class FullTextConfigTests
    {
        private FullTextObjectFactory _factory = new FullTextObjectFactory();

        private IGraph GetBaseGraph()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            g.NamespaceMap.AddNamespace("dnr-ft", new Uri(FullTextHelper.FullTextConfigurationNamespace));

            return g;
        }

        [Fact]
        public void FullTextConfigSchemaDefault()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");
        }

        [Fact]
        public void FullTextConfigAnalyzerLuceneStandard()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");
        }

        [Fact]
        public void FullTextConfigAnalyzerLuceneStandardWithVersion()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));
            g.Assert(obj, g.CreateUriNode("dnr-ft:version"), (2400).ToLiteral(g));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");
        }

        [Fact]
        public void FullTextConfigIndexLuceneRAM()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");
        }

        [Fact]
        public void FullTextConfigIndexLuceneFS()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(obj, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), g.CreateLiteralNode("Lucene.Net.Store.FSDirectory, Lucene.Net"));
            g.Assert(obj, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile)), g.CreateLiteralNode("test"));
            System.IO.Directory.CreateDirectory("test");

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.True(temp is FSDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");
        }

        [Fact]
        public void FullTextConfigIndexerLuceneSubjects()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LuceneSubjectsIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.True(temp is LuceneSubjectsIndexer, "Should have returned a LuceneSubjectsIndexer Instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [Fact]
        public void FullTextConfigIndexerLuceneObjects()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.True(temp is LuceneObjectsIndexer, "Should have returned a LuceneObjectsIndexer Instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [Fact]
        public void FullTextConfigIndexerLucenePredicates()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LucenePredicatesIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.True(temp is LucenePredicatesIndexer, "Should have returned a LucenePredicatesIndexer Instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [Fact]
        public void FullTextConfigSearchProviderLucene()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));
            g.Assert(indexObj, g.CreateUriNode("dnr-ft:ensureIndex"), (true).ToLiteral(g));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.True(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.True(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Searcher config which ties all the above together
            INode searcherObj = g.CreateBlankNode();
            g.Assert(searcherObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Searcher"));
            g.Assert(searcherObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText"));
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, searcherObj);
            Assert.True(temp is LuceneSearchProvider, "Should have returned a LuceneSearchProvider Instance");
            Assert.True(temp is IFullTextSearchProvider, "Should have returned a IFullTextSearchProvider Instance");
        }

        [Fact]
        public void FullTextConfigSearchProviderLuceneWithBuildIndex()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));
            g.Assert(indexObj, g.CreateUriNode("dnr-ft:ensureIndex"), (true).ToLiteral(g));

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            //Add the Searcher config which ties all the above together
            INode searcherObj = g.CreateBlankNode();
            g.Assert(searcherObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Searcher"));
            g.Assert(searcherObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText"));
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            //Now add the Graph we want to get auto-indexed
            INode graphObj = g.CreateBlankNode();
            g.Assert(graphObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr:Graph"));
            g.Assert(graphObj, g.CreateUriNode("dnr:fromEmbedded"), g.CreateLiteralNode("VDS.RDF.Configuration.configuration.ttl"));

            //Then add the Indexer for use by the auto-indexing  
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LuceneSubjectsIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            //Finally add the properties to indicate we want auto-indexing and what to index
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:buildIndexFor"), graphObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:buildIndexWith"), indexerObj);

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);
            Object temp = ConfigurationLoader.LoadObject(g, searcherObj);
            Assert.True(temp is LuceneSearchProvider, "Should have returned a LuceneSearchProvider Instance");
            Assert.True(temp is IFullTextSearchProvider, "Should have returned a IFullTextSearchProvider Instance");

            //Finally check that auto-indexing has worked OK
            IFullTextSearchProvider provider = (IFullTextSearchProvider)temp;
            try
            {
                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http"))
                {
                    Console.WriteLine(result.Node.ToString() + " - " + result.Score.ToString());
                    i++;
                }

                Assert.True(i > 0, "Expected 1 or more result due to the auto-indexed data");
            }
            finally
            {
                provider.Dispose();
            }          
        }

        [Fact]
        public void FullTextConfigSerializeSchemaDefault()
        {
            DefaultIndexSchema schema = new DefaultIndexSchema();
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            schema.SerializeConfiguration(context);

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema instance");
            Assert.True(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema instance");
        }

        [Fact]
        public void FullTextConfigSerializeIndexLuceneRAM()
        {
            RAMDirectory directory = new RAMDirectory();
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            directory.SerializeConfiguration(context);

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is RAMDirectory, "Should have returned a RAMDirectory instance");
            Assert.True(temp is Directory, "Should have returned a Directory instance");
        }

        [Fact]
        public void FullTextConfigSerializeIndexLuceneFS()
        {
            FSDirectory directory = FSDirectory.Open(new DirInfo("test"));
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            directory.SerializeConfiguration(context);
            directory.Dispose();

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is FSDirectory, "Should have returned a FSDirectory instance");
            Assert.True(temp is Directory, "Should have returned a Directory instance");
        }

        [Fact]
        public void FullTextConfigSerializeAnalyzerLuceneStandard()
        {
            StandardAnalyzer analyzer = new StandardAnalyzer(LuceneTestHarness.LuceneVersion);
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            analyzer.SerializeConfiguration(context);
            
            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer instance");
            Assert.True(temp is Analyzer, "Should have returned a Analyzer instance");
        }

        [Fact]
        public void FullTextConfigSerializeIndexerLuceneSubjects()
        {
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            indexer.SerializeConfiguration(context);
            indexer.Dispose();

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is LuceneSubjectsIndexer, "Should have returned a LuceneSubjectsIndexer instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer instance");
        }

        [Fact]
        public void FullTextConfigSerializeIndexerLuceneObjects()
        {
            LuceneObjectsIndexer indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            indexer.SerializeConfiguration(context);
            indexer.Dispose();

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is LuceneObjectsIndexer, "Should have returned a LuceneObjectsIndexer instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer instance");
        }

        [Fact]
        public void FullTextConfigSerializeIndexerLucenePredicates()
        {
            LucenePredicatesIndexer indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
            ConfigurationSerializationContext context = new ConfigurationSerializationContext();
            INode obj = context.Graph.CreateBlankNode();
            context.NextSubject = obj;
            indexer.SerializeConfiguration(context);
            indexer.Dispose();

            TestTools.ShowGraph(context.Graph);

            ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
            Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
            Assert.True(temp is LucenePredicatesIndexer, "Should have returned a LucenePredicatesIndexer instance");
            Assert.True(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer instance");
        }

        [Fact]
        public void FullTextConfigSerializeFullTextOptimiser()
        {
            try
            {
                IndexWriter writer = new IndexWriter(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
                writer.Dispose();

                FullTextOptimiser optimiser = new FullTextOptimiser(new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index, LuceneTestHarness.Schema));
                ConfigurationSerializationContext context = new ConfigurationSerializationContext();
                INode obj = context.Graph.CreateBlankNode();
                context.NextSubject = obj;
                optimiser.SerializeConfiguration(context);

                TestTools.ShowGraph(context.Graph);

                ConfigurationLoader.AutoConfigureObjectFactories(context.Graph);
                Object temp = ConfigurationLoader.LoadObject(context.Graph, obj);
                Assert.True(temp is FullTextOptimiser, "Should have returned a LucenePredicatesIndexer instance");
                Assert.True(temp is IAlgebraOptimiser, "Should have returned a IFullTextIndexer instance");
            }
            finally
            {
                LuceneTestHarness.Index.Dispose();
            }
        }
    }
}
#endif