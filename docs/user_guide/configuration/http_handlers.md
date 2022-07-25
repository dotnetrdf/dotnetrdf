# Configuring HTTP Handlers 

HTTP Handler configuration is used to specify the settings for HTTP Handlers that you wish to use in ASP.Net applications. You can then either add handler registrations manually to your Web.config file or automatically using [rdfWebDeploy](/tools/rdfWebDeploy.md) in order to get the handlers running in your ASP.Net application (see [Deploying with rdfWebDeploy](../asp_deploying_with_rdfwebdeploy.md) for a full example).

When you specify a handler definition you must use a special dotNetRDF URI as the subject like so:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/folder/endpoint> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.QueryHandler" .
```

These URIs are of the format `<dotnetrdf:/path>`, if you are using a Handler that supports wildcard paths then the path must end in a `/*` like `<dotnetrdf:/path/*>`. These paths should be absolute relative paths to guarantee correct operation.

Handlers must always have a `dnr:type` property specified in order to indicate what type of Handler is used to service requests to that path.

## General Handler Configuration 

All handlers have a standard set of properties that can be used to specify some basic configuration for the Handler.

### dnr:userGroup 

The `dnr:userGroup` property is used to associate user groups (see [Configuration API - User Groups](user_groups.md)) with a Handler for basic authentication purposes. Note that this feature is currently experimental and may be revised in future releases.

### dnr:showErrors 

The `dnr:showErrors` property takes a boolean value and controls whether friendly error messages are shown by handlers where such error messages are supported. Defaults to `true` if not specified.

### dnr:introText 

The `dnr:introText` property specifies a path to a file that contains introductory text that handlers can use for any HTML forms they output

### dnr:cacheDuration 

The `dnr:cacheDuration` property specifies a duration in minutes that configuration and data is cached in-memory for. Defaults to ` 15` if not specified, values must be in the range of ` 0` to ` 120` and values outside this range will be set to the minimum/maximum as appropriate. Note that a value of ` 0` indicates no caching.

### dnr:cacheSliding 

The `dnr:cacheSliding` property specifies whether sliding cache expiration is used. Defaults to `true` if not specified.

### dnr:expressionFactory 

The `dnr:expressionFactory` property adds locally scoped SPARQL expression factories to a Handler which the Handler will use when parsing SPARQL. See [Configuration API - Expression Factories](sparql_expression_factories.md) for more details.

### dnr:enableCors 

The `dnr:enableCors` property takes a boolean value and specifies whether CORS headers are output.  Defaults to `true` if not specified.

## Handler Output Configuration 

All Handlers also support the following set of properties which are used to define how writers behave when outputting Graphs, SPARQL Results etc in response to requests.

### dnr:compressionLevel 

The `dnr:compressionLevel` property sets the compression level used by compressing writers, takes an integer, default writers understand values in the range -1 (None) to 10 (High). Defaults to ` 5` if not specified.

### dnr:prettyPrinting 

The `dnr:prettyPrinting` property takes a boolean and controls the use of pretty printing, defaults to `true` if not specified.

### dnr:highSpeedWriting 

The `dnr:highSpeedWriting` property takes a boolean and controls the use of high speed mode, defaults to `true` if not specified.

### dnr:dtdWriting 

The `dnr:dtdWriting` property takes a boolean and controls whether DTDs are included to help compress syntax for XML formats, defaults to `false` if not specified.

### dnr:attributeWriting 

The `dnr:attributeWriting` property takes a boolean and controls advanced behaviour of some XML writers relating to the compressing of literal objects as attributes, defaults to `true` if not specified.

### dnr:multiThreadedWriting 

The #`dnr:multiThreadedWriting` property takes a boolean and controls whether multi-threaded writing can be used, defaults to `true` if not specified.

### dnr:stylesheet 

The `dnr:stylesheet` property specifies a path to a stylesheet that is used for the formatting of any HTML content the handlers output.

### dnr:importNamespacesFrom 

The `dnr:importNamespacesFrom` property points to a Graph that defines default namespaces (see [Configuration API - Graphs](graphs.md)), these are used for any output that can use QNames to compress output. Defaults to `rdf`, `rdfs` and `xsd` namespaces if not specified.

# Handler Configurations 

The following section shows how to configure each type of Handler. Since most types of Handlers can support rather complex configurations involving multiple object configurations only outline configurations are shown here with links to the relevant part of the user guide given.

## Graph Handlers 

Graph Handlers are used to serve an RDF Graph either at a fixed URI or at a base URI. Use the [`GraphHandler`](xref:VDS.RDF.Web.GraphHandler) for fixed URIs and the [`WildcardGraphHandler`](xref:VDS.RDF.Web.WildcardGraphHandler) for base URIs. In practise the latter is mainly needed if you wish to serve a Graph which uses a slash based URI scheme but all terms are defined in that Graph.

Configuration for these Handlers looks like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/graph> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.GraphHandler" ;
  dnr:usingGraph _:graph .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromFile "~/App_Data/example.rdf" .
```

This creates configuration for a Handler which responds to requests on the URI /graph by sending back the Graph specified by the `dnr:usingGraph` property. For details of how to configure Graphs see [Configuration API - Graphs](graphs.md).

For serving graphs at wildcard paths replace the URI with `<dotnetrdf:/graph/*>` and change the value of `dnr:type` to `VDS.RDF.Web.WildcardGraphHandler`. This would result in a handler which responds to requests to any URI under `/graph/` with the specified Graph.

## Query Handlers 

Query Handlers are used to create SPARQL Query endpoints at fixed URIs. To do this there is a single handler implementation which is the [`QueryHandler`](xref:VDS.RDF.Web.QueryHandler).

An example configuration might look like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/sparql> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.QueryHandler" ;
  dnr:queryProcessor _:proc .

_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

This specifies configuration for a Handler which responds to requests on the URI `/sparql` by providing a SPARQL Query endpoint. The `dnr:queryProcessor` property is used to specify the class that processes the queries - for more details on configuring Query Processors see [Configuration API - Query Processors](query_processors.md).

Query Handlers support the following additional properties. Note that while a Handler may have these properties specified not all query processors can/will use/respect these settings.

### dnr:defaultGraphUri 

The `dnr:defaultGraphUri` property specifies a Default Graph URI for queries

### dnr:timeout 

The dnr:timeout property specifies Timeout for queries

### dnr:partialResults 

The `dnr:partialResults` property specifies Partial Results on query timeout behaviour

### dnr:showQueryForm 

The `dnr:showQueryForm` property takes a boolean specifying whether a HTML query form is shown if a request without a query is made to the endpoint

### dnr:defaultQueryFile 

The `dnr:defaultQueryFile` property specifies a path to a file containing the default query to display in the Query Form.

### dnr:queryOptimiser 

The `dnr:queryOptimiser` property specifies a Query Optimiser (see [Configuration API - Optimisers](sparql_optimisers.md)) to be used for optimising SPARQL Queries.

### dnr:algebraOptimiser 

The `dnr:algebraOptimiser` property specifies Algebra Optimisers (see [Configuration API - Optimisers](sparql_optimisers.md)) to be used for optimising SPARQL Algebra prior to evaluation.

## Update Handlers 

Update Handlers are used to provide SPARQL Update endpoints at fixed URIs. To do this there is a single implementation which is [`UpdateHandler`](xref:VDS.RDF.Web.UpdateHandler).

An example configuration might look like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/update> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.UpdateHandler" ;
  dnr:updateProcessor _:proc .

_:proc a dnr:SparqlUpdateProcessor ;
  dnr:type "VDS.RDF.Update.LeviathanUpdateProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

This specifies configuration for a Handler which responds to requests on the URI `/update` by providing a SPARQL Update endpoint. The `dnr:updateProcessor` property is used to specify the class that processes the updates - for more details on configuring Update Processors see [Configuration API - Update Processors](update_processors.md).

Update Handlers support the following additional properties. Note that while a Handler may have these properties specified not all update processors can/will use/respect these settings.

### dnr:showUpdateForm 

The `dnr:showUpdateForm` property takes a boolean specifying whether a HTML update form is shown if a request without a update is made to the endpoint

### dnr:defaultUpdateFile 

The `dnr:defaultUpdateFile` property specifies a path to a file containing the default update to display in the Update Form.

## Protocol Handlers 

Protocol Handlers are used to provide SPARQL Graph Store HTTP Protocol endpoints at either fixed/base URIs. Use the [`ProtocolHandler`](xref:VDS.RDF.Web.ProtocolHandler) for fixed URIs and the [`WildcardProtocolHandler`](xref:VDS.RDF.Web.WildcardProtocolHandler) for base URIs. If you want to support the full capabilities of the protocol you should use the latter since one of the features of the protocol is that requests to URIs under the Base URI should use that URI as the Graph URI unless the graph parameter is specified in the query string.

An example configuration is as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/protocol/*> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.WildcardProtocolHandler" ;
  dnr:protocolProcessor _:proc .

_:proc a dnr:SparqlHttpProtocolProcessor ;
  dnr:type "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

This specifies configuration for a Handler which responds to requests under the URI /protocol/ by providing a SPARQL Graph Store HTTP Protocol endpoint. The `dnr:protocolProcessor` property is used to specify the class that processes the protocol requests - for more details on configuring Protocol Processors see [Configuration API - Protocol Processors](protocol_processors.md).

## SPARQL Servers 

SPARQL Servers are handlers which combine the features of SPARQL Query, Update and Uniform HTTP Protocol into one endpoint. They must always be registered at wildcard URIs and the currently there is one concrete implementation the [`SparqlServer`](xref:VDS.RDF.Web.SparqlServer). A SPARQL Server responds to requests to the Base URI plus query as a Query Endpoint, Base URI plus update as an Update Endpoint and all other URIs as a Graph Store HTTP Protocol Endpoint.

Configuration is essentially the combination of configuration for a Query, Update and Protocol handler like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

<dotnetrdf:/server/*> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.SparqlServer" ;
  dnr:queryProcessor _:qProc ;
  dnr:updateProcessor _:uProc ;
  dnr:protocolProcessor _:pProc .

_:qProc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" ;
  dnr:usingStore _:store .

_:uProc a dnr:SparqlUpdateProcessor ;
  dnr:type "VDS.RDF.Update.LeviathanUpdateProcessor" ;
  dnr:usingStore _:store .

_:pProc a dnr:SparqlHttpProtocolProcessor ;
  dnr:type "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor" ;
  dnr:usingStore _:store .

_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.TripleStore" .
```

This specifies configuration for a Handler which responds to requests to the URI `/server/query` as a Query Endpoint, `/server/update` as an Update Endpoint and all other URIs under `/server` as a Graph Store HTTP Protocol endpoint.