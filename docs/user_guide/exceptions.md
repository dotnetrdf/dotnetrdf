# Exceptions

The dotNetRDF library defines a number of custom exception classes that are used to communicate error information about errors in working with RDF.

All exceptions classes are derived from the standard .Net `Exception` class. 
The top level exception class within our library is `RdfException`.
All other exceptions classes in our library derive from this like so:

* [VDS.RDF.RdfException](xref:VDS.RDF.RdfException)
  * [VDS.RDF.Configuration.DotNetRdfConfigurationException](xref:VDS.RDF.Configuration.DotNetRdfConfigurationException)
  * [VDS.RDF.Parsing.RdfParseException](xref:VDS.RDF.Parsing.RdfParseException)
    * [VDS.RDF.Parsing.RdfParserSelectionException](xref:VDS.RDF.Parsing.RdfParserSelectionException)
  * [VDS.RDF.Query.RdfQueryException](xref:VDS.RDF.Query.RdfQueryException)
    * [VDS.RDF.Query.RdfQueryTimeoutException](xref:VDS.RDF.Query.RdfQueryTimeoutException)
  * [VDS.RDF.Storage.RdfStorageException](xref:VDS.RDF.Storage.RdfStorageException)
  * [VDS.RDF.Update.SparqlUpdateException](xref:VDS.RDF.Update.SparqlUpdateException)
  * [VDS.RDF.Writing.RdfOutputException](xref:VDS.RDF.Writing.RdfOutputException)
    * [VDS.RDF.Writing.RdfWriterSelectionException](xref:VDS.RDF.Writing.RdfWriterSelectionException)

Most of our exceptions may contain Inner Exceptions where appropriate.

> [!NOTE]
> There are more exceptions than just those listed here but this covers the ones you are most likely to encounter.

# RdfException

The [RdfException](xref:VDS.RDF.RdfException) is the most general exception class and is used to represent general errors in usage of the Library such as using an invalid prefixed name, trying to create a Triple with Nodes from different Graphs etc.

## DotNetRdfConfigurationException

The [DotNetRdfConfigurationException](xref:VDS.RDF.Configuration.DotNetRdfConfigurationException) is an exception that may occur when utilizing the [Configuration API](configuration/index.md).  Typically it occurs due to malformed configuration information.

## RdfParseException

The [RdfParseException](xref:VDS.RDF.Parsing.RdfParseException) is an exception that occurs during the parsing of RDF data when the parser is unable to continue parsing for any reason. Parsing exceptions typically contain a description of the issue which may include Tokeniser data about where the error occurred.

### RdfParserSelectionException

The [RdfParserSelectionException](xref:VDS.RDF.Parsing.RdfParserSelectionException) occurs specifically when the library is unable to select an appropriate parser for the data you are trying to parse.  This may be due to an unknown file extension, incorrect MIME type returned by a HTTP server or the data otherwise not being identifiable.

## RdfQueryException

The [RdfQueryException](xref:VDS.RDF.Query.RdfQueryException) is an exception that occurs during the execution of SPARQL Queries when the query fails for any reason. The exception may be thrown in order to inform the user that the query couldn't be executed for some reason but it also used heavily internally in the execution of SPARQL expressions (though these errors are handled internally and shouldn't ever be seen by the end user).

### RdfQueryTimeoutException

The [RdfQueryTimeoutException](xref:VDS.RDF.Query.RdfQueryTimeoutException) is a subclass of the [RdfQueryException](xref:VDS.RDF.Query.RdfQueryException) and occurs only when the query exceeds the execution timeout. When it occurs it will detail that the query exceeded the timeout and the time after which the query was aborted which is typically higher than the timeout since the timeout is checked only at certain points in query execution.

## RdfStorageException

The [RdfStorageException](xref:VDS.RDF.Storage.RdfStorageException) is an exception that occurs when there is an error accessing some form of RDF Storage. It is primarily thrown by the classes which make up the Storage APIs when there is an issue accessing the underlying storage.

## SparqlUpdateException

The [SparqlUpdateException](xref:VDS.RDF.Update.SparqlUpdateException) is an exception that occurs during the execution of SPARQL Updates.

## RdfOutputException

The [RdfOutputException](xref:VDS.RDF.Writing.RdfOutputException) is an exception that occurs when there an error occurs during the output of RDF by one of the serializers. For the most part the error messages are standardised for this type of exception.

### RdfWriterSelectionException

The [RdfWriterSelectionException](xref:VDS.RDF.Writing.RdfWriterSelectionException) is an exception that occurs when the library is unable to select a writer for the data being output.  This may be due to an unknown file extension or MIME type.