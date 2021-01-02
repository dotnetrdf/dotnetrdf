# 3.0 Change Notes

## Removed Global Options
The global Options class has been removed and options are now specified closer to where they are used or in some cases removed entirely

### UseBomForUtf8
This option has been replaced with a constructor-injected option to specify the precise text encoding to be used in all writers. For backwards compatibility, the default text encoding used is UTF-8 with **no BOM** (as the default value for this option was `false`)

### ForceHttpBasicAuth
This option has been removed completely and not implemented as a constructor-injected option. This will be reviewed once the codebase is moved over to the .NET HttpClient APIs.

**WARNING:** Changing this property has no effect on HTTP requests made by the library at this point in time, and so the use of this obsolete API will result in a compiler error. This will be reviewed once the HTTP client part of the codebase is moved over to the .NET HttpClient APIs.

### ForceBlockingIO
This option has been removed. To force use of blocking IO you must now explicitly wrap the source stream/text reader by calling ParsingTextReader.CreateBlocking(TextReader) / ParsingTextReader.CreateBlocking(TextReader, int). By default all parsers will wrap streams other than memory or file streams in a blocking text reader, so this step should only be necessary in rare circumstances of unexpected latency in file or memory IO. 

**WARNING:** Setting this property to `true` will not change the use of blocking IO by the parsers. You must wrap the stream you wish to use blocking IO on as described above. As this is a breaking change, the use of these obsolete API will result in a compiler error.

### ValidateIris

This option to force validation of parsed IRIs was only used in the Turtle parsers and can now be specified in the constructor of those parsers.

### InternUris
This option has been moved to the UriFactory static class. Interning is enabled by default and can be disabled by setting UriFactory.InternUris to false.

### UseDtd
This option should be set explicitly when creating a writer. All writer instances that support the use of a DTD provide a UseDtd property (through the IDtdWriter interface) which can be used to change this option after the writer is created. The MimeTypesHelper methods for creating writers also provide an optional parameter for setting this option.

### DefaultCompressionLevel
The compression level desited for a writer should be set explicitly when creating a writer. All writer instances that support compression also provide a CompressionLevel property (through the ICompressingWriter interface) which may be used to change the compression level after the writer is created. The MimeTypesHelper methods for creating writers also provide an optional parameter for setting this option.

### DefaultTokenQueueMode
The token queue mode for tokenizing parsers should be set explicitly when creating a parser. All parsers that implement the `ITokenisingParser` interface provide a `TokenQueueMode` property which may be used to change the mode after the parser is created. The MimeTypesHelper methods for creating parsers also provides an optional parameter for setting this option.

### HttpDebugging, HttpFullDebugging
Console logging of HTTP requests and responses has been removed. Please use the standard .NET HttpClient logging facility instead.

**WARNING:** As these APIs do not have a direct replacement in the library, the use of these APIs will result in a compile-time error.

### DefaultCulture, DefaultComparison
The desired culture and comparison options for node collation should be set explicitly when creating an `INodeComparer` instance. The node comparer to be used in SPARQL queries can be specified as an option when creating a `LeviathanQueryProcessor` using the new optional `options` callback parameter on the `LeviathanQueryProcessor` constructor. The default settings result in ordering that conforms to the SPARQL 1.1 specification. NOTE: The default comparison option for a `SparqlNodeComparere` is `Ordinal`, so to get culture-specific ordering you must both specify the desired culture *and* set the `CompareOptions` property to a value such as `CompareOptions.None` or `CompareOptions.IgnoreCase` depending on how you want strings to be compared.

### FullTripleIndexing
The desired level of indexing can be specified when creating a new `TreeIndexedTripleCollection`. For classes that use a `TreeIndexedTripleCollection` such as the in-memory `Graph` class, the default behaviour remains to create a fully indexed triple collection (this matches the default behaviour in dotNetRDF 2.x), however all of these classes also provide overloaded operators that allow a triple collection with the desired level of indexing to be injected at construction time. 

### UriLoaderCaching
This option is moved to UriLoader.CacheEnabled. This is still a static property at present as the UriLoader is currently implemented as a static class.

### UriLoaderTimeout
This option is moved to UriLoader.Timeout. This is still a static property at present as the UriLoader is currently implemented as a static class.

### QueryExecutionTimeout, AlgebraOptimisation. StrictOperators, RigorousEvaluation, UsePLinqEvaluation
These options can be set when creating a new `LeviathanQueryProcessor` using the new optional `options` callback parameter on the `LeviathanQueryProcessor` constructor.

### QueryOptimisation
By default queries and SPARQL update commands will be optimised by the query parser. To disable this behaviour, use the `QueryOptimisation` property on the `SparqlQueryParser` class (or the same property on the `SparqlUpdateParser` class for SPARQL update commands). NOTE: The default behaviour of optimising queries matches the previous default behaviour of dotNetRDF.

