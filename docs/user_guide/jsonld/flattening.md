# Flattening

The process of flattening gathers together all of the properties of a single node under a single dictionary in the output document. It also ensures that all blank nodes are labelled with a blank node identifier. A flattened document is a lot easier to process in certain applications (especially where mapping to RDF triples is desired). For more information please refer to [the JSON-LD API Specification](https://json-ld.org/spec/latest/json-ld-api/index.html#flattening).

The Flattening operation is implemented by the static method [`JsonLdProcessor.Flatten()`](xref:VDS.RDF.JsonLd.JsonLdProcessor.Flatten(Newtonsoft.Json.Linq.JToken,Newtonsoft.Json.Linq.JToken)). The method accepts the following parameters:

* `input` - the JSON-LD document to be flattened. This parameter may be either a `Newtonsoft.Json.Linq.JObject` representing the parsed JSON-LD document, or a `Newtonsoft.Json.Linq.JValue` with a string value which is the URI of the JSON-LD document to be retrieved and processed.
* `context` - this optional parameter specifies a context to use to compact the flattened document. This parameter can be either null (in which case no compaction is performed) or one of the following values:
    * A `Newtonsoft.Json.Linq.JObject` representing a parsed JSON-LD context
    * A `Newtonsoft.Json.Linq.JValue` with a string value which is the URI of the JSON-LD document to retrieve and parse
    * A `Newtonsoft.Json.Linq.JArray` of `Newtonsoft.Json.Linq.JObject` or `Newtonsoft.Json.Linq.JValue` items which represents a collection of contexts to be combined and applied to the flattened output
* `options` - a [`JsonLdProcessorOptions`](processor_options.md) providing the options for the flatten and compaction (if required) operations.

