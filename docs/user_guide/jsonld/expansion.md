# Expansion

The Expansion operation removes context from a JSON-LD document and makes the data structures within the JSON-LD document more regular. For more information about this operation, please refer to [the JSON-LD API Specification](https://json-ld.org/spec/latest/json-ld-api/index.html#expansion).

JSON-LD Expansion is implemented by the [`JsonLdProcessor.Expand()`](xref:VDS.RDF.JsonLd.JsonLdProcessor.Expand(System.Uri,VDS.RDF.JsonLd.JsonLdProcessorOptions,System.Collections.Generic.IList{VDS.RDF.JsonLd.JsonLdProcessorWarning})) static method, which accepts as input either a parsed JSON object (as a `Newtonsoft.Json.Linq.JObject`) or the URI of a JSON-LD document to be retrieved (passed either as a `System.Uri` or as a `Newtonsoft.Json.Linq.JValue`). If a URI is passed, the JSON-LD document is retrieved via an HTTP GET. This resolution and retrieval function can be overridden in the [`JsonLdProcessorOptions`](processor_options.md) instance passed to the method.

The expanded document is returned as a `Newtonsoft.Json.Linq.JArray` instance, which can then be further processed as required.