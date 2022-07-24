# Minimize Memory Usage 

Depending on how it is used and the data you are working with dotNetRDF can be very memory hungry resulting in large memory footprints for relatively small volumes of data or a memory footprint that grows ever larger over time.

This is not down to memory leaks but rather down to internal features of the API that are designed to make things faster for the average user.  If you are reading this page then you are not an average user and are looking to tailor your usage of dotNetRDF to better manage the memory footprint.  dotNetRDF has you covered here and there are a number of features you can disable/configure and alternative ways of working that will reduce your memory usage.

# URI Interning 

Our [URI Factory](/user_guide/uri_factory.md) feature is used to improve the speed of URI comparisons however if the data you are working with has a lot of unique URIs or you work with data for short periods before throwing it away then this can use a lot of memory over time.

If you are working with the default root URI factory rather than managing your own instances of the `IUriFactory` interface, then you can disable this feature completely like so:

```csharp
UriFactory.InternUris = false;
```

Or you can choose to periodically clean up the memory used:

```csharp
UriFactory.Clear();
```

However, you may find it beneficial to consider creating scoped URI Factory instances (e.g. one for each separate run of a processing pipeline) and dispose of these when the code using them is finished.

# Triple Indexing 

By default the standard [Graph](xref:VDS.RDF.Graph) implementation builds a number of triple indexes behind the scenes.  This help make the `GetTriplesWithX()` methods and SPARQL queries run very fast on in-memory data.  However depending on what you are doing with the graph you may either not need indexes at all or only need certain indexes.

By default indexed graphs in dotNetRDF will incorporate 6 indices:

* Subject
* Predicate
* Object
* Subject-Predicate
* Subject-Object
* Predicate-Object

The first three are considered simple indices and the last three compound indices.

## Use No Indices 

If you don't need the indices you can avoid using indices completely by always using the [NonIndexedGraph](xref:VDS.RDF.NonIndexedGraph) in place of the standard `Graph`

## Use Only Simple Indexes 

If you can live with only simple indices and don't need compound indices you can disable the compound indices like so:

```csharp
Options.FullTripleIndexing = false;
```

**Note:** Indexed graph implementations inspect this setting only when a graph is first creating so you must set this prior to creating your graph.

With full triple indexing set to off only simple indices will be created for newly instantiated graphs.

## Use Specific Indexes

The standard graph allows you to configure the underlying [BaseTripleCollection](xref:VDS.RDF.BaseTripleCollection) used so you can create a graph that only uses the indexes you want:

```csharp
// Create a Triple Collection with only a subject index
BaseTripleCollection tripleCollection = new TreeIndexedTripleCollection(true, false, false, false, false, false, MultiDictionaryMode.AVL);

// Create a Graph using the customized triple collection
Graph g = new Graph(tripleCollection);
```

# Stream Processing 

If you can work with RDF/SPARQL Results in a stream then you can leverage the [Handlers API](/user_guide/handlers.md) as an alternative to loading your data fully into memory, this API gives you complete control over what happens to triples/results as they are generated.

# Token Queue Mode 

Token based parsers accept a [TokenQueueMode](xref:VDS.RDF.Parsing.Tokens.TokenQueueMode) which is defines how it tokenizes the input data, the choice of this can have a significant affect on memory usage.  The global default for this can be controlled via the `Options.DefaultTokenQueueMode` property.

| Queue Mode | Behaviour |
| --- | --- |
| `QueueAllBeforeParsing` | As the name suggests in this mode input data is completely tokenized before any parsing takes place.  This is the most expensive memory wise but may be more performant for very small data |
| `SynchronousBufferDuringParsing` | This is the default, during parsing the tokenizer will buffer some number of tokens ahead in the input data.  It will stop buffering when the buffer is full and start buffering again once the parser starts reading tokens.  This makes it the most stable and memory efficient mode. |
| `AsynchronousBufferDuringParsing` | In this mode the tokenizer will buffer as many tokens as it can asynchronous with the parser, so it can consume very large amounts of memory.  However if the parser is very fast this may be the fastest way to parse. |