# JSON-LD API

The namespace [`VDS.RDF.JsonLd`])(xref:VDS.RDF.JsonLd) contains classes related to the processing of JSON-LD documents.
This includes methods to expand, compact and frame JSON-LD documents.
For more information about what these terms mean please refer to the [JSON-LD Specification](https://json-ld.org/spec/latest/json-ld/).
However, please note that for consistency with other RDF formats supported by dotNetRDF, the parser and writer are found in the namespaces [`VDS.RDF.Parsing`](xref:VDS.RDF.Parsing) and [`VDS.RDF.Writing`](xref:VDS.RDF.Writing) respectively.

> [!NOTE]
> This section of the documentation refers to the implementation of the JSON-LD Expansion, Compaction, Flattening and Framing operations.
> For reading/writing JSON-LD the parser and writer work in exactly the same way as for other syntaxes as described in [Reading RDF](../reading_rdf.md) and [Writing RDF](../writing_rdf.md).

The APIs we implement are based on the [JSON-LD 1.1 Specification](https://www.w3.org/TR/json-ld11/), [JSON-LD 1.1 API Specification](https://www.w3.org/TR/json-ld11-api/), and [JSON-LD 1.1 Framing Specification](https://www.w3.org/TR/json-ld11-framing/) and are implemented as static methods on the class [`VDS.RDF.JsonLd.JsonLdProcessor`](xref:VDS.RDF.JsonLd.JsonLdProcessor).
Where JSON objects or arrays are passed through the APIs, we use the Newtonsoft.JSON library's LINQ APIs to represent those objects.

## Features

* [Expansion](expansion.md)
* [Compaction](compaction.md)
* [Flattening](flattening.md)
* [Framing](framing.md)
* [Processor Options](processor_options.md)
* [Error Handling](error_handling.md)
