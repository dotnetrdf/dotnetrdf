using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.Storage
{
    [Collection("Virtuoso")]
    public class VirtuosoConnectorTest : VirtuosoTestBase
    {
        public VirtuosoConnectorTest(ITestOutputHelper output)
        {
        }

        public VirtuosoConnector GetConnector()
        {
            return GetConnection() as VirtuosoConnector;
        }

        public static VirtuosoConnectorBase GetConnection()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseVirtuoso),
                "Test Config marks Virtuoso as unavailable, test cannot be run");
            return new VirtuosoConnector(TestConfigManager.GetSetting(TestConfigManager.VirtuosoServer),
                TestConfigManager.GetSettingAsInt(TestConfigManager.VirtuosoPort),
                TestConfigManager.GetSetting(TestConfigManager.VirtuosoDatabase),
                TestConfigManager.GetSetting(TestConfigManager.VirtuosoUser),
                TestConfigManager.GetSetting(TestConfigManager.VirtuosoPassword));
        }


        [SkippableFact]
        public void TransactionTest()
        {
            VirtuosoConnector connector = GetConnector();
            connector.Begin();
            connector.Commit();
        }

        [SkippableFact]
        public void TestUpdateGraphTransactionIsolation()
        {
            using VirtuosoConnector connector1 = GetConnector();
            using VirtuosoConnector connector2 = GetConnector();

            var graphUri = new Uri("http://localhost/VirtuosoTest");
            var query = "SELECT * WHERE { GRAPH ?g { <http://example.org/foo> ?p ?o .} }";
            var testData = new Graph(graphUri);
            FileLoader.Load(testData, Path.Combine("resources", "MergePart1.ttl"));
            connector1.SaveGraph(testData);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            var t = new Triple(
                new UriNode(new Uri("http://example.org/foo")),
                new UriNode(new Uri("http://example.org/p")),
                new LiteralNode("o", true));
            connector1.Begin();
            connector1.UpdateGraph(graphUri, new List<Triple> { t }, null);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            connector1.Commit();

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().HaveCount(1);
        }

        [SkippableFact]
        public void TestUpdateGraphRollback()
        {
            using VirtuosoConnector connector1 = GetConnector();
            using VirtuosoConnector connector2 = GetConnector();

            var graphUri = new Uri("http://localhost/VirtuosoTest");
            var query = "SELECT * WHERE { GRAPH ?g { <http://example.org/foo> ?p ?o .} }";
            var testData = new Graph(graphUri);
            FileLoader.Load(testData, Path.Combine("resources", "MergePart1.ttl"));
            connector1.SaveGraph(testData);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            var t = new Triple(
                new UriNode(new Uri("http://example.org/foo")),
                new UriNode(new Uri("http://example.org/p")),
                new LiteralNode("o", true));
            connector1.Begin();
            connector1.UpdateGraph(graphUri, new List<Triple> { t }, null);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            connector1.Rollback();

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

        }

        [SkippableFact]
        public void TestSparqlUpdateTransactionIsolation()
        {
            using VirtuosoConnector connector1 = GetConnector();
            using VirtuosoConnector connector2 = GetConnector();

            var graphUri = new Uri("http://localhost/VirtuosoTest");
            var query = "SELECT * WHERE { GRAPH ?g { <http://example.org/foo> ?p ?o .} }";
            var testData = new Graph(graphUri);
            FileLoader.Load(testData, Path.Combine("resources", "MergePart1.ttl"));
            connector1.SaveGraph(testData);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            var t = new Triple(
                new UriNode(new Uri("http://example.org/foo")),
                new UriNode(new Uri("http://example.org/p")),
                new LiteralNode("o", true));
            connector1.Begin();
            connector1.Update("INSERT DATA { GRAPH <http://localhost/VirtuosoTest> { <http://example.org/foo> <http://example.org/p> \"o\" .}}");

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            connector1.Commit();

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().NotBeEmpty();
        }

        [SkippableFact]
        public void TestSparqlUpdateRollback()
        {
            using VirtuosoConnector connector1 = GetConnector();
            using VirtuosoConnector connector2 = GetConnector();

            var graphUri = new Uri("http://localhost/VirtuosoTest");
            var query = "SELECT * WHERE { GRAPH ?g { <http://example.org/foo> ?p ?o .} }";
            var testData = new Graph(graphUri);
            FileLoader.Load(testData, Path.Combine("resources", "MergePart1.ttl"));
            connector1.SaveGraph(testData);

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            var t = new Triple(
                new UriNode(new Uri("http://example.org/foo")),
                new UriNode(new Uri("http://example.org/p")),
                new LiteralNode("o", true));
            connector1.Begin();
            connector1.Update("INSERT DATA { GRAPH <http://localhost/VirtuosoTest> { <http://example.org/foo> <http://example.org/p> \"o\" .}}");

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();

            connector1.Rollback();

            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().BeEmpty();
        }

        [SkippableFact]
        public void SaveGraphWillAutoCommit()
        {
            using VirtuosoConnector connector1 = GetConnector();
            using VirtuosoConnector connector2 = GetConnector();

            var graphUri = new Uri("http://localhost/VirtuosoTest");
            var query = "SELECT * WHERE { GRAPH ?g { <http://example.org/a/a> ?p ?o .} }";
            var testData = new Graph(graphUri);
            FileLoader.Load(testData, Path.Combine("resources", "MergePart1.ttl"));
            connector1.Begin();
            connector1.SaveGraph(testData);
            
            // Even though Commmit has not been called on connector1, Virtuoso's row autocommit feature means the graph data is committed and queryable
            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().NotBeEmpty();

            // As the data was committed a rollback has no effect
            connector1.Rollback();
            connector2.Query(query).Should().BeOfType<SparqlResultSet>().Which.Should().NotBeEmpty();
        }
    }

}