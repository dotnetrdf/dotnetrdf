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
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;

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
    }
}
