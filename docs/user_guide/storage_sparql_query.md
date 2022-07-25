# SPARQL Query Endpoints 

You can treat any publicly accessible SPARQL Query endpoint as a read-only store using the [SparqlConnector](xref:VDS.RDF.Storage.SparqlConnector).

> [!NOTE]
> If you were looking for documentation on querying a SPARQL endpoint please see [Querying with SPARQL](querying_with_sparql.md)

## Supported Capabilities 

* Load and List Graphs
* SPARQL Query

## Creating a Connection 

You can create a connection either just by providing the endpoint URI like so:

```csharp

SparqlConnector sparql = new SparqlConnector(new Uri("http://example.org/sparql"));
```

Or you can provide a [SparqlRemoteEndpoint](xref:VDS.RDF.Query.SparqlRemoteEndpoint) instance like so:

```csharp

SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://example.org/sparql"), "http://default-graph-uri");

SparqlConnector sparql = new SparqlConnector(endpoint);
```

In both cases there is an overload which takes a [SparqlConnectorLoadMethods](xref:VDS.RDF.Storage.SparqlConnectorLoadMethod) which determines whether the [`LoadGraph()`](xref:VDS.RDF.Storage.IStorageProvider.LoadGraph(VDS.RDF.IGraph,System.String))
 method operates by making a `CONSTRUCT` or a `DESCRIBE` query, the default is `CONSTRUCT`