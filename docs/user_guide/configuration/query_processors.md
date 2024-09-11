# Configuring Query Processors 

Query Processors are classes that can process SPARQL Queries are return a [`SparqlResultSet`](xref:VDS.RDF.Query.SparqlResultSet) or [`IGraph`](xref:VDS.RDF.IGraph) as appropriate. Query Processors implement the [`ISparqlQueryProcessor`](xref:VDS.RDF.Query.ISparqlQueryProcessor) interface and the library provides 5 concrete implementations of this all of which can be configured using the Configuration API.

# Basic Configuration 

Basic Configuration for a Query Processor looks like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" .
```

## Leviathan Query Processor 

The Leviathan Query Processor is used to process queries on in-memory stores using the library's Leviathan SPARQL Engine. It is configured quite simply by adding a `dnr:usingStore` property to the basic configuration, the object pointed to by this property must be a Triple Store which implements the [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore) interface e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

For information on how to configure Triple Stores see [Configuration API - Triple Stores](triple_stores.md).

Alternatively you may use the `dnr:usingDataset` property to connect it to a Dataset instead. See [Configuration API - Datasets](sparql_datasets.md) for details. If both `dnr:usingDataset` and `dnr:usingStore` are present then `dnr:usingDataset` has priority and `dnr:usingStore` will be ignored.

## Generic Query Processor 

The Generic Query Processor is used to process queries against some arbitrary store's SPARQL engine where the store you wish to connect to has an implementation of [`IQueryableStorage`](xref:VDS.RDF.Storage.IQueryableStorage). To configure these handlers simply add a `dnr:storageProvider` property to the basic configuration like so:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.GenericQueryProcessor" ;
  dnr:storageProvider _:manager .

_:manager a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.FusekiConnector" ;
  dnr:server <http://localhost:3030/dataset> .
```

The above specifies a Query Processor which passes the queries to the SPARQL engine of the specified Fuseki store. See [Configuration API - Storage Providers](storage_providers.md) for more detail on configuring storage providers.

## Simple Query Processor 

Similar to the Generic Query Processor the Simple Query Processor passes queries to the `ExecuteQuery()` method of a Triple Store that implements the [`INativelyQueryablerStore`](xref:VDS.RDF.INativelyQueryableStore) interface. To configure this add a using Store property that points to a Triple Store that implements the relevant interface e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.SimpleQueryProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.NativeTripleStore" ;
  dnr:storageProvider _:manager .

_:manager a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.TalisPlatformConnector" ;
  dnr:storeID "space" .
```

The above configuration is effectively identical to the previous example, generally using Generic Query Processors is better and easier to configure.

## Remote Query Processor 

The Remote Query Processor is used to farm out queries to a remote SPARQL endpoint, it can be configured to use any endpoint configurable as described on [Configuration API - SPARQL Clients](sparql_clients.md).

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.RemoteQueryProcessor" ;
  dnr:queryEndpoint _:endpoint .

_:endpoint a dnr:SparqlQueryClient ;
  dnr:type "VDS.RDF.Query.SparqlQueryClient" ;
  dnr:queryEndpointUri <http://example.org/sparql> .
```

The above configures a Remote Query Processor which passes queries to the endpoint at `http://example.org/sparql`

## Pull Query Processor

The Pull Query Processor is an alternate in-memory SPARQL query engine implementation which aims to minimize the size
of intermediate results sets during processing by using a streaming approach to query evaluation.

The Pull Query Processor must be configured with a [Triple Store](triple_stores.md) instance to query against and accepts
options for setting the default query execution timeout and whether the default graph is the union of all graphs.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:proc a dnr:SparqlQueryProcessor
  dnr:type "VDS.RDF.Query.Pull.PullQueryProcessor" ;
  dnr:usingStore _:store ;
  dnr:timeout 60000 ;
  dnr:unionDefaultGraph true ;
.

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

The example above configures a Pull Query Processor to query over an (initially empty) in-memory triple store using
the union of all graphs as the default graph and with a query execution timeout set to 1 minute (60 000ms).
