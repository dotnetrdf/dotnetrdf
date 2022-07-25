# Updating with SPARQL 

SPARQL Update is a relatively new technology close to being standardised by the W3C SPARQL Working Group as part of the SPARQL 1.1 standard. dotNetRDF supports all of SPARQL Update as per the current Last Call draft of the specification. This article explains how to use SPARQL Updates using the dotNetRDF API.

Advanced Users may want to take a look at the [Advanced SPARQL](advanced_sparql.md) and [SPARQL Optimization](../developer_guide/sparql/optimization.md) pages for more details about how our in-memory SPARQL engine functions.

# Update Representation 

Classes relating to SPARQL Update can be found in the `VDS.RDF.Update` namespace. Unlike Queries a SPARQL Update may contain multiple commands to be executed separated by a semicolon (just like DDL for SQL) so the basic representation of a SPARQL Update is the [SparqlUpdateCommandSet](xref:VDS.RDF.Update.SparqlUpdateCommandSet) which represents a set of commands to be executed. This has a `Commands` property which returns an enumeration of [SparqlUpdateCommand](xref:VDS.RDF.Update.SparqlUpdateCommand) instances.

The `SparqlUpdateCommand` is the abstract base class for all SPARQL Updates. Concrete implementations of this class for each possible SPARQL Update command can be found in the `VDS.RDF.Update.Commands` namespace.

# Parsing Updates 

SPARQL Updates are parsed using the [SparqlUpdateParser](xref:VDS.RDF.Parsing.SparqlUpdateParser) found in the `VDS.RDF.Parsing` namespace. Like the parser for SPARQL Queries it supports four parsing methods - `Parse(StreamReader reader)`, `ParseFromFile(String file)`, `ParseFromString(SparqlParameterizedString update)` and `ParseFromString(String update)` - which all return a `SparqlUpdateCommandSet`.

This is fairly simply used like so:

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

public class SparqlUpdateParsingExample
{
	public static void Main(String[] args)
	{
		//Get an Update Parser
		SparqlUpdateParser parser = new SparqlUpdateParser();

		//Generate a Command
		SparqlParameterizedString cmdString = new SparqlParameterizedString();
		cmdString.CommandText = "LOAD <http://dbpedia.org/resource/Southampton> INTO <http://example.org/Soton>";

		//Parse the command into a SparqlUpdateCommandSet
		SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);

		//Now go ahead and do what you want with the Updates...
	}
}
```

# Applying SPARQL Updates 

There are multiple ways to apply SPARQL Updates which we'll cover here but the most commonly used way which we recommend is to use an instance of `ISparqlUpdateProcessor`. It provides methods for processing an entire command set or specific commands.

## In-Memory Updates 

If your data is purely in-memory then you will want to use the LeviathanUpdateProcessor like so:

```csharp
using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

public class LeviathanUpdateProcessorExample
{
    public static void Main(String[] args)
    {
        //First create our dataset
        TripleStore store = new TripleStore();

        //Get an Update Parser
        SparqlUpdateParser parser = new SparqlUpdateParser();

        //Generate a Command
        SparqlParameterizedString cmdString = new SparqlParameterizedString();
        cmdString.CommandText = "LOAD <http://dbpedia.org/resource/Southampton> INTO <http://example.org/Soton>";

        //Parse the command into a SparqlUpdateCommandSet
        SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);

        //Create a dataset for our queries to operate over
        //We need to explicitly state our default graph or the unnamed graph is used
        //Alternatively you can set the second parameter to true to use the union of all graphs
        //as the default graph
        InMemoryDataset ds = new InMemoryDataset(ds, new Uri("http://mydefaultgraph.org"));

