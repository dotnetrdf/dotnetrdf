# Processor Options

The class [`VDS.RDF.JsonLd.JsonLdProcessorOptions`](xref:VDS.RDF.JsonLd.JsonLdProcessorOptions) is used to pass various processing options to the JSON-LD APIs. This class exposes each processor option as a property with getter and setter.

| Option Property | Purpose
|-----------------|-----------|
| Base            | If provided, this URI overrides the base URI of the document being processed for the purposes of relative URI resolution.
| DocumentLoader  | If provided, this function will be used to resolve a URI and load a JSON document from it. See below for more details about implementing a custom loader.
| ProcessingMode  | Sets the processing mode for the JSON-LD processor. See below for more details.
| ExpandContext   | Sets a context that is used to initialize the active context when expanding a document. This value may be a JValue (with a string value which is the URI of the context to be retrieved), a JObject (representing a parsed context), or a JArray of JValue or JObject items.
| ExtractAllScripts | This option relates to the processing of JSON-LD from HTML sources which is not yet implemented.
| FrameExpension | A boolean flag indicating if special processing relating to the expansion of JSON-LD Frames should be applied to an Expand operation. This flag SHOULD NOT be set when calling the Frame API (it is set internally when needed). Defaults to `false`
| CompactArrays   | A boolean flag indicating if arrays of one element should be replaced by a single value during compaction. Defaults to `true`.
| Ordered | Specifies whether the processor should operate on properties in lexicographical order or not. Defaults to `false`.
| ProduceGeneralizedRdf | A boolean flag indicating if the JSON-LD processor may emit blank nodes for triple predicates. If this option is false, then triples with blank node predicates will be omitted. Defaults to `false`. NOTE: The JSON-LD 1.1 specification deprecates the use of this option and notes that a future version of the specification may remove it. It is strongly recommended to avoid creating JSON-LD contexts that result in blank node identifiers for properties.
| RdfDirection | An optional flag specifying how text direction information should be encoded for string literals in RDF. If unset (the default), text direction is not encoded. The value [`JsonLdRdfDirectionMode.I18NDatatype`](xref:VDS.RDF.JsonLd.Syntax.JsonLdRdfDirectionMode.I18NDatatype) instructs the processor to encode direction information as a W3C I18N literal datatype URI. The value [`JsonLdRdfDirectionMode.CompoundLiteral`](xref:VDS.RDF.JsonLd.Syntax.JsonLdRdfDirectionMode.CompoundLiteral) instructs the processor to encode literals with text direction information as a blank node with separate properties for the string value, language (if set) and text direction.
| UseNativeTypes | A boolean flag that determines whether or not JSON native values should be used to represent literals. If enabled, certain string, numeric and boolean datatypes will be encoded in JSON using JSON native values, avoiding the need for an `@type` attribute. Defaults to `false`.
| Embed | Sets the default value object embed flag used in the Framing Algorithm. Defaults to [`JsonLdEmbed.Once`](xref:VDS.RDF.JsonLd.Syntax.JsonLdEmbed.Once)
| Explicit | A boolean flag indicating if explicit inclusion is enabled in the Framing Algorithm. Under explicit inclusion, only properties that are mentioned in a frame are included in the output. Default to `false`.
| RequireAll | A boolean flag controlling how frames are matched in the Framing Algorithm. When true, all non-keyword properties must match the frame for a node to match. When false, any of the non-keyword properties may match the frame for a node to match. Defaults to `false`.
| FrameDefault | A boolean flag indicating if the JSON-LD processor should frame the default graph only (when true) or a merged graph (when false). Defaults to `false`.
| PruneBlankNodeIdentifiers | A boolean flag indicating if the Framing operation should remove unreferenced blank node identifiers from the output. Defaults to `true`. NOTE: This option is no longer part of the JSON-LD 1.1 specification but is retained for use when processing with the JSON-LD 1.0 processing mode.
| OmitDefault | A boolean flag that indicates whether or not the Framing operation should include the @default values of properties when generating the framed output if those properties are missing from the input. Defaults to `false`.
| OmitGraph | A boolean flag that indicates whether or not the outer @graph property can be omitted by the framing process. If `true` an `@graph` property is only generated when required to hold multiple top-level nodes. This property defaults to `true` if `ProcessingMode` is `JsonLdProcessingMode.JsonLd10` or `false` otherwise.
| RemoteContextLimit | An integer value specifying the maximum number of remote context documents to retrieve and process. When this limit is exceeded a [`JsonLdProcessorException`](xref:VDS.RDF.JsonLd.JsonLdProcessorException) exception will be raised. If set to 0, this prevents the processor from retrieving and processing remote contexts (and it will raise an exception if an input document attempts to use a remote context). A value less than 0 indicates that any number of remote contexts may be processed. Defaults to `-1`.

## Specifying Processor Mode

There are three supported JSON-LD processing modes:

* `JsonLd10` - JSON-LD 1.0 processing only. JSON-LD 1.1 features are not supported and if encountered in an input document will result in a [`JsonLdProcessorException`](xref:VDS.RDF.JsonLd.JsonLdProcessorException) being raised with the error code [`ProcessingModeConflict`](xref:VDS.RDF.JsonLd.JsonLdErrorCode.ProcessingModeConflict).
* `JsonLd11` - JSON-LD 1.1 processing mode. This is the default processing mode.
* `JsonLd11FrameExpansion` - JSON-LD 1.1 processing with frame expansion options enabled. This mode is only useful when expanding a JSON-LD frame document as it will cause the expansion processor to retain certain features that are unique to JSON-LD frame documents.

## Custom JSON Loader

The [`Loader`](xref:VDS.RDF.JsonLd.JsonLdProcessorOptions.Loader) property of [`JsonLdProcessorOptions`](xref:VDS.RDF.JsonLd.JsonLdProcessorOptions) allows the developer to override the default remote JSON loading functionality of the JsonLdProcessor. It is a function that takes a `System.Uri` as its only parameter and returns a [`VDS.RDF.JsonLd.RemoteDocument`](xref:VDS.RDF.JsonLd.RemoteDocument) instance representing the loaded JSON. By implementing a custom loader function it is possible to implement your own resolution logic such as redirecting requests to another location or using  a local cache.

The RemoteDocument class has three properties of which two MUST be set and one MAY be set:

| RemoteDocument Property | Value Expected
|-------------------------|------------------------|
| Document                | REQUIRED. The JSON document. The value may be either a `Newtonsoft.Json.Linq.JToken` or a string. If the value is a string, the document will be parsed from the string provided.
| DocumentUrl             | REQUIRED. The final URL of the loaded document, taking into account any redirects. This is required for proper relative URI resolution during processing.
| ContextUrl              | OPTIONAL. If the response from the remote server contains an HTTP Link Header (RFC5988) using the http://www.w3.org/ns/json-ld#context link relation in the response, then the ContextUrl property should be set to that value *unless* the response content type is `application/ld+json` (in which case the link header is ignored). NOTE: it is an error for the server to return multiple context link headers and this should be indicated by having the loader raise a [`JsonLdProcessorException`](xref:VDS.RDF.JsonLd.JsonLdProcessorException) with the error code [`JsonLdErrorCode.MultipleContextLinkHeaders`](xref:VDS.RDF.JsonLd.JsonLdErrorCode)