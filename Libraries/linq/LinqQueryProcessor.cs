using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Storage;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// A LINQ Query Processor is a wrapper around a normal SPARQL Query processor designed to dump the SPARQL Results into an <see cref="ILinqResultsSink">ILinqResultsSink</see>
    /// </summary>
    public class LinqQueryProcessor : ISparqlQueryProcessor
    {
        private ISparqlQueryProcessor _underlyingProcessor;

        public LinqQueryProcessor(ISparqlQueryProcessor processor)
        {
            this._underlyingProcessor = processor;
        }

        public LinqQueryProcessor(IInMemoryQueryableStore store)
            : this(new LeviathanQueryProcessor(store)) { }

        public LinqQueryProcessor(Uri endpointUri)
            : this(new RemoteQueryProcessor(new SparqlRemoteEndpoint(endpointUri))) { }

        public LinqQueryProcessor(SparqlRemoteEndpoint endpoint)
            : this(new RemoteQueryProcessor(endpoint)) { }

        public LinqQueryProcessor(IQueryableGenericIOManager manager)
            : this(new GenericQueryProcessor(manager)) { }

        public LinqQueryProcessor(INativelyQueryableStore store)
            : this(new SimpleQueryProcessor(store)) { }

        public object ProcessQuery(SparqlQuery query)
        {
            return this._underlyingProcessor.ProcessQuery(query);
        }

        public void ProcessQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, SparqlQuery query)
        {
            this._underlyingProcessor.ProcessQuery(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Processes the SPARQL Query using the underlying processor and then
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sink"></param>
        public void Run(SparqlQuery query, ILinqResultsSink sink)
        {
            Object results = this.ProcessQuery(query);
            if (results is SparqlResultSet)
            {
                sink.Fill((SparqlResultSet)results);
            }
            else if (results is BaseMultiset)
            {
                sink.Fill((BaseMultiset)results);
            }
            else
            {
                throw new LinqToRdfException("Underlying Query Processor returned unexpected results of type " + results.GetType().ToString());
            }
        }
    }
}
