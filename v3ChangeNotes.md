# 3.0 Change Notes

## Removed Global Options
The global Options class has been removed and options are now specified closer to where they are used or in some cases removed entirely

### UseBomForUtf8
This option has been replaced with a constructor-injected option to specify the precise text encoding to be used in all writers. For backwards compatibility, the default text encoding used is UTF-8 with **no BOM** (as the default value for this option was `false`)

### ForceHttpBasicAuth
This option has been removed completely and not implemented as a constructor-injected option. This will be reviewed once the codebase is moved over to the .NET HttpClient APIs

### ForceBlockingIO
This option has been removed. To force use of blocking IO you must now explicitly wrap the source stream/text reader by calling ParsingTextReader.CreateBlocking(TextReader) / ParsingTextReader.CreateBlocking(TextReader, int). By default all parsers will wrap streams other than memory or file streams in a blocking text reader, so this step should only be necessary in rare circumstances of unexpected latency in file or memory IO. 

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

### DefaultCulture, DefaultComparison
The desired culture and comparison options for node collation should be set explicitly when creating an `INodeComparer` instance. These options can also be set when creating a `LeviathanQueryProcessor` using the new optional `options` callback parameter on the `LeviathanQueryProcessor` constructor. The default settings result in ordering that conforms to the SPARQL 1.1 specification. NOTE: The default comparison option is `Ordinal`, so to get culture-specific ordering you must both specify the desired culture *and* set the `CompareOptions` property to a value such as `CompareOptions.None` or `CompareOptions.IgnoreCase` depending on how you want strings to be compared.

### FullTripleIndexing
The desired level of indexing can be specified when creating a new `TreeIndexedTripleCollection`. For classes that use a `TreeIndexedTripleCollection` such as the in-memory `Graph` class, the default behaviour remains to create a fully indexed triple collection (this matches the default behaviour in dotNetRDF 2.x), however all of these classes also provide overloaded operators that allow a triple collection with the desired level of indexing to be injected at construction time. 