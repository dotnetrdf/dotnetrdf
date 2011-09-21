using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
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

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
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

        [TestMethod]
        public void FullTextConfigSchemaDefault()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.IsTrue(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.IsTrue(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");
        }

        [TestMethod]
        public void FullTextConfigAnalyzerLuceneStandard()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");
        }

        [TestMethod]
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
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");
        }

        [TestMethod]
        public void FullTextConfigIndexLuceneRAM()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(obj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.IsTrue(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");
        }

        [TestMethod]
        public void FullTextConfigIndexLuceneFS()
        {
            IGraph g = this.GetBaseGraph();
            INode obj = g.CreateBlankNode();
            g.Assert(obj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(obj, g.CreateUriNode(ConfigurationLoader.PropertyType), g.CreateLiteralNode("Lucene.Net.Store.FSDirectory, Lucene.Net"));
            g.Assert(obj, g.CreateUriNode(ConfigurationLoader.PropertyFromFile), g.CreateLiteralNode("test"));
            System.IO.Directory.CreateDirectory("test");

            TestTools.ShowGraph(g);

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, obj);
            Assert.IsTrue(temp is FSDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");
        }

        [TestMethod]
        public void FullTextConfigIndexerLuceneSubjects()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.IsTrue(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.IsTrue(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.IsTrue(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LuceneSubjectsIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.IsTrue(temp is LuceneSubjectsIndexer, "Should have returned a LuceneSubjectsIndexer Instance");
            Assert.IsTrue(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [TestMethod]
        public void FullTextConfigIndexerLuceneObjects()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.IsTrue(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.IsTrue(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.IsTrue(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.IsTrue(temp is LuceneObjectsIndexer, "Should have returned a LuceneObjectsIndexer Instance");
            Assert.IsTrue(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [TestMethod]
        public void FullTextConfigIndexerLucenePredicates()
        {
            //Add and test the Index Configuration
            IGraph g = this.GetBaseGraph();
            INode indexObj = g.CreateBlankNode();
            g.Assert(indexObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Index"));
            g.Assert(indexObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Store.RAMDirectory, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            Object temp = ConfigurationLoader.LoadObject(g, indexObj);
            Assert.IsTrue(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.IsTrue(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.IsTrue(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Indexer config which ties all the above together
            INode indexerObj = g.CreateBlankNode();
            g.Assert(indexerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Indexer"));
            g.Assert(indexerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Indexing.Lucene.LucenePredicatesIndexer, dotNetRDF.Query.FullText"));
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(indexerObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, indexerObj);
            Assert.IsTrue(temp is LucenePredicatesIndexer, "Should have returned a LucenePredicatesIndexer Instance");
            Assert.IsTrue(temp is IFullTextIndexer, "Should have returned a IFullTextIndexer Instance");
        }

        [TestMethod]
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
            Assert.IsTrue(temp is RAMDirectory, "Should have returned a RAMDirectory Instance");
            Assert.IsTrue(temp is Directory, "Should have returned a Directory Instance");

            //Add and Test the analyzer Config
            INode analyzerObj = g.CreateBlankNode();
            g.Assert(analyzerObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Analyzer"));
            g.Assert(analyzerObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, analyzerObj);
            Assert.IsTrue(temp is StandardAnalyzer, "Should have returned a StandardAnalyzer Instance");
            Assert.IsTrue(temp is Analyzer, "Should have returned an Analyzer Instance");

            //Add and Test the schema config
            INode schemaObj = g.CreateBlankNode();
            g.Assert(schemaObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Schema"));
            g.Assert(schemaObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText"));

            ConfigurationLoader.AddObjectFactory(this._factory);

            temp = ConfigurationLoader.LoadObject(g, schemaObj);
            Assert.IsTrue(temp is DefaultIndexSchema, "Should have returned a DefaultIndexSchema Instance");
            Assert.IsTrue(temp is IFullTextIndexSchema, "Should have returned a IFullTextIndexSchema Instance");

            //Finally add the Searcher config which ties all the above together
            INode searcherObj = g.CreateBlankNode();
            g.Assert(searcherObj, g.CreateUriNode("rdf:type"), g.CreateUriNode("dnr-ft:Searcher"));
            g.Assert(searcherObj, g.CreateUriNode("dnr:type"), g.CreateLiteralNode("VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText"));
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:index"), indexObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:analyzer"), analyzerObj);
            g.Assert(searcherObj, g.CreateUriNode("dnr-ft:schema"), schemaObj);

            TestTools.ShowGraph(g);

            temp = ConfigurationLoader.LoadObject(g, searcherObj);
            Assert.IsTrue(temp is LuceneSearchProvider, "Should have returned a LuceneSearchProvider Instance");
            Assert.IsTrue(temp is IFullTextSearchProvider, "Should have returned a IFullTextSearchProvider Instance");
        }
    }
}
