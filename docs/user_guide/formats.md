# Formats Supported By dotNetRDF

The following table summarizes all of the formats that are supported by dotNetRDF for parsing and/or writing.

Where multiple dialects are listed, the selection of the dialect is generally controlled through a constructor argument when making a new instance of the relevant parser or writer.
The dialect shown in **bold-face** is the default dialect used when loading from an HTTP response / local file without explicitly specifying the parser to be used.

The **Form** column shows what format the output of the reader is (or the input of the writer is expected to be).

The **Reader Class** and **Writer Class** link to the class documentation for the class that implements parsing and writing for the specified format. 
Where multiple classes are listed, the first one in the list is the class that is used by default when loading from an HTTP response / local file without explicitly specifying the parser to be used.

The **Media Types** column shows the IANA media type strings that are recognized for the format when loading RDF from a URL.
The GZipped versions of parsers map to the same IANA media type strings as their non-gzipped counterpart and are used when an HTTP respnose indicates that the response body is compressed.

The **File Extensions** column shows the file extensions that are recognized for the format when loading RDF from a local file.

Note, for some formats parsing or writing is not supported. This is shown by the value `N/A` in the **Reader Class** or **Writer Class** column where the format is not supported for reading/writing.


| Format            | Dialects | Form  | Reader Class | Writer Class | Media Types | File Extenions |
|-------------------|----------|-------|--------------|--------------|-------------|----------------|
| N-Triples         | Original W3C Specification<br> RDF 1.1<br> **RDF-Star** | Graph | [NTriplesParser](xref:VDS.RDF.Parsing.NTriplesParser) | [NTriplesWriter](xref:VDS.RDF.Writing.NTriplesWriter) | application/n-triples<br> text/plain<br> text/ntriples<br> text/ntriples+turtle<br> application/rdf-triples<br> application/x-ntriples<br> application/ntriples | .nt
| GZipped N-Triples | Original W3C Specification<br> RDF 1.1<br> **RDF-Star** | Graph | [GZippedNTriplesParser](xref:VDS.RDF.Parsing.GZippedNTriplesParser) | [GZippedNTriplesWriter](xref:VDS.RDF.Writing.GZippedNTriplesWriter) |  | .nt.gz
| Turtle            | Original<br> RDF 1.1<br> **RDF-Star** | Graph | [TurtleParser](xref:VDS.RDF.Parsing.TurtleParser) | [CompressingTurtleWriter](xref:VDS.RDF.Writing.CompressingTurtleWriter) | text/turtle<br> application/x-turtle<br> application/turtle | .ttl
| GZipped Turtle    | Original<br> RDF 1.1<br> **RDF-Star** | Graph | [GzippedTurtleParser](xref:VDS.RDF.Parsing.GZippedTurtleParser) | [GZippedTurtleWriter](xref:VDS.RDF.Writing.GZippedTurtleWriter) | | .ttl.gz
| N3 (Notation 3)   |          | Graph | [Notation3Parser](xref:VDS.RDF.Parsing.Notation3Parser) | [Notation3Writer](xref:VDS.RDF.Writing.Notation3Writer) |  text/n3<br> text/rdf+n3 | .n3
| GZipped N3        |          | Graph | [GZippedNotation3Parser](xref:VDS.RDF.Parsing.GZippedNotation3Parser) | [GZippedNotation3Writer](xref:VDS.RDF.Writing.GZippedNotation3Writer) | | .n3.gz
| NQuads            | Original DERI Specification<br> RDF 1.1<br> **RDF-Star** | Store | [NQuadsParser](xref:VDS.RDF.Parsing.NQuadsParser) | [NQuadsWriter](xref:VDS.RDF.Writing.NQuadsWriter) | application/n-quads<br> text/x-nquads | .nq 
| GZipped NQuads    | Original DERI Specification<br> RDF 1.1<br> **RDF-Star** | Store | [GZippedNQuadsParser](xref:VDS.RDF.Parsing.GZippedNQuadsParser) | [GZippedNQuadsWriter](xref:VDS.RDF.Writing.GZippedNQuadsWriter) | | .nq.gz
| TriG              | Original<br> **Member Submission**<br> RDF 1.1<br> RDF-Star | Store | [TriGParser](xref:VDS.RDF.Parsing.TriGParser) | [TriGWriter](xref:VDS.RDF.Writing.TriGWriter) | application/x-trig | .trig |
| GZipped Trig      | Original<br> **Member Submission**<br> RDF 1.1<br> RDF-Star | Store | [GZippedTriGParser](xref:VDS.RDF.Parsing.GZippedTriGParser) | [GZippedTriGWriter](xref:VDS.RDF.Writing.GZippedTriGWriter) | | .trig.gz |
| TriX              |          | Store | [TriXParser](xref:VDS.RDF.Parsing.TriXParser) | [TriXWriter](xref:VDS.RDF.Writing.TriXWriter) | application/trix | .xml 
| GZipped TriX      |          | Store | [GZippedTriXParser](xref:VDS.RDF.Parsing.GZippedTriXParser) | [GZippedTriXWriter](xref:VDS.RDF.Writing.GZippedTriXWriter) | |.xml.gz 
| RDF/XML           |          | Graph | [RdfXmlParser](xref:VDS.RDF.Parsing.RdfXmlParser) | [RdfXmlWriter](xref:VDS.RDF.Writing.RdfXmlWriter)<br> [PrettyRdfXmlWriter](xref:VDS.RDF.Writing.PrettyRdfXmlWriter) | application/rdf+xml<br> text/xml<br> application/xml | .rdf<br>.owl
| GZipped RDF/XML   |          | Graph | [GZippedRdfXmlParser](xref:VDS.RDF.Parsing.GZippedRdfXmlParser) | [GZippedRdfXmlWriter](xref:VDS.RDF.Writing.GZippedRdfXmlWriter) | | .rdf.gz
| RDF/JSON          |          | Graph | [RdfJsonParser](xref:VDS.RDF.Parsing.RdfJsonParser) | [RdfJsonWriter](xref:VDS.RDF.Writing.RdfJsonWriter) | application/json<br> text/json<br> application/rdf+json | .rj<br>.json
| GZipped RDF/JSON  |          | Graph | [GZippedRdfJsonParser](xref:VDS.RDF.Parsing.GZippedRdfJsonParser) | [GZippedRdfJsonWriter](xref:VDS.RDF.Writing.GZippedRdfJsonWriter) | | .rj.gz<br>.json.gz
| JSON-LD           | JSON-LD 1.0<br> JSON-LD 1.1 | Store | [JsonLdParser](xref:VDS.RDF.Parsing.JsonLdParser) | [JsonLdWriter](xref:VDS.RDF.Writing.JsonLdWriter) | application/ld+json | .jsonld<br>.json |
| GZipped JSON-LD   | JSON-LD 1.0<br> JSON-LD 1.1 | Store | [GZippedJsonLdParser](xref:VDS.RDF.Parsing.GZippedJsonLdParser) | [GZippedJsonLdWriter](xref:VDS.RDF.Writing.GZippedJsonLdWriter) | | .jsonld.gz<br>.json.gz
| HTML + RDFa       | RDFa 1.0<br> RDFa 1.1<br> **Auto-detect** (see notes) | Graph | [RdfAParser](xref:VDS.RDF.Parsing.RdfAParser) | [HtmlWriter](xref:VDS.RDF.Writing.HtmlWriter) | text/html<br> application/xhtml+xml | .html<br>.xhtml
| GZipped HTML + RDFa |        | Graph | [GZippedRdfAParser](xref:VDS.RDF.Parsing.GZippedRdfAParser) | [GZippedRdfAWriter](xref:VDS.RDF.Writing.GZippedRdfAWriter) | | .html.gz<br>.xhtml.gz
| CSV (Triples)     |          | Graph | N/A | [CsvWriter](xref:VDS.RDF.Writing.CsvWriter) | |
| CSV (Quads)       |          | Store | N/A | [CsvStoreWriter](xref:VDS.RDF.Writing.CsvStoreWriter)|
| TSV (Triples)     |          | Graph | N/A | [TsvWriter](xref:VDS.RDF.Writing.TsvWriter) |
| TSV (Quads)       |          | Store | N/A | [TsvStoreWriter](xref:VDS.RDF.Writing.TsvStoreWriter)| text/html<br> application/xhtml+xml
| SPARQL Results XML |         | SPARQL Results | [SparqlXmlParser](xref:VDS.RDF.Parsing.SparqlXmlParser) | [SparqlXmlWriter](xref:VDS.RDF.Writing.SparqlXmlWriter) | application/sparql-results+xml | .srx
| GZipped SPARQL Results XML | | SPARQL Results | [GZippedSparqlXmlParser](xref:VDS.RDF.Parsing.GZippedSparqlXmlParser) | [GZippedSparqlXmlWriter](xref:VDS.RDF.Writing.GZippedSparqlXmlWriter) | | .srx.gz
| SPARQL Results JSON |        | SPARQL Results | [SparqlJsonParser](xref:VDS.RDF.Parsing.SparqlJsonParser) | [SparqlJsonWriter](xref:VDS.RDF.Writing.SparqlJsonWriter) | application/sparql-results+json | .srj<br>.json
| GZipped SPARQL Results JSON | | SPARQL Results | [GZippedSparqlJsonParser](xref:VDS.RDF.Parsing.GZippedSparqlJsonParser) | [GZippedSparqlJsonWriter](xref:VDS.RDF.Writing.GZippedSparqlJsonWriter) | | .srj.gz<br>.json.gz
| SPARQL Results Boolean |     | SPARQL Results | [SparqlBooleanParser](xref:VDS.RDF.Parsing.SparqlBooleanParser) | N/A | text/boolean 
| SPARQL Results CSV |         | SPARQL Results | [SparqlCsvParser](xref:VDS.RDF.Parsing.SparqlCsvParser) | [SparqlCsvWriter](xref:VDS.RDF.Writing.SparqlCsvWriter) |text/csv<br> text/comma-separated-values | .csv
| GZipped SPARQL Results CSV | | SPARQL Results | [GZippedSparqlCsvParser](xref:VDS.RDF.Parsing.GZippedSparqlCsvParser) | [GZippedSparqlCsvWriter](xref:VDS.RDF.Writing.GZippedSparqlCsvWriter) | | .csv.gz
| SPARQL Results TSV |         | SPARQL Results | [SparqlTsvParser](xref:VDS.RDF.Parsing.SparqlTsvParser) | [SparqlTsvWriter](xref:VDS.RDF.Writing.SparqlTsvWriter) | text/tab-separated-values | .tsv
| GZipped SPARQL Results TSV | | SPARQL Results | [GZippedSparqlTsvParser](xref:VDS.RDF.Parsing.GZippedSparqlTsvParser) | [GZippedSparqlTsvWriter](xref:VDS.RDF.Writing.GZippedSparqlTsvWriter) | | .tsv.gz
| SPARQL Results RDF | See Notes | SPARQL Results | [SparqlRdfParser](xref:VDS.RDF.Parsing.SparqlRdfParser) | N/A |
| SPARQL Results HTML |        | SPARQL Results | N/A | [SparqlHtmlWriter](xref:VDS.RDF.Writing.SparqlHtmlWriter) | 
| SPARQL Query      | SPARQL 1.0<br> **SPARQL 1.1**<br> SPARQL 1.1 plus dotNetRDF Extensions<br> SPARQL-Star | SPARQL Query   | [SparqlQueryParser](xref:VDS.RDF.Parsing.SparqlQueryParser) | N/A | application/sparql-query | .rq
| SPARQL Update     | SPARQL 1.0<br> **SPARQL 1.1**<br> SPARQL 1.1 plus dotNetRDF Extensions<br> SPARQL-Star | SPARQL Update  | [SparqlUpdateParser](xerf:VDS.RDF.Parsing.SparqlUpdateParser) | N/A | application/sparql-update | .ru
| GraphViz DOT      |          | Graph          | N/A | [GraphVizWriter](xref:VDS.RDF.Writing.GraphVizWriter)
| GraphML           |          | Graph          | N/A | [GraphMLWriter](xref:VDS.RDF.Writing.GraphMLWriter) |

## Notes

The HTML+RDFa parser supports auto-detection modes that default to either RDFa 1.1 or RDFa 1.0. The default mode used defaults to RDFa 1.1.

The SPARQL Results RDF parser will support any of the RDF syntaxes for graphs.
It expects the results set to be expressed using the [DAWG SPARQL Results vocabulary](http://www.w3.org/2001/sw/DataAccess/tests/result-set#).
It defaults to an attempt to auto-detect the format from the input stream (which will fall back to N-Triples if another format is not successfully detected).



## Convenience Writers

The class [SingleGraphWriter](xref:VDS.RDF.Writing.SingleGraphWriter) allows a Store writer to be used to write out a single [IGraph](xref:VDS.RDF.IGraph) instance by treating it as the default graph of a Store.