        //Create an Update Processor using our dataset and apply the updates
        LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store, options => {
            // Set processor options here.
            options.UpdateExecutionTimeout = 60*1000
        });
        processor.ProcessCommandSet(cmds);

        //We should now have a Graph in our dataset as a result of the LOAD update
        //So we'll retrieve this and print it to the Console
        IGraph g = store.Graphs[new Uri("http://example.org/Soton")];
        NTriplesFormatter formatter = new NTriplesFormatter();
        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
    }
}
```

A key thing to notice here is that we create a [ISparqlDataset](xref:VDS.RDF.Query.Datasets.ISparqlDataset) instance which wraps our `IInMemoryQueryableStore` instance. This dataset allows us to control which graph is used as the default graph for updates or even to use the union of all graphs as the default graph.

A common error with making updates is that updates by default typically operate only over the unnamed default graph in the store (depending on your query processor). Therefore executing updates may yield no changes depending on what graphs your data is in and whether you configured your dataset correctly. Please see the [SPARQL Datasets](sparql_datasets.md) page for discussions of configuring different kinds of dataset.  If your update has no effect it is always worth running an equivalent `SELECT` or `CONSTRUCT` query to see if that yields any results to make sure you are actually matching some data.

## Generic Updates 

The other really useful processor is the [GenericUpdateProcessor](xref:VDS.RDF.Update.GenericUpdateProcessor) which allows you to apply updates to an underlying [IStorageProvider](xref:VDS.RDF.Storage.IStorageProvider) instance.

The neat feature of this processor is that it is capable of applying SPARQL Updates even if the underlying store does not support them itself. If the underlying store does support SPARQL Update then the processor delegates the processing to the underlying store.

> [!NOTE]
> This is almost certainly less efficient than using a stores own implementation and depending on the exact capabilities of the underlying store not all Updates can be applied.
> If the underlying store provides its own implementation we always prefer this over our own when using the `GenericUpdateProcessor`

```csharp

using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

public class GenericUpdateProcessorExample
{
	public static void Main(String[] args)
	{
		//Get an Update Parser
		SparqlUpdateParser parser = new SparqlUpdateParser();

		//Generate a Command
		SparqlParameterizedString cmdString = new SparqlParameterizedString();
		cmdString.CommandText = "LOAD <http://dbpedia.org/resource/Southampton> INTO <http://example.org/Soton>";

		//Parse the command into a SparqlUpdateCommandSet
		SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);

		//Connect to Sesame (which does not support SPARQL Update itself)
		//and create a GenericUpdateProcessor to apply the update
		SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://localhost:8080/openrdf-sesame", "repository");
		GenericUpdateProcessor processor = new GenericUpdateProcessor(sesame);
		processor.ProcessCommandSet(cmds);

		//We should now have a Graph in our dataset as a result of the LOAD update
		//So we'll retrieve this and print it to the Console
		IGraph g = new Graph();
		sesame.LoadGraph(g, "http://example.org/Soton");
		NTriplesFormatter formatter = new NTriplesFormatter();
		foreach (Triple t in g.Triples)
		{
			Console.WriteLine(t.ToString(formatter));
		}
	}
}
```

## Remote Updates 

We also provide a [RemoteUpdateProcessor](xref:VDS.RDF.Update.RemoteUpdateProcessor) which is a wrapper around a [SparqlRemoteUpdateEndpoint](xref:VDS.RDF.Update.SparqlRemoteUpdateEndpoint) instance.  This allows for updates to be processed by a remote SPARQL Update endpoint.

### Sending Updates to Remote Endpoints 

Just like SPARQL Queries you may wish to send SPARQL Updates to remote servers. To do this we provide the `SparqlRemoteUpdateEndpoint` class, it provides an `Update(String sparqlUpdate)` method which is used to send an update to the remote endpoint plus various methods and properties associated with configuring credentials and proxies for the request.

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Update;

public class RemoteSparqlUpdateExample
{
	public static void Main(String[] args)
	{
		//First build the update we want to send
		//In this example we are making a copy of a Graph then deleting the rdf:type triples
		//from our copy
		SparqlParameterizesString update = new SparqlParameterizedString();
		update.CommandText = "COPY GRAPH <http://example.org/source> TO GRAPH <http://example.org/copy>;";
		update.CommandText += "WITH <http://example.org/copy> DELETE WHERE { ?s a ?type }";

		//Then create our Endpoint instance
		SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint("http://example.org/update");

		//And finally send the update request
		endpoint.Update(update.ToString());
	}
}
```

The above example creates a SPARQL Update containing two commands (a COPY then a DELETE) which is sent to a Remote Endpoint. Invoking the `Update(String update)` method may throw an error if the remote endpoint refuses to process your update request for whatever reason (lack of permissions, malformed update etc).
