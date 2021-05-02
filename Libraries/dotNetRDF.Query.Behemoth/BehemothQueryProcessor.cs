using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using Graph = VDS.RDF.Graph;

namespace dotNetRDF.Query.Behemoth
{
    public class BehemothQueryProcessor : ISparqlQueryProcessor
    {
        private readonly ISparqlDataset _dataset;
        public BehemothQueryProcessor(ISparqlDataset dataset)
        {
            _dataset = dataset;
        }

        public object ProcessQuery(SparqlQuery query)
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    var results = new SparqlResultSet();
                    ProcessQuery(null, new ResultSetHandler(results), query);
                    return results;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    IGraph g = new Graph();
                    ProcessQuery(new GraphHandler(g), null, query);
                    return g;
                default:
                    throw new RdfQueryException("Cannot process unknown query types");
            }
        }

        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            BehemothEvaluationContext context = GetContext(query);
            var builder = new BehemothBuilder();
            ISparqlAlgebra algebra = query.ToAlgebra(!query.IsOptimised);
            IEvaluationBlock rootBlock = builder.ProcessAlgebra(algebra, context);
            if (resultsHandler != null)
            {
                resultsHandler.StartResults();
                try
                {
                    if (query.QueryType == SparqlQueryType.Ask)
                    {
                        resultsHandler.HandleBooleanResult(rootBlock.Evaluate(Bindings.Empty).Any());
                    }
                    else
                    {
                        foreach (Bindings bindings in rootBlock.Evaluate(Bindings.Empty))
                        {
                            resultsHandler.HandleResult(bindings.AsSparqlResult());
                        }
                    }

                    resultsHandler.EndResults(true);
                }
                catch (Exception)
                {
                    resultsHandler.EndResults(false);
                    throw;
                }
            }

        }

        public void ProcessQuery(SparqlQuery query, GraphCallback rdfCallback, SparqlResultsCallback resultsCallback, object state)
        {
            throw new NotImplementedException();
        }

        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query,
            QueryCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public Task<object> ProcessQueryAsync(SparqlQuery query)
        {
            throw new NotImplementedException();
        }

        public Task ProcessQueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            throw new NotImplementedException();
        }

        private BehemothEvaluationContext GetContext(SparqlQuery query)
        {
            return new BehemothEvaluationContext(_dataset);
        }
    }
}
