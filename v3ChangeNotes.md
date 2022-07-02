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

## There is now more control over the interning of URIs

Prior to this release, the UriFactory static class provided a way to construct URIs in a way that interns the URI, providing the same System.Net.Uri instance for the same URI string. This can drastically reduce memory usage in some scenarios, but in other scenarios where you are dealing with a lot of data or simply a lot of variance in URIs, the interned map of URIs can actually become a memory-hog itself.

In this release we introduce the concept of a hierarchical structure of URI factories each of which has its own interned set of URIs and which can also delegate lookups to a parent factory. This gives you a lot more control over how long interned URIs are kept for. For example in a server you may choose to only intern URIs on a per request basis, or create a session-scoped factory so that the interned URIs are kept just within a single session.

To support this we introduce a new IUriFactory interface and a default implementation (CachingUriFactory). The static UriFactory class now has a static Root (which is an instance of CachingUriFactory) and the static Create and Clear methods just delegate to Root.Create and Root.Clear. In addition there is a lot of restructuring of APIs to add the option to pass a UriFactory or configure a UriFactory to use - for example the IRdfReader interface now has overrides that accept an IUriFactory parameter. In all cases, if the parameter is optional and you don't specify an IUriFactory instance for these parameters, the default is to use the static UriFactory.Root instance, so existing code should not be affected by this change.

## BREAKING: Added ISparqlResultSet interface and changed SparqlResults to implement IEnumerable<ISparqlResultSet> rather than IEnumerable<SparqlResultSet>
The ISparqlResultSet interface has been added to allow a more clean separation between results generated by the Leviathan engine and those generated by other engines or as the result of SPARQL result set parsing. This will cause compile-time implicit type conversion errors where code is receiving values into a variable typed as SparqlResultSet. Changing such variables to ISparqlResultSet should enable clean compilation.

## AutoConfigureSparqlOperators method has moved

The method is now a static method of VDS.RDF.Configuration.SparqlConfigurationLoader so calls to `ConfigurationLoader.AutoConfigureSparqlOperators` will need to be changed to `SparqlConfigurationLoader.AutoConfigureSparqlOperators`

## MIME types for SPARQL Query and Update are no longer registered by default

Because the parsers for SPARQL Query and Update are now in a separate assembly from the core assembly containing the MIME type registry, these parsers need to be registered. The static method `VDS.RDF.SparqlMimeTypeExtensions.RegisterSparqlMimeTypes()`
will perform this registration.

This change will break existing code that uses the MimeTypesHelper class to retrieve SPARQL query or update parsers.

## Triple and IRdfHandler changes

The `Triple` class no longer has a `Graph` property. This property was used in dotNetRDF 2.x to reference the `IGraph` instance that the triple was created in and not necessarily the graph that the triple existed in, which could lead to some confusion.
Because `Triple` has been changed to remove this property, the `IRdfHandler` interface has been extended to add a new `HandleQuad` method which receives a `Triple` and a `IRefNode` as arguments. 
This method is invoked when handling a triple that is asserted in a graph, with the name of the graph as the `IRefNode` argument.
When updating your implementations of `IRdfHandler`, please be aware that some parsers (especially those that support graphs as part of their syntax) may report triples in the default graph by invoking `HandleQuad` with a null value for the `IRefNode` argument, rather than by invoking `HandleTriple`.
You should therefore ensure that you provide an implementation of `HandleQuad` even if your handler does not handle triples in a named graph. In your handler you can then decide whether to treat a non-null `IRefNode` argument as an error or to handle it in some other way (e.g. by ignoring the graph component, or by ignoring the whole quad).