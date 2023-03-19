# Storage Providers 

This area of the User Guide covers the available [IStorageProvider](xref:VDS.RDF.Storage.IStorageProvider) and [IAsyncStorageProvider](xref:VDS.RDF.Storage.IAsyncStorageProvider) implementations.  Each provider has its own page which details provider specific behaviour and any special functionality available for that provider.

You should read the [Triple Store Integration](triple_store_integration.md) page for an overview of how to use the Storage API.

The available providers are as follows:

| Provider | Description |
| --- | --- |
| [Allegro Graph](storage_allegrograph.md) | AllegroGraph 3.x and 4.x |
| [Blazegraph](storage_blazegraph.md) | Blazegraph |
| [Dataset Files](storage_datasetfile.md) | Read-only view over a NQuads/TriG/TriX file |
| [4store](storage_4store.md) | 4store |
| [Fuseki](storage_fuseki.md) | Apache Jena Fuseki, access any Jena based store via Fuseki |
| [In-Memory](storage_inmemory.md) | In-Memory store |
| [Sesame](storage_sesame.md) | Any Sesame based store is supported e.g. Sesame, OWLIM, BigData |
| [SPARQL Query Endpoints](storage_sparql_query.md) | Any SPARQL Query endpoint |
| [SPARQL Query and Update Endpoints](storage_sparql_query_and_update.md) | Any store providing both a query and update endpoint |
| [SPARQL Graph Store Protocol](storage_sparql_graph_store.md) | Any SPARQL Graph Store Protocol endpoint |
| [Stardog](storage_stardog.md) | Stardog |

> [!INFO]
> dotNetRDF 3.0 has dropped support for connecting to a Virtuoso server through the Virtuoso client library as this library
> requires a dependency on .NET Framework and cannot be used on a modern .NET stack. It is still possible to connect to
> a Virtuoso server via its SPARQL query, update and graph store protocol endpoints using the generic SPARQL connectors
> provided by dotNetRDF.

There are also some useful wrappers available:

| Wrapper | Description |
| --- | --- |
| [ReadOnlyConnector](xref:VDS.RDF.Storage.ReadOnlyConnector) | Make any other provider read-only |
| [QueryableReadOnlyConnector](xref:VDS.RDF.Storage.QueryableReadOnlyConnector) | Make any queryable provider read-only |