### UnsafeOptimisation
To use the unsafe optimisation option in the `ImplictJoinOptimiser` retrieve the instance of the optimiser from `SparqlOptimisers.AlgebraOptimisers` and set its UnsafeOptimisation property to true. The `IAlgebraOptimiser` interface has been extended to add this property for all optimisers although currently only the `ImplictJoinOptimiser` operates on it.

**WARNING:** Changing the static `Options.UnsafeOptimisation` property at runtime will not change the behaviour of the optimiser as it has already been instantiated. For this reason, the Obsolete warning for this property has been set to generate a compile-time error rather than a warning.

### QueryDefaultSyntax
The desired SPARQL syntax can be set directly on all relevant classes - the most commonly used one being `SparqlQueryParser`, either by a constructor parameter or a property setting or in most cases both. 
The default syntax library-wide is SPARQL 1.1 (`SparqlQuerySyntax.SPARQL_1_1`).

### QueryAllowUnknownFunctions
This option can now be set through the `AllowUnknownFunctions` property of `SparqlQueryParser` and `SparqlUpdateParser`.

### UpdateExecutionTimeout
This option can be set when creating a new `LeviathanUpdateProcessor` using the new optional `options` callback parameter on the `LeviathanUpdateProcessor` constructor.

### NormalizeLiteralValues
This option can now be set on any implementation of `INodeFactory`. This includes `Graph` and other related classes and will affect calls made to `CreateLiteralNode` on those classes.

### LiteralEqualityMode
This option has moved to the `EqualityHelper` static class. 


## .NET serialization support has been removed

We recommend instead using one of the supported RDF/SPARQL syntaxes to serialize/deserialize triples, graphs, stores or SPARQL results.

## The static UriLoader class has a new non-static replacement

The new `VDS.RDF.Parsing.Loader` class provides similar functionality to the `VDS.RDF.Parsing.UriLoader` class but uses the more modern `System.Net.Http` library for its HTTP connections. Making the class non-static allows you to create multiple loader instances configured with different `HttpClient` instances. The main "missing" feature of the new `Loader` class is caching, which can (and should) be handled at the HttpClient layer.

## Node construction changes

There are some significant changes to the way that nodes can be created. At the heart of these changes is that nodes are no longer scoped to a specific graph. A node can be used to assert triples in many different graphs without having to be copied between the graphs. Much of this copying was handled internally by the implementations of the `INodeFactory` interface (an interface which was implemented by the `Graph` class amongst others).

From dotNetRDF 3.0, nodes can be directly created using public constructors on the relevant classes. However the `INodeFactory` class remains as it provides a number of convenient functions:

    * It allows the specification of a `BaseUri` that can be used to create `UriNode`s using relative URIs.
    * It has an `INamespaceMapper` which can be used to resolve QNames to URIs when creating `UriNode`s.
    * It can be used to create auto-assigned blank node identifiers.
    
The `Graph` class now has a `NodeFactory` property that allows the node factory for a graph to be accessed directly and it implements the `INodeFactory` interface by delegating all calls to the `NodeFactory` member. The `NodeFactory` property is read-only, but can be specified in the `Graph` constructor. 

**NOTE**: the way that these changes have been implemented ensures that all existing code that creates nodes and triples in graphs will compile cleanly and continue to behave as expected. There is one significant exception though as noted in the section below.

## BREAKING: Graph Names are now specified using the `Name` property, not `BaseUri`

Prior to this release of dotNetRDF, the `BaseUri` property of an `Graph` served a dual purpose - it provided a base URI that could be used to resolve relative URI references (e.g. when creating new `UriNode`s in the graph); and it served as the name of the graph in an RDF dataset.

With 3.0, the name of the graph can now be either an `IUriNode` or an `IBlankNode` (matching the definition of graph names in the 1.1 version of the RDF specification). This is accessed through the `Name` property of the `IGraph` interface. This property is *read-only* as a graph name should be immutable (especially when it is used to index that graph in collections such as an RDF dataset) - so it can only be set as a constructor parameter. The `BaseUri` property remains on the `IGraph` interface, but it is actually now inherited from the `INodeFactory` interface (which `IGraph` extends) and is used only to resolve relative URIs when creating a new `UriNode`. `BaseUri` is a read-write property as you can (and may want to) change the effective base URI depending on the sources of RDF you are parsing to load the graph with data.

This is an important breaking change for code that deals with named graphs. The change is relatively simple but it does require having access to the graph name at the time that the graph is constructed. Old code that set the graph `BaseUri` to give the graph a name looked like this:

    var graph = new Graph();
    // ... maybe some intervening code
    graph.BaseUri = new Uri("http://example.org/graph1");
    
From dotNetRDF 3.0, the code above **does not change the name of the graph**! To set the graph name, it must now be passed as an `IUriNode` or `IBlankNode` constructor parameter:

    var graph = new Graph(new UriNode(new Uri("http://example.org/graph1")));
    
Of course if you want a URI to be both the name and the base URI of the graph, that is still possible too:

    var graph = new Graph(new UriNode(new Uri("http://example.org/graph1"))) {
       BaseUri = new Uri("http://example.org/graph1")
    };
