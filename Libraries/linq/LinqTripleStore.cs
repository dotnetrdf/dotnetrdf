using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// A LINQ Triple Store is a wrapper for a Query Processor used to process the queries
    /// </summary>
    public class LinqTripleStore
    {
        /// <summary>
        /// Creates a new LINQ Triple Store
        /// </summary>
        /// <param name="queryProcessor">LINQ Query Processor</param>
        /// <param name="queryMethod">Query Method</param>
        protected LinqTripleStore(LinqQueryProcessor queryProcessor, LinqQueryMethod queryMethod)
        {
            QueryMethod = queryMethod;
            QueryProcessor = queryProcessor;
        }

        /// <summary>
        /// Creates a new LINQ Triple Store
        /// </summary>
        /// <param name="queryProcessor">LINQ Query Processor</param>
        /// <param name="updateProcessor">SPARQL Update Processor</param>
        /// <param name="queryMethod">Query Method</param>
        protected LinqTripleStore(LinqQueryProcessor queryProcessor, LinqUpdateProcessor updateProcessor, LinqQueryMethod queryMethod)
            : this(queryProcessor, queryMethod)
        {
            UpdateProcessor = updateProcessor;
        }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over an in-memory store
        /// </summary>
        /// <param name="localStore">In-memory store</param>
        /// <remarks>
        /// Changes can be persisted to the in-memory store but depending on the type of store used you may have to manually persist the store
        /// </remarks>
        public LinqTripleStore(IInMemoryQueryableStore localStore)
            : this(new LinqQueryProcessor(localStore), new LinqUpdateProcessor(localStore), LinqQueryMethod.InMemorySparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a remote SPARQL endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public LinqTripleStore(string endpointUri)
            : this(new LinqQueryProcessor(new Uri(endpointUri)), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a remote SPARQL endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public LinqTripleStore(Uri endpointUri)
            : this(new LinqQueryProcessor(endpointUri), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a remote SPARQL endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public LinqTripleStore(SparqlRemoteEndpoint endpoint)
            : this(new LinqQueryProcessor(endpoint), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a pair of remote SPARQL endpoints
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        public LinqTripleStore(String queryEndpoint, String updateEndpoint)
            : this(new LinqQueryProcessor(new Uri(queryEndpoint)), new LinqUpdateProcessor(updateEndpoint), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a pair of remote SPARQL endpoints
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        public LinqTripleStore(Uri queryEndpoint, Uri updateEndpoint)
            : this(new LinqQueryProcessor(queryEndpoint), new LinqUpdateProcessor(updateEndpoint), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a pair of remote SPARQL endpoints
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        public LinqTripleStore(SparqlRemoteEndpoint queryEndpoint, SparqlRemoteUpdateEndpoint updateEndpoint)
            : this(new LinqQueryProcessor(queryEndpoint), new LinqUpdateProcessor(updateEndpoint), LinqQueryMethod.RemoteSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
        /// </summary>
        /// <param name="manager">Manager</param>
        public LinqTripleStore(IQueryableGenericIOManager manager)
            : this(new LinqQueryProcessor(manager), LinqQueryMethod.GenericSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a <see cref="IUpdateableGenericIOManager">IUpdateableGenericIOManager</see>
        /// </summary>
        /// <param name="manager">Manager</param>
        public LinqTripleStore(IUpdateableGenericIOManager manager)
            : this(new LinqQueryProcessor(manager), new LinqUpdateProcessor(manager), LinqQueryMethod.GenericSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a Native store
        /// </summary>
        /// <param name="store">Native Store</param>
        public LinqTripleStore(INativelyQueryableStore store)
            : this(new LinqQueryProcessor(store), LinqQueryMethod.NativeSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a given SPARQL Query Processor
        /// </summary>
        /// <param name="queryProcessor">Query Processor</param>
        public LinqTripleStore(ISparqlQueryProcessor queryProcessor)
            : this(new LinqQueryProcessor(queryProcessor), LinqQueryMethod.CustomSparql) { }

        /// <summary>
        /// Creates a new LINQ Triple Store that operates over a given pair of SPARQL Processors
        /// </summary>
        /// <param name="queryProcessor">Query Processor</param>
        /// <param name="updateProcessor">Update Processor</param>
        public LinqTripleStore(ISparqlQueryProcessor queryProcessor, ISparqlUpdateProcessor updateProcessor)
            : this(new LinqQueryProcessor(queryProcessor), new LinqUpdateProcessor(updateProcessor), LinqQueryMethod.CustomSparql) { }

        /// <summary>
        /// Gets the Type of the Query
        /// </summary>
        public LinqQueryMethod QueryMethod { get; internal set; }

        /// <summary>
        /// Gets the LINQ Query Processor used to retrieve objects from the underlying store
        /// </summary>
        public LinqQueryProcessor QueryProcessor { get; internal set; }

        /// <summary>
        /// Gets the SPARQL Update Processor used to persist objects back to their underlying store
        /// </summary>
        public LinqUpdateProcessor UpdateProcessor { get; internal set; }

        /// <summary>
        /// Gets whether this instance supports persistence of Objects back to their underlying store
        /// </summary>
        public bool SupportsPersistence
        {
            get
            {
                return (this.UpdateProcessor != null);
            }
        }
    }
}
