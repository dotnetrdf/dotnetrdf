# SPARQL Query and Update Endpoints 

You can treat any publicly accessible SPARQL store which has both Query and Update endpoints as a read-write store using the [ReadWriteSparqlConnector](xref:VDS.RDF.Storage.ReadWriteSparqlConnector).

> [!NOTE]
> If you were looking for documentation on querying a SPARQL endpoint please see [Querying with SPARQL](querying_with_sparql.md)

## Supported Capabilities 

* Save, Load, Delete, Update and List Graphs
* SPARQL Query
* SPARQL Update

> [!WARNING]
> Updating a graph may not work correctly for blank node containing graphs.

## Creating a Connection 

You can create a connection either just by providing the endpoint URIs like so:

```csharp

SparqlConnector sparql = new SparqlConnector(new Uri("http://example.org/query"), new Uri("http://example.org/update"));
```

Or you can provide a [SparqlRemoteEndpoint](xref:VDS.RDF.Query.SparqlRemoteEndpoint) and [SparqlRemoteUpdateEndpoint](xref:VDS.RDF.Update.SparqlRemoteUpdateEndpoint) instance like so:

```csharp

SparqlRemoteEndpoint queryEndpoint = new SparqlRemoteEndpoint(new Uri("http://example.org/query"), "http://default-graph-uri");
SparqlRemoteUpdateEndpoint updateEndpoint = new SparqlRemoteUpdateEndpoint(new Uri("http://example.org/update"));

ReadWriteSparqlConnector sparql = new SparqlConnector(queryEndpoint, updateEndpoint);
```

In both cases there is an overload which takes a [SparqlConnectorLoadMethods](xref:VDS.RDF.Storage.SparqlConnectorLoadMethod) which determines whether the `LoadGraph()` method operates by making a `CONSTRUCT` or a `DESCRIBE` query, the default is `CONSTRUCT`