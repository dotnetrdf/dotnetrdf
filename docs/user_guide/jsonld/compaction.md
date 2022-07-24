# Compaction

Compaction is the opposite to [Expansion](expansion.md). The compaction process applies a context to a JSON-LD document and uses it to shorten IRIs to terms or CURIES and compress JSON-LD values to simple strings or numbers. For more information please refer to [the JSON-LD API Specification](https://json-ld.org/spec/latest/json-ld-api/index.html#compaction).

The Compaction operation is implemented through the static method [`JsonLdProcessor.Compact`](xref:VDS.RDF.JsonLd.JsonLdProcessor.Compact(Newtonsoft.Json.Linq.JToken,Newtonsoft.Json.Linq.JToken,VDS.RDF.JsonLd.JsonLdProcessorOptions)) (and in the .NET Standard framework the asynchronous `JsonLdProcessor.CompactAsync` method). This method accepts three parameters:

* `input` - a `Newtonsoft.Json.Linq.JToken` that represents the input document to be processed or the URI of the document to be retrieved and processed. The input may be one of the following types:
    * A JObject representing a parsed JSON-LD document
    * A JValue with a string value which is the URI of the JSON-LD document to retrieve and parse
* `context` - a `Newtonsoft.Json.Linq.JToken` that represents the context to be applied to the input document. The `context` value may be one of the following types:
    * A JObject representing a parsed JSON-LD context
    * A JValue with a string value which is the URI of the JSON-LD document to retrieve and parse
    * A JArray of JObject or JValue items which represents a collection of contexts to be combined and applied to the input document.
* `options` - a [`JsonLdProcessorOptions`](processor_options.md) instance providing the options for the compaction operation.

The output of the method is a `Newtonsoft.Json.Linq.JObject` that represents the compacted JSON-LD document.
