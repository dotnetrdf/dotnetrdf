# Configuring SPARQL Endpoints 

> [!WARNING]
> The SparqlEndpoint classes are now considered obsolete and replaced by the SparqlClient classes.
> Please see (Configuration - SPARQL Clients)[sparql_clients.md] for how to configure the replacement classes.

SPARQL Endpoints are classes that provide access to remote SPARQL Query or Update endpoints.

The library provides 2 concrete implementations for query endpoints which are [`SparqlRemoteEndpoint`](xref:VDS.RDF.Query.SparqlRemoteEndpoint) and [`FederatedSparqlRemoteEndpoint`](xref:VDS.RDF.Query.FederatedSparqlRemoteEndpoint). The latter of these can be used to federate the query across multiple endpoints and merge the results together.  There is also a [`SparqlRemoteUpdateEndpoint`](xref:VDS.RDF.Update.SparqlRemoteUpdateEndpoint) which represents update endpoints.

## Simple Query Endpoints 

Simple query endpoints are endpoints that use a single remote endpoint for queries. These can be specified relatively simply as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://example.org/sparql> ;
  dnr:defaultGraphUri "http://example.org/defaultGraph" ;
  dnr:namedGraphUri "http://example.org/namedGraph" .
```

Any number of `dnr:defaultGraphUri` and `dnr:namedGraphUri` properties can be used to specify multiple default and named graphs to be used for requests to the endpoint

### User Credentials 

If your SPARQL endpoint requires a username and password this can be added using the `dnr:user` and dnr:password properties or by using the dnr:credentials property to point to an object of type `dnr:User` e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://example.org/sparql> ;
  dnr:user "username" ;
  dnr:password "password" .
```

### Proxy Settings 

You can also specify settings for a proxy server which must be used for requests using the dnr:proxy property to point to an object of type `dnr:Proxy`. See [Configuration API - Proxies](proxy_servers.md) for how to configure proxies.

Note that if the user credentials for the endpoint also apply to the proxy you can omit specifying them on the proxy object and use the `dnr:useCredentialsForProxy` property to state that the same credentials are used for the proxy.

## Federated Query Endpoints 

Federated Endpoints are endpoints that make queries across multiple remote endpoints and combine the results together before returning them. A Federated Endpoint is specified as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:fed a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.FederatedSparqlRemoteEndpoint" ;
  dnr:queryEndpoint <http://sparql.org/books> ;
  dnr:queryEndpoint _:dbpedia .

<http://sparql.org/books> a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://sparql.org/books> .

_:dbpedia a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://dbpedia.org/sparql> .
```

The above configures a Federated Endpoint which sends the query to [DBPedia's SPARQL endpoint](http://dbpedia.org/sparql) and the [Books endpoint](http://sparql.org/books/sparql) at http://sparql.org

## Update Endpoints 

Updates endpoints are endpoints used to make updates against, an update endpoint is specified as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlUpdateEndpoint ;
  dnr:type "VDS.RDF.Update.SparqlRemoteUpdateEndpoint" ;
  dnr:updateEndpointUri <http://example.org/update> ;
  dnr:user "username" ;
  dnr:password "password" .
```
