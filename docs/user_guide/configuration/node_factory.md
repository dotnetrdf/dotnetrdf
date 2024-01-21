# Configuring Node Factories

[Node Factories](../node_factory.md) can be specified using the Configuration Vocabulary.
The configuration loader can use this configuration to instantiate any class which implements the `INodeFactory` interface and has a public no-args constructor.

## Basic Configuration

A vanilla Node Factory can be specified as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" .
```

## Configuring Node Factory Options

A number of additional configuration vocabulary properties are available for setting the options on a Node Factory instance.

### Setting the BaseURI

The [`BaseUri`](xref:VDS.RDF.INodeFactory.BaseUri) property can be configured as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" ;
  dnr:assignUri <http://example.org/> .
```

### Configuring Language Tag Validation

The [`LanguageTagValidation`](xref:VDS.RDF.INodeFactory.LanguageTagValidation) property can be configured using on of the following string values:

  * `false` or `none` for no validation.
  * `true` or `turtle` for validation against the Turtle 1.1 production for language tags.
  * `bcp47` or `wellformed` for validation against the BCP47 specification's definition of a well-formed langauge tag.

> [!NOTE]
> The `true` and `false` values must be quoted strings, not boolean literals.

e.g. to disable language tag validation on a node factory:
```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" ;
  dnr:withLanguageTagValidation "false" .
```

### Configuring Literal Normalization

The [`NormalizeLiteralValues`](xref:VDS.RDF.INodeFactory.NormalizeLiteralValues) property can be configured using boolean value as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" ;
  dnr:normalizeLiterals true .
```

### Configuring URI Factory

The [`UriFactory`](xref:VDS.RDF.INodeFactory.UriFactory) property can be configured using a `dnr:NodeFactory` node as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" ;
  dnr:usingUriFactory _:uriFactory .

_uriFactory a dnr:UriFactory ;
  dnr:type "VDS.RDF.CachingUriFactory" .
```
