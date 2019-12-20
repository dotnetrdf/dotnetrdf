using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.Query
{
    public class LeviathanAsyncQueryTests
    {
        private InMemoryDataset _dataset;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();
        private LeviathanQueryProcessor _processor;
        private NodeFactory _factory = new NodeFactory();
        private readonly ITestOutputHelper _output;

        public LeviathanAsyncQueryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void EnsureTestData()
        {
            if (_dataset == null)
            {
                _dataset = new InMemoryDataset();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                _dataset.AddGraph(g);
                _dataset.SetDefaultGraph(g.BaseUri);

                _processor = new LeviathanQueryProcessor(_dataset);
            }
        }

        [Fact]
        public void TestAsyncQueryResultSetCallback()
        {
            var dataset = new InMemoryDataset();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            dataset.AddGraph(g);
            dataset.SetDefaultGraph(g.BaseUri);

            var processor = new LeviathanQueryProcessor(dataset);

            var wait = new AutoResetEvent(false);
            var query = _parser.ParseFromString(
                "SELECT * WHERE { ?instance a ?class }");
            var callbackInvoked = false;
            int resultCount = 0;
            processor.ProcessQuery(query, null, (resultSet, state) =>
            {
                try
                {
                    Assert.IsNotType<AsyncError>(state);
                    Assert.Equal("some state", state);
                    Assert.NotNull(resultSet);
                    Assert.NotEmpty(resultSet.Results);
                }
                finally
                {
                    resultCount = resultSet.Count;
                    callbackInvoked = true;
                    wait.Set();
                }
            }, "some state");
            wait.WaitOne();
            Assert.True(callbackInvoked);
            Assert.True(resultCount > 0);
        }
    }
}
