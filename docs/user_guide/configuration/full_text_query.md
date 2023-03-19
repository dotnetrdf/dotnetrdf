# Configuring Full Text Query 

Full Text Query is a non-standard SPARQL extension provided by the **dotNetRDF.Query.FullText.dll** library, please see [Full Text Querying with SPARQL](../full_text_querying_with_sparql.md) for general details on its usage.

You can use the [Configuration API](index.md) to attach full text query functionality to a SPARQL Endpoint.
To do this you will need to specify configuration for one of several ancillary objects.

As the Full Text Query functionality is an additional feature it uses a separate [Full Text Configuration Vocabulary](http://www.dotnetrdf.org/configuration/fulltext#) to specify the additional vocabulary used in configuring these objects.

Note that all types specified by the `dnr:type` property that refer to classes in this library must have the assembly name `dotNetRDF.Query.FullText` included and all types that refer to classes from Lucene.Net must include `Lucene.Net`

# Basic Configuration 

For all Full Text Configuration you'll need to add the additional prefix declaration like so:

```turtle

@prefix dnr-ft: <http://www.dotnetrdf.org/configuration/fulltext#> .
```

You will also need to declare the following:

```turtle

_:fulltextFactory a dnr:ObjectFactory ;
  dnr:type "VDS.RDF.Configuration.FullTextObjectFactory, dotNetRDF.Query.FullText" .
```

## Index Schema 

Index Schemas are specified as follows, this example shows the [`DefaultIndexSchema`](xref:VDS.RDF.Query.FullText.Schema.DefaultIndexSchema) which is the only schema included currently (advanced users can create their own schemas):

```turtle

_:schema a dnr-ft:Schema ;
  dnr:type "VDS.RDF.Query.FullText.Schema.DefaultIndexSchema, dotNetRDF.Query.FullText" .
```

## Indexes 

Indexes represent the actual indexed data, currently this means Lucene.Net directories. Their configuration looks like the following, this example shows a file system based index being used:

```turtle

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.FSDirectory, Lucene.Net" ;
  dnr:fromFile "~/App_Data/index/" ;
  dnr-ft:ensureIndex true .
```

Here the `dnr:fromFile` property is used to point to the directory where the index resides.

The optional `dnr-ft:ensureIndex` property is used to ensure that the index is ready for use, this is useful if you are creating a non-persistent index such as a Lucene.Net `RAMDirectory`.

## Analyzers 

Analyzers are required for Lucene.Net to do the analysis for both indexing and querying. Their configuration looks like the following, any Analyzer instance that has a unparameterized constructor or takes a single Lucene.Net Version parameter can be loaded this way:

```turtle

_:analyzer a dnr-ft:Analyzer ;
  dnr:type "Lucene.Net.Analysis.Standard.StandardAnalayzer" ;
  dnr-ft:version 2900 .
```
The optional `dnr-ft:version` property is used to specify the version of the analyzer to be used, if none is specified 3000 i.e. Lucene.Net 3.0 is assumed

## Indexers 

Indexers are classes that implement the [`IFullTextIndexer`](xref:VDS.RDF.Query.FullText.Indexing.IFullTextIndexer) interface and can perform indexing. Their configuration looks like the following:

```turtle

_:indexer a dnr-ft:Indexer ;
  dnr:type "VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index .

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.FSDirectory, Lucene.Net" ;
  dnr:fromFile "~/App_Data/index/" .
```

The `dnr-ft:index` property is used to point to the index being used and is a required property.

You may optionally specify the `dnr-ft:analyzer` property to point to an analyzer, if not specified the Lucene.Net `StandardAnalyzer` is used.

Also you may optionally specify the `dnr-ft:schema` property to point to a Index Schema, if not specified the `DefaultIndexSchema` is used.

## Search Providers 

A Search Provider provides the actual full text query capability, currently only one implementation [`LuceneSearchProvider`](xref:VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider) is available and it is configured as follows:

```turtle

_:searcher a dnr-ft:Searcher ;
  dnr:type "VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index .

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.FSDirectory, Lucene.Net" ;
  dnr:fromFile "~/App_Data/index/" ;
  dnr-ft:ensureIndex true .
```

As with Indexers the `dnr-ft:index` property is required and points to the index being used. Again the `dnr-ft:analyzer` and `dnr-ft:schema` properties are optional and the same defaults as for Indexers are used if they are not specified.

### Auto-indexing 

When you specify a Search Provider you can also state that it should auto-index some data sources to build the index it operates over. This is useful if you want to create non-persistent indexes over small amounts of data in a SPARQL endpoint for example.

This can be done using the `dnr-ft:buildIndexFor` and `dnr-ft:buildIndexWith` properties which specify data sources and an Indexer respectively.

An example of this is shown below:

```turtle

_:searcher a dnr-ft:Searcher ;
  dnr:type "VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index ;
  dnr-ft:buildIndexWith _:indexer ;
  dnr-ft:buildIndexFor _:graph .

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.RAMDirectory, Lucene.Net" ;
  dnr-ft:ensureIndex true .

_:indexer a dnr-ft:Indexer ;
  dnr:type "VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index .

_:graph a dnr:Graph ;
  dnr:fromFile "example.rdf" .
```

## Full Text Optimiser 

The [`FullTextOptimiser`](xref:VDS.RDF.Query.Optimisation.FullTextOptimiser) is the only optimiser provided by the Full Text library and is used to enable actual full text query support. It is configured as follows:

```turtle

_:optimiser a dnr:AlgebraOptimiser ;
  dnr:type "VDS.RDF.Query.Optimisation.FullTextOptimiser, dotNetRDF.Query.FullText" ;
  dnr-ft:searcher _:searcher .

_:searcher a dnr-ft:Searcher ;
  dnr:type "VDS.RDF.Query.FullText.Search.Lucene.LuceneSearchProvider, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index .

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.FSDirectory, Lucene.Net" ;
  dnr:fromFile "~/App_Data/index/" ;
  dnr-ft:ensureIndex true .
```

## Datasets 

You can use the [`FullTextIndexedDataset`](xref:VDS.RDF.Query.Datasets.FullTextIndexedDataset) as a decorator over another dataset to automatically keep an index in sync with a dataset as that dataset changes.

It is configured as follows:

```turtle

_:ftDataset a dnr:SparqlDataset ;
  dnr:type "VDS.RDF.Query.Dataset.FullTextIndexedDataset, dotNetRDF.Query.FullText" ;
  dnr:usingDataset _:dataset ;
  dnr-ft:indexer _:indexer ;
  dnr-ft:indexNow true .

_:indexer a dnr-ft:Indexer ;
  dnr:type "VDS.RDF.Query.FullText.Indexing.Lucene.LuceneObjectsIndexer, dotNetRDF.Query.FullText" ;
  dnr-ft:index _:index .

_:index a dnr-ft:Index ;
  dnr:type "Lucene.Net.Store.FSDirectory, Lucene.Net" ;
  dnr:fromFile "~/App_Data/index/" .

_:dataset a dnr:SparqlDataset ;
  dnr:type "VDS.RDF.Query.Dataset.InMemoryDataset" .
```

The `dnr-ft:indexNow` property is used to set whether the wrapped dataset should be indexed at initialization time, if you have a pre-built index you should set this to false but for non-persistent datasets you load up on demand it is generally useful to set to true.