# Configuring SPARQL Clients 

SPARQL Clients are classes that provide access to remote SPARQL Query or Update endpoints.

The library provides 2 concrete implementations for query endpoints which are [`SparqlQueryClient`](xref:VDS.RDF.Query.SparqlQueryClient) and [`FederatedSparqlQueryClient`](xref:VDS.RDF.Query.FederatedSparqlQueryClient).
The latter of these can be used to federate the query across multiple endpoints and merge the results together.
There is also a [`SparqlUpdateClient`](xref:VDS.RDF.Update.SparqlUpdateClient) which represents acccess to update endpoints.

## Simple Query Client

Simple query clients are clients that use a single remote endpoint for queries.
These can be specified relatively simply as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlQueryClient ;
  dnr:type "VDS.RDF.Query.SparqlQueryClient" ;
  dnr:queryEndpointUri <http://example.org/sparql> ;
  dnr:defaultGraphUri "http://example.org/defaultGraph" ;
  dnr:namedGraphUri "http://example.org/namedGraph" .
```

Any number of `dnr:defaultGraphUri` and `dnr:namedGraphUri` properties can be used to specify multiple default and named graphs to be used for requests to the endpoint

### User Credentials 

If your SPARQL endpoint requires a username and password this can be added using the `dnr:user` and dnr:password properties or by using the dnr:credentials property to point to an object of type `dnr:User` e.g.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlQueryClient ;
  dnr:type "VDS.RDF.Query.SparqlQueryClient" ;
  dnr:queryEndpointUri <http://example.org/sparql> ;
  dnr:user "username" ;
  dnr:password "password" .
```

### Proxy Settings 

You can also specify settings for a proxy server which must be used for requests using the dnr:proxy property to point to an object of type `dnr:Proxy`. See [Configuration API - Proxies](proxy_servers.md) for how to configure proxies.

Note that if the user credentials for the endpoint also apply to the proxy you can omit specifying them on the proxy object and use the `dnr:useCredentialsForProxy` property to state that the same credentials are used for the proxy.

## Federated Query Clients 

Federated Clients are clients that make queries across multiple remote endpoints and combine the results together before returning them. A Federated Client is specified as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:fed a dnr:SparqlQueryClient ;
  dnr:type "VDS.RDF.Query.FederatedSparqlQueryClient" ;
  dnr:queryEndpoint <http://sparql.org/books> ;
  dnr:queryEndpoint _:dbpedia .

<http://sparql.org/books> a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlQueryClient" ;
  dnr:queryEndpointUri <http://sparql.org/books> .

_:dbpedia a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlQueryClient" ;
  dnr:queryEndpointUri <http://dbpedia.org/sparql> .
```

The above configures a Federated Endpoint which sends the query to [DBPedia's SPARQL endpoint](http://dbpedia.org/sparql) and the [Books endpoint](http://sparql.org/books/sparql) at http://sparql.org

## Update Endpoints 

Updates endpoints are endpoints used to make updates against, an update endpoint is specified as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:endpoint a dnr:SparqlUpdateClient ;
  dnr:type "VDS.RDF.Update.SparqlUpdateClient" ;
  dnr:updateEndpointUri <http://example.org/update> ;
  dnr:user "username" ;
  dnr:password "password" .
```
