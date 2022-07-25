# Configuring Storage Providers 

Storage Providers can be specified in order to provide connections to any of the 3rd Party Triple Stores supported by dotNetRDF.

## Allegro Graph 

You can specify a connection to an Allegro Graph server as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:agraph a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.AllegroGraphConnector" ;
  dnr:server "http://agraph.example.com" ;
  dnr:catalogID "catalog" ;
  dnr:storeID "repository" ;
  dnr:user "username" ;
  dnr:password "password" .
```

For this configuration the `dnr:catalogID` property is used to specify the catalog on the Allegro Graph server you wish to use, the `dnr:storeID` property specifies a repository in that catalog. 

The username and password are optional for Allegro Graph.

> [!NOTE]
> If using AllegroGraph 4.x and higher and you wish to connect to the root catalog you can simply ommit the `dnr:catalogID` property.

## Dataset Files 

You can specify a read-only connection to an RDF dataset file like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:datasetFile a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.DatasetFileManager" ;
  dnr:fromFile "example.trig" ;
  dnr:async false .
```

The `dnr:fromFile` property is used to specify the dataset file to connect to and the `dnr:async` property controls whether parsing of that file is done asynchronously.

## 4store 

4store servers can be connected to as shown below:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:fourStore a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.FourStoreConnector" ;
  dnr:server "http://4store.example.com" ;
  dnr:enableUpdates false .
```

For 4store connections it is sufficient to just specify the `dnr:server` property to indicate the server you wish to connect to.

The `dnr:enableUpdates` property controls whether the 4store instance you are connecting to is a version which has support for triple level updates. Since most recent versions of 4store now support this feature triple level updates are automatically enabled so this property is generally unnecessary unless you want to disable the feature.

## Fuseki 

You can connect to any store which is exposed via a Fuseki server using the following configuration:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:fuseki a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.FusekiConnector" ;
  dnr:server "http://fuseki.example.com/dataset/data" .
```

**Note:** The server URI must be for the `/data` endpoint of a dataset of the Fuseki server.

## Sesame HTTP Protocol 

Any server which supports the Sesame HTTP Protocol can be connected to as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:sesame a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.SesameHttpProtocolConnector" ;
  dnr:server "http://sesame.example.com" ;
  dnr:storeID "repository" ;
  dnr:user "username" ;
  dnr:password "password" .
```

When connecting to Sesame HTTP Protocol conforming stores the `dnr:server` property specifies the Base URI of the server and the `dnr:storeID` property the repository on the server you are connecting to.

The username and password are optional and may be omitted.

## SPARQL Query Endpoints 

It is possible to create read-only connections to SPARQL Query endpoints in two ways, either by specifying the endpoint details directly or by separately specifying an endpoint.

### Direct Specification 

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:sparqlQuery a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.SparqlConnector" ;
  dnr:queryEndpointUri "http://example.org/sparql" ;
  dnr:defaultGraphUri "http://example.org/defaultGraph" ;
  dnr:namedGraphUri "http://example.org/namedGraph" ;
  dnr:loadMode "Describe" .
```

The above specifies that the connection be made to the SPARQL Query endpoint with URI `http://example.org/sparql` using the `dnr:queryEndpointUri` property.

The `dnr:defaultGraphUri` and `dnr:namedGraphUri` properties are used to define Default and Named Graph URIs for use with the endpoint, these properties are optional.

The `dnr:loadMode` property takes a value of either `Describe` or `Construct` and states how the connector should attempt to load graphs, either by doing a `DESCRIBE` or `CONSTRUCT` query. If omitted then the default mode is `Construct`

### Separate Specification 

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:sparqlQuery a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.SparqlConnector" ;
  dnr:queryEndpoint _:endpoint ;
  dnr:loadMode "Describe" .

_:endpoint a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://example.org/sparql> ;
  dnr:defaultGraphUri "http://example.org/defaultGraph" ;
  dnr:namedGraphUri "http://example.org/namedGraph" .
```

This defines the same connection as in the previous example but has the advantage that more complex endpoints can be specified in this way as described in [Configuration API - SPARQL Endpoints](sparql_endpoints.md).

## SPARQL Query and Update Endpoints 

Where a store provides both a query and update endpoint you may specify a connection to the store by specifying the relevant endpoints, as with the SPARQL Query connection you can use either the direct or separate specification modes.  For simplicitly our example here shows just the separate specification mode:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:sparqlQuery a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.ReadWriteSparqlConnector" ;
  dnr:queryEndpoint _:queryEndpoint ;
  dnr:updateEndpoint _:updateEndpoint ;
  dnr:loadMode "Describe" .

_:queryEndpoint a dnr:SparqlQueryEndpoint ;
  dnr:type "VDS.RDF.Query.SparqlRemoteEndpoint" ;
  dnr:queryEndpointUri <http://example.org/query> ;
  dnr:defaultGraphUri "http://example.org/defaultGraph" ;
  dnr:namedGraphUri "http://example.org/namedGraph" .
  
_:updateEndpoint a dnr:SparqlUpdateEndpoint ;
  dnr:type "VDS.RDF.Update.SparqlRemoteUpdateEndpoint" ;
  dnr:updateEndpointUri <http://example.org/update>" .
```

## SPARQL Graph Store HTTP Protocol 

SPARQL 1.1 introduces a new RESTful HTTP Protocol for Graph Stores and you can create connections to stores that support this protocol as shown below:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:sparqlHttp a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.SparqlHttpProtocolConnector" ;
  dnr:server "http://example.org/server" .
```

Defining a connection is simply a case of stating the server used with the `dnr:server` property.

## Stardog 

You can specify a connection to an Stardog server as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:stardog a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.StardogConnector" ;
  dnr:server "http://stardog.example.com" ;
  dnr:storeID "db" ;
  dnr:user "username" ;
  dnr:password "password" .
```

The `dnr:server` property sets the Base URI for the server and the `dnr:storeID` property sets the Database ID. The user credentials are optional, currently only HTTP based authentication is supported.

You may optionally add the `dnr:loadMode` property to specify what reasoning mode to use for queries. Supported modes will depend on the exact version of Stardog used but we support DL, EL, RL, QL and RDFS modes which any recent version of Stardog will support.

## Virtuoso 

Connections to Virtuoso's native quad store can be defined as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

# Object Factory must be declared, if you have multiple objects from the Data.Virtuoso
# you need only declare it once in the file
_:virtuosoFactory a dnr:ObjectFactory ;
  dnr:type "VDS.RDF.Configuration.VirtuosoObjectFactory, dotNetRDF.Data.Virtuoso" .

_:virtuoso a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.VirtuosoManager, dotNetRDF.Data.Virtuoso" ;
  dnr:server "http://virtuoso.example.com" ;
  dnr:port "1234" ;
  dnr:database "DB" ;
  dnr:user "username" ;
  dnr:password "password" .
```

For Virtuoso connections the `dnr:server` and `dnr:port` properties specify the server being connected to, omitting `dnr:port` causes the default port of `1111` to be used. The `dnr:database` is an optional property which default to the default quad store database `DB` if not specified.

Username and password are mandatory for connections to Virtuoso.

Note: Since Virtuoso support is in the separate **dotNetRDF.Data.Virtuoso.dll** library any configuration for objects from that library must include the assembly name.