using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF
{
    public static class SparqlExtensions
    {
        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, String sparqlQuery)
        {
            //Due to change in default graph behaviour ensure that we associate this graph as the default graph of the dataset
            InMemoryDataset ds = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(sparqlQuery);
            return processor.ProcessQuery(q);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            InMemoryDataset ds = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(sparqlQuery);
            processor.ProcessQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, SparqlParameterizedString sparqlQuery)
        {
            return g.ExecuteQuery(sparqlQuery.ToString());
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlParameterizedString sparqlQuery)
        {
            g.ExecuteQuery(rdfHandler, resultsHandler, sparqlQuery.ToString());
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public static Object ExecuteQuery(this IGraph g, SparqlQuery query)
        {
            InMemoryDataset ds = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);
            return processor.ProcessQuery(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on a Graph handling the results using the handlers provided
        /// </summary>
        /// <param name="g">Graph to query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        public static void ExecuteQuery(this IGraph g, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            InMemoryDataset ds = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(ds);
            processor.ProcessQuery(rdfHandler, resultsHandler, query);
        }
    }
}
