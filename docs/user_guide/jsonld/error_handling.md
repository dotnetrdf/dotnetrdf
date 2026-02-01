# Error Handling

Errors relating to JSON-LD processing are notified by the [`JsonLdProcessor`](xref:VDS.RDF.JsonLd.JsonLdProcessor) raising a [`JsonLdProcessorException`](xref:VDS.RDF.JsonLd.JsonLdProcessorException). These exceptions contain an [`ErrorCode`](xref:VDS.RDF.JsonLd.JsonLdProcessorException.ErrorCode) field whose value is a [`JsonLdErrorCode`](xref:VDS.RDF.JsonLd.JsonLdErrorCode). All of the error codes are based directly on the codes defined in the [JSON-LD API Specification](https://json-ld.org/spec/latest/json-ld-api/) and [JSON-LD Framing Specification](https://json-ld.org/spec/latest/json-ld-framing/).

Warnings that are raised during processing are gathered into the [`Warnings`](xref:VDS.RDF>JsonLd.JsonLdProcessor.Warnings) property. 

> [!NOTE]
> When using the `SafeMode` option, callers should be sure to check the `Warnings`, as the presence of warnings indicates that data may have been dropped by the JSON-LD processor.