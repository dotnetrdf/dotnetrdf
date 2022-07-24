# Framing

Framing is used to (re)shape the data in a JSON-LD document. It uses an example **frame document** to both match the input data and provide a template for how the output data should be shaped. For more information please refer to [the JSON-LD Framing Specification](https://json-ld.org/spec/latest/json-ld-framing/#framing).

The Framing operation is implemented by the [`JsonLdProcessor.Frame()`](xref:VDS.RDF.JsonLd.JsonLdProcessor.Frame(Newtonsoft.Json.Linq.JToken,Newtonsoft.Json.Linq.JToken,VDS.RDF.JsonLd.JsonLdProcessorOptions)) static method. That method accepts the following parameters:

* `input` - the JSON-LD document to be framed. This parameter may be either a `Newtonsoft.Json.Linq.JObject` representing the parsed JSON-LD document, or a `Newtonsoft.Json.Linq.JValue` with a string value which is the URI of the JSON-LD document to be retrieved and processed.
* `frame` - the JSON document defining the frame to be applied to the input. This parameter may be either a `Newtonsoft.Json.Linq.JObject` representing the parsed JSON-LD frame document, or a `Newtonsoft.Json.Linq.JValue` with a string value which is the URI of the JSON-LD frame document to be retrieved and processed.
* `options` -  a [`JsonLdProcessorOptions`](processor_options.md) providing the options for the framing operation.