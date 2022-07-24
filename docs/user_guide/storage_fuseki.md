# Fuseki 

[Apache Jena](http://jena.apache.org) [Fuseki](http://jena.apache.org/documentation/serving_data/index.html) is a HTTP server that allows you to expose any Jena based store e.g. TDB for SPARQL access over HTTP.  dotNetRDF can connect to such stores using the [FusekiConnector](xref:VDS.RDF.Storage.FusekiConnector).

## Supported Capabilities 

* Load, Save, Update, Delete and List Graphs
* SPARQL Query and Update

**Note:** Exact capabilities are down to the Fuseki server instance, for instance it might be configured for read-only access.

## Creating a Connection 

Connecting to Fuseki requires the Base URI of a dataset on the server, this is the URI that ends in `/data`, for example:

```csharp

FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
```

You may optionally supply a proxy server if necessary.