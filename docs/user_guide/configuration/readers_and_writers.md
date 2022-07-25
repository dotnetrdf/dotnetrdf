# Configuring Readers and Writers 

Configuring Readers and Writers allows you to register custom readers/writers or change the default implementations used by the library.

## Basic Configuration 

You can configure any type of reader/writer supported via the library using this mechanism.  There is some common vocabulary that is used to associate a reader/writer with specific data types.

The `http://www.w3.org/ns/formats/media_type` URI is used to reference MIME types with which the reader/writer should be associated and the `http://www.w3.org/ns/formats/preferred_suffix` URI is used to reference file extensions with which the reader/writer should be associated.

The following example shows how to change the default RDF/XML writer to be the [`PrettyRdfXmlWriter`](xref:VDS.RDF.Writing.PrettyRdfXmlWriter):

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
@prefix fmt: <http://www.w3.org/ns/formats/>.

# Customise to use PrettyRdfXmlWriter

[`] a dnr:RdfWriter ;
	dnr:type "VDS.RDF.Writing.PrettyRdfXmlWriter" ;
	fmt:media_type "application/rdf+xml" ;
	fmt:preferred_suffix "rdf" .
```

You can use the `AutoConfigureReadersAndWriters(IGraph g)` method to have the API automatically load reader and writer configuration.

The same basic configuration applies regardless of whether you are configuring a reader/writer, you merely need to swap out `dnr:RdfWriter` for the class representing what you are registering:

| RDF Class | .Net Class |
| --- | --- |
| `dnr:RdfWriter` | [`IRdfWriter`](xref:VDS.RDF.IRdfWriter) |
| `dnr:RdfParser` | [`IRdfReader`](xref:VDS.RDF.IRdfReader) |
| `dnr:DatasetParser` | [`IStoreReader`](xref:VDS.RDF.IStoreReader) |
| `dnr:DatasetWriter` | [`IStoreWriter`](xref:VDS.RDF.IStoreWriter) |
| `dnr:SparqlResultsParser` | [`ISparqlResultsReader`](xref:VDS.RDF.ISparqlResultsReader) |
| `dnr:SparqlResultsWriter` | [`ISparqlResultsWriter`](xref:VDS.RDF.ISparqlResultsWriter) |