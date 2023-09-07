# Configuring URI Factories

[URI Factories](../uri_factory.md) can be specified using the Configuration Vocabulary.
The configuration loader can use this configuration to instantiate any class which implements the `IUriFactory` interface and has a public constructor that takes a single `INodeFactory` argument.

## Basic Configuration

A vanilla URI Factory can be specified as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:uriFactory a dnr:UriFactory ;
  dnr:type "VDS.RDF.CachingUriFactory" .
```

## Configuring URI Factory Options

### Configuring URI Interning

By default the standard dotNetRDF `IUriFactory` implementation is the `CachingNodeFactory` which supports optional interning (caching) of URIs.
The `IUriFactory` interface provides an option for enabling/disabling this cache.

To construct a URI factory with caching disabled:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:uriFactory a dnr:UriFactory ;
  dnr:type "VDS.RDF.CachingUriFactory" ;
  dnr:internUris false .
```

> [!NOTE]
> The default setting for interning on a `VDS.RDF.CachingUriFactory` is `true`. 
> That default may be different for other implementations.
