# Configuring Graphs 

Graphs can be specified using the [Configuration Vocabulary](http://www.dotnetrdf.org/configuration#) in a variety of ways. Graphs can be specified as empty or they can be specified as the merge of multiple data sources.

Graphs are loaded from Configuration in the following way:

1. Instantiate a Graph of the correct type as specified by the `dnr:type` property, optionally setting the graph name, node factory and/or URI factory.
1. Fill Graph with data from specified sources in the following order:
    1. Other Graphs
    1. Files
    1. Strings (RDF Fragments encoded as Literal Nodes)
    1. Datasets
    1. 3rd Party Triple Stores
    1. URIs
1. Assign a specific URI as the Base URI of the Graph
1.# Apply any specified reasoners

> [!NOTE]
> If any of the data sources fails to load then the loading of the Graph will fail.

# Basic Configuration 

At it's most basic a Graph is specified as follows:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" .
```

With the built in loader you can specify any type which implements the `IGraph` interface and has a public constructor with the following signature:
```c#
public {Type}(IRefNode name, 
            INodeFactory nodeFactory,
            IUriFactory uriFactory, 
            BaseTripleCollection tripleCollection, 
            bool emptyNamespaceMap)
```

## Specifying a graph name

A graph can be configured with a name as follows:
```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:withName <http://example.org/my/graph/name> .
```
The value of the `dnr:withName` property can be either a URI or a blank node.

## Specifying the NodeFactory to use

You can configure a graph to use a specific NodeFactory instance for instantiating its nodes.
This factory will be used when loading data into the graph.

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:nodeFactory a dnr:NodeFactory ;
  dnr:type "VDS.RDF.NodeFactory" .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:withName <http://example.org/my/graph/name> ;
  dnr:usingNodeFactory _:nodeFactory .
```

The NodeFactory options can also be configured. 
See [Configuring Node Factories](node_factory.md)) for more information.

## Specifying the UriFactory to use

You can configure a graph to use a specific NodeFactory instance for instantiating its nodes.
This factory will be used when loading data into the graph.

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:uriFactory a dnr:UriFactory ;
   dnr:type "VDS.RDF.CachingUriFactory" .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:withName <http://example.org/my/graph/name> ;
  dnr:usingUriFactory _:uriFactory .
```

The UriFactory options can also be configured.
See [Configuring URI Factories](uri_factory.md)) for more information.


## Data Source Configuration 

As already detailed there are multiple types of data source which you can fill a Graph with data from. Any combination of these may be used for a Graph and the resulting Graph will be the merge of all the data sources with the sources being loaded in the order specified earlier.

### Other Graphs 

Loading data from other graphs is specified as follows using the `dnr:fromGraph` property:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromGraph _:otherGraph .

_:otherGraph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" .
```

> [!NOTE]
> While it is possible to potentially introduce circular references by this mechanism the [`ConfigurationLoader`](xref:VDS.RDF.Configuration.ConfigurationLoader) is designed such that these references are detected during the loading process and an error will be thrown.

### Files 

Loading data from files is specified as shown below using the `dnr:fromFile` property. File paths can either be absolute or may be relative. In the case of relative paths the resolution of the path can be controlled by introducing an [`IPathResolver`](xref:VDS.RDF.Configuration.IPathResolver) implementation by setting the `PathResolver` property of the `ConfigurationLoader`. Files are expected to be RDF graphs in formats which dotNetRDF understands.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromFile "example.rdf" .
```

### Strings (RDF Fragments) 

You can encode the source of data for your Graph directly as a string using the `dnr:fromString` property like so:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromString """@prefix : <http://example.org/> .
                    :this :is "RDF data encoded in a Literal" . """ .
```

### Datasets 

Graphs can be filled with data from named graphs held in a SPARQL dataset specified using a combination of the `dnr:fromDataset` and `dnr:withUri` properties. The `dnr:fromDataset` property is used to point to a SPARQL Dataset (see [Configuration API - SPARQL Datasets](sparql_datasets.md)) from which data should be loaded and the `dnr:withUri` property is used to specify the URI of the Graph from the dataset which should be loaded. Multiple `dnr:withUri` properties may be specified to load multiple graphs from the dataset.

If you specify multiple `dnr:fromDataset` properties then every URI specified with the `dnr:withUri` property will be loaded from every dataset. To load different graphs from different datasets use the `dnr:fromGraph` property to point to other graphs and setup those graphs to load the specific graphs from the specific databases you need.

### 3rd Party Triple Stores

Graphs can be filled with data from named graphs held in 3rd party triple stores using a combination of the `dnr:fromStore` property and the `dnr:withUri` property in an identical manner to that described above for Databases. Example configuration is as follows:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  # Get the Graph with URI http://nasa.dataincubator.org/spacecraft/SHUTTLE from the store _:store
  dnr:fromStore _:store ;
  dnr:withUri <http://example.org/graph> .

# Specifies a connection to a Fuseki store
_:store a dnr:StorageProvider ;
  dnr:type "VDS.RDF.Storage.FusekiConnector" ;
  dnr:server <http://localhost:3030/dataset> .
```

### URIs 

Graphs can be loaded from URIs using the `dnr:fromUri` property:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  # Gets the Graph of DBPedia's description of Southampton
  dnr:fromUri <http://dbpedia.org/resource/Southampton> .
```

## Assigning a Base URI 

Since Graphs loaded in this way will either have no Base URI or have a Base URI of one of their data sources it is often useful to assign a property URI to this Graph. This URI is the URI used if the Graph is subsequently saved/loaded or otherwise manipulated. A Base URI can be assigned using the `dnr:assignUri` property like so:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:assignUri <http://example.org/assignedUri> .
```

## Applying Reasoners 

You can also apply reasoners to Graphs loaded in this way with the triples that the reasoner produces being materialised in the Graph. To learn how to configure a reasoner see [Configuration API - Reasoners](reasoners.md).

Linking a reasoner to a Graph is as easy as using the `dnr:reasoner` property as the following example in which an RDF Schema reasoner is applied to the Graph.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:graph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:reasoner _:rdfs .

_:rdfs a dnr:Reasoner ;
  dnr:type "VDS.RDF.Query.Inference.RdfsReasoner" .
```