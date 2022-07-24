# SPARQL Graph Store Protocol Endpoints 

Any store which provides a [SPARQL Graph Store Protocol](http://www.w3.org/TR/sparql11-http-rdf-update/) endpoint may be connected to via the [SparqlHttpProtocolConnector](xref:VDS.RDF.Storage.SparqlHttpProtocolConnector).
This protocol is different from the normal SPARQL query and update protocol.
If you wish to connect to a store that uses them see the [SPARQL Query Endpoints](storage_sparql_query.md) and [SPARQL Query and Update Endpoints](storage_sparql_query_and_update.md) documentation instead.

## Supported Capabilities 

* Load, Save, Delete and Additive Updates for Graphs

## Creating a Connection 

To connect to the server you need to know the Base URI at which it exposes the protocol:

```csharp

SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri("http://example.org/sparql"));
```

Optionally you may provide a proxy server if necessary.