# Global Options Changes

One of the architectural changes we have made is to try to move away from using a lot of global static settings in favour of specifying options more closely to where they are moved (e.g. via constructor injection or method arguments). 
In line with the deprecation policy, we have (where possible) retained the global static as a way of configuring a global default, but marked the property as deprecated. 
In a small number of cases this has not been possible and the static property has been removed.
To aid migration, this document goes through each of the static `Options` class properties in the dotNetRDF 2.7 API and documents how to migrate code making use of that option.

## AlgebraOptimisation

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanQueryProcessor`](xref:VDS.RDF.Query.LeviathanQueryProcessor) constructors.
See [Customizing Query Processor Behaviour](querying_with_sparql.md#customizing-query-processor-behaviour) for an example.

## AllowMultiThreadWriting

**DEPRECATED**

Writers that support multi-threaded writing now provide a property and a constructor parameter to control the use of this feature.

## DefaultComparisonOptions

**DEPRECATED**

The desired culture and comparison options for node collation should be set explicitly when creating an <xref:VDS.RDF.Query.ISparqlNodeComparer> instance (such as <xref:VDS.RDF.Query.SparqlNodeComparer>.
The node comparer to be used in SPARQL queries can be specified as an option when creating a <xref:VDS.RDF.Query.LeviathanQueryProcessor> using the new optional `options` callback parameter on its constructor.
The default settings result in ordering that conforms to the SPARQL 1.1 specification.

> [!NOTE]
> The default comparison option for a <xref:VDS.RDF.Query.SparqlNodeComparer> is `Ordinal`, so to get culture-specific ordering you must both specify the desired culture *and* set the <xref:VDS.RDF.Query.SparqlNodeComparer.Options> property to a value such as `CompareOptions.None` or `CompareOptions.IgnoreCase` depending on how you want strings to be compared.

## DefaultCompressionLevel

**DEPRECATED**

Instead set the compression level to use using the `CompressionLevel` property of the `ICompressingWriter`(xref:VDS.RDF.ICompressingWriter) interface.

## DefaultCulture

**DEPRECATED**

The desired culture and comparison options for node collation should be set explicitly when creating an <xref:VDS.RDF.Query.ISparqlNodeComparer> instance (such as <xref:VDS.RDF.Query.SparqlNodeComparer>.
The node comparer to be used in SPARQL queries can be specified as an option when creating a <xref:VDS.RDF.Query.LeviathanQueryProcessor> using the new optional `options` callback parameter on its constructor.
The default settings result in ordering that conforms to the SPARQL 1.1 specification.

> [!NOTE]
> The default comparison option for a <xref:VDS.RDF.Query.SparqlNodeComparer> is `Ordinal`, so to get culture-specific ordering you must both specify the desired culture *and* set the <xref:VDS.RDF.Query.SparqlNodeComparer.Options> property to a value such as `CompareOptions.None` or `CompareOptions.IgnoreCase` depending on how you want strings to be compared.


## DefaultTokenQueueMode

**DEPRECATED**

Use the <xref:VDS.RDF.Parsing.ITokenisingParser.TokenQueueMode> property instead, or pass the desired TokenQueueMode to the constructor of those parsers that support this option.

## ForceBlockingIO

**DEPRECATED**

To force use of blocking IO you must now explicitly wrap the source stream/text reader by calling <xref:VDS.RDF.Parsing.ParsingTextReader.CreateBlocking(System.IO.TextReader)> or <xref:VDS.RDF.Parsing.ParsingTextReader.CreateBlocking(System.IO.TextReader,System.Int32)>.
By default all parsers will wrap streams other than memory or file streams in a blocking text reader, so this step should only be necessary in rare circumstances of unexpected latency in file or memory IO.

> [!WARNING]
> This static property no longer has any affect on the use of blocking IO. To force the use of blocking IO you *MUST* follow the procedure outlined above.

## ForceHttpBasicAuth

**DEPRECATED**

There is currently no replacement for this option.

## FullTripleIndexing

**DEPRECATED**

This option can now be set as a constructor parameter on those classes that support full-triple indexing.

## HttpDebugging

**DEPRECATED**

This option has been deprecated. The standard .NET HttpClient library provides a logging facility which clients should use instead.

## HttpFullDebugging

**DEPRECATED**

This option has been deprecated. The standard .NET HttpClient library provides a logging facility which clients should use instead.

## InternUris

**DEPRECATED**

This option can now be set on individual <xref:VDS.RDF.IUriFactory> instances via the <xref:VDS.RDF.IUriFactory.InternUris> property.
To control the interning of URIs in the default factory used by the <xref:VDS.RDF.UriFactory> static class, you can use the <xref:VDS.RDF.UriFactory.InternUris> property.

## LiteralEqualityMode

**DEPRECATED**

This property has been moved to [`EqualityHelper.LiteralEqualityMode`](xref:VDS.RDF.EqualityHelper.LiteralEqualityMode)

## LiteralValueNormalization

**DEPRECATED**

This property is now configured on an [`INodeFactory`](xref:VDS.RDF.INodeFactory) instance via the [`NormalizeLiteralValues`](xref:VDS.RDF.INodeFactory.NormalizeLiteralValues) property.

## QueryAllowUnknownFunctions

**DEPRECATED**

This property is now configured using the `AllowUnknownFunctions` of [`SparqlQueryParser`](xref:VDS.RDF.Parsing.SparqlQueryParser.AllowUnknownFunctions) or [`SparqlUpdateParser`](xref:VDS.RDF.Parsing.SparqlUpdateParser.AllowUnknownFunctions)

## QueryDefaultSyntax

**REMOVED**

> [!WARNING]
> This property has been removed from the API. 

You can specify the syntax to use when parsing a SPARQL query by passing it in as a constructor parameter to the [`SparqlQueryParser`](xref:VDS.RDF.Parsing.SparqlQueryParser).

## QueryExecutionTimeout

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanQueryProcessor`](xref:VDS.RDF.Query.LeviathanQueryProcessor) constructors.
See [Customizing Query Processor Behaviour](querying_with_sparql.md#customizing-query-processor-behaviour) for an example.

## QueryOptimisation

**DEPRECATED**

Instead set the `QueryOptimisation` property of [`SparqlQueryParser`](xref:VDS.RDF.Parsing.SparqlQueryParser.QueryOptimisation) or [`SparqlUpdateParser`](xref:VDS.RDF.Parsing.SparqlUpdateParser.QueryOptimisation).

## RigorousEvaluation

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanQueryProcessor`](xref:VDS.RDF.Query.LeviathanQueryProcessor) constructors.
See [Customizing Query Processor Behaviour](querying_with_sparql.md#customizing-query-processor-behaviour) for an example.

## StrictOperators

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanQueryProcessor`](xref:VDS.RDF.Query.LeviathanQueryProcessor) constructors.
See [Customizing Query Processor Behaviour](querying_with_sparql.md#customizing-query-processor-behaviour) for an example.

## UnsafeOptimisation

**DEPRECATED**

This option applies only to a subset of algebra optimisers and can be enabled by setting the [`UnsafeOptimisation`](xref:VDS.RDF.Query.Optimisation.IAlgebraOptimiser.UnsafeOptimisation) property of those <xref:VDS.RDF.Query.Optimisation.IAlgebraOptimiser> instances that support it.

> [!WARNING]
> This global setting no longer affects whether any optimisers will apply unsafe optimisations.
> If you want to use unsafe optimisations, you now *MUST* set that option explicitly on the optimiser.

## UpdateExecutionTimeout

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanUpdateProcessor`](xref:VDS.RDF.Update.LeviathanUpdateProcessor) constructors.
See [In-Memory Updates](updating_with_sparql.md#in-memory-updates) for an example.

## UriLoaderCaching

**DEPRECATED**

Use [`UriLoader.CacheEnabled`](xref:VDS.RDF.Parsing.UriLoader.CacheEnabled) instead.

## UriLoaderTimeout

**DEPRECATED**

Use[`UriLoader.Timeout`](xref:VDS.RDF.Parsing.UriLoader.Timeout) instead.

## UseBomForUtf8

**DEPRECATED**

To control the use of the UTF-8 BOM, set the appropriate encoding in the writer constructor.

## UseDtd

**DEPRECATED**

This option can now be set on those readers that supporting parsing an XML DTD via a property and a constructor argument.

## UsePLinqEvaluation

**DEPRECATED**

This property can now be set in the `options` callback of the [`LeviathanQueryProcessor`](xref:VDS.RDF.Query.LeviathanQueryProcessor) constructors.
See [Customizing Query Processor Behaviour](querying_with_sparql.md#customizing-query-processor-behaviour) for an example.

## ValidateIris

**DEPRECATED**

This option to force validation of parsed IRIs was only used in the Turtle parsers and can now be specified in the constructor of those parsers.