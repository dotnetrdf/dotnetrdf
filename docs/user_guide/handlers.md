# Handlers API 

The Handlers API is a powerful API that permits the stream processing of RDF and SPARQL Results.  It can be used in virtually any part of the API that works with RDF or SPARQL results.

The API is designed to facilitate stream processing of the data, that is that handlers get the data as soon as it is available and they control whether processing continues or terminates. Handlers implement either the [`IRdfHandler`](xref:VDS.RDF.IRdfHandler) or the [`ISparqlResultsHandler`](xref:VDS.RDF.ISparqlResultsHandler) interface in order to do this, please note that there is no reason a custom implementation cannot implement both but for ease of implementation and abstraction purposes our implementations do one or the other.

One thing to note is that both handler interfaces descend from the [`INodeFactory`](xref:VDS.RDF.INodeFactory) interface which is a large interface that if implemented incorrectly may lead to serious issues. Therefore we'd typically recommend extending either of [`BaseRdfHandler`](xref:VDS.RDF.Parsing.Handlers.BaseRdfHandler) or [`BaseResultsHandler`](xref:VDS.RDF.Parsing.Handlers.BaseResultsHandler). If you are an advanced developer you may wish to extend their parent class [`BaseHandler`](xref:VDS.RDF.Parsing.Handlers.BaseHandler) instead which will allow you complete control over how you implement the rest of the handler interface while still giving you the `INodeFactory` implementation.

> [!NOTE]
> While the handlers API allows you to read RDF in a fully streaming fashion this does not mean that memory usage won't steadily increase over time due to various internal state that a parser has to keep during the parsing process. 
> You may also need to either disable the URI Interning feature of the default [Node Factory](node_factory.md), or create a separate scoped Node Factory for each separate parsing run if you wish to stream parse very large data files.

# The IRdfHandler Interface 

So let's start by looking at the methods of the [`IRdfHandler`](xref:VDS.RDF.IRdfHandler) interface:

## StartRdf() 

This is called when processing starts, this is typically used to take any initialisation actions and should be used to ensure that a handler is not being used in multiple places at once.

Again please note that there is no reason why you cannot write a handler that can be used in multiple places simultaneously it's just that for most of our own implementations this would lead to unexpected results.

## HandleBaseUri(Uri baseUri) 

This is called whenever the Base URI is altered by the data being processed, in most cases there is no need to do anything in this method other than return `true` to indicate that parsing should continue.

## HandleNamespace(String prefix, Uri namespaceUri) 

This is called whenever a Namespace declaration is encountered, again in mosts cases there is no need to do anything in this method other than return `true` to indicate that parsing should continue.

## HandleTriple(Triple t) 

This is the method which is probably of interest to most people as this is where you actually receive the RDF triples for processing. If the data source from which you are handling triples is actually providing quads then the `GraphUri` property of the triples will be non-null.

In this method you can implement whatever logic you wish regarding triples and then either return `true` to indicate that processing should continue or `false` to indicate processing should terminate.

## EndRdf(bool ok) 

This is the method that is called when processing completes, either because the end of the data was reached or because one of your handlers methods indicated that processing should terminate. The boolean parameter indicates whether the parsing completed/terminated without error or whether an error was encountered. 

Depending on your implementation you may wish to take different clean up actions if an error occurred as opposed to normal parsing completion.

# The ISparqlResultsHandler Interface 

This interface functions very similarily to the `IRdfHandler` interface just the methods are named differently.

## StartResults() 

Called when processing starts and allows you to take any relevant initialisation actions.

## HandleVariable(String var) 

Called whenever a new variable declaration is encountered, typically this will be called a few times at the start of processing before actual results are reached though this will depend on the data source.

## HandleBooleanResult(bool result) 

Called if the results set is a boolean result.

## HandleResult(SparqlResult result) 

Called if the results set is a set of variable bindings. Like the `HandleTriple()` method this is where you will likely want to implement most of your logic and again you should return either `true` to continue processing or `false` to terminate processing.

## EndResults(bool ok) 

Called when processing completes, either because the end of the data was reached or because one of your handlers methods indicated that processing should terminate. The boolean parameter indicates whether the parsing completed/terminated without error or whether an error was encountered. Depending on your implementation you may wish to take different clean up actions if an error occurred as opposed to normal parsing completion.

# Sample Usages 

You can see a basic example of using the API on the [Reading RDF](reading_rdf.md) page

## Using the WriteThroughHandler 

The [`WriteThroughHandler`](xref:VDS.RDF.Parsing.Handlers.WriteThroughHandler) is a powerful `IRdfHandler` implementation that takes in Triples/Quads and outputs them to an arbitrary `TextWriter` using an `ITripleFormatter` of your choice.

This allows you to perform fast data conversion between different formats, please be aware that depending on the format the data compression will be far poorer than that produced by loading the data into memory and then writing it out with an `IRdfWriter`.

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

public class WriteThroughHandlerExample
{
	public static void Main(String[] args)
	{
		//First create out Handler Instance
		//We recommend always using the variant that takes a Type as this allows
		//the Handler to instantiate the formatter itself and echo Namespaces from the
		//input data to the output data
		//The false specifies that the TextWriter being used should not be closed when
		//handling finishes
		WriteThroughHandler handler = new WriteThroughHandler(typeof(NTriplesFormatter), Console.Out, false);

		//Load in the data handling it with our handler
		//We're loading in example.rdf which will get output as NTriples to the Console
		FileLoader.Load(handler, "example.rdf");
	}
}
```