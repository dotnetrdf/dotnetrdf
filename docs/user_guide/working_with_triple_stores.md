# Working with Triple Stores 

Triples Stores in dotNetRDF are used to represent collections of graphs and to allow you to work with larger quantities of RDF easily. As stated in the [Core Concepts](core_concepts.md) our triple stores are designed to be less tangible than graphs since a triple store does not necessarily have to be in-memory and may simply represent an interface to or a partial view on some actual underlying store.

> [!NOTE]
> This document primarily discusses Triple Stores in terms of their representation in-memory within the library. For details of working with external Triple Stores please see [Triple Store Integration](triple_store_integration.md)

# Basic Properties 

Triple Stores are based on the [`ITripleStore`](xref:VDS.RDF.ITripleStore) interface which defines the basic properties of a triple store as follows:

## Graphs 

Gets the collection of graphs in the triple store which is a [`BaseGraphCollection`](xref:VDS.RDF.BaseGraphCollection) - this collection allows you to enumerate through and count the number of graphs in the triple store.

Note that this only returns graphs loaded in-memory for the triple store and does not necessarily represent the entire triple store.

## IsEmpty 

Gets whether a Triple Store is empty (contains no Graphs)

## Triples 

Gets the collection of triples from all the graphs currently in the triple store in-memory. This means it does not necessarily represent the entire triple store.

## Indexer Access 

Indexer access may be used to get a graph from the triple store with the given URI e.g.

```csharp

//Assuming we have a store already
IGraph g = store[new Uri("http://example.org/graph")];
```

# Basic Methods 

The [`ITripleStore`](xref:VDS.RDF.ITripleStore) interface defines the following methods for Triple Stores:

## HasGraph 

Checks whether a Graph with the given URI exists in the Triple Store e.g.

```csharp

if (store.HasGraph(new Uri("http://example.org/")) 
{
	Console.WriteLine("Graph exists");
}
else
{
	Console.WriteLine("Graph doesn't exist");
}
```

## Add and AddFromUri

Used to add graphs into the triple store, graphs can either by added by use of classes that implement `IGraph` or by specifying the URI of a graph. If you use the latter method then the triple store will attempt to retrieve the RDF located at that URI and then insert the resulting graph into the triple store. 

Using the IGraph versions of the methods allows you to insert any type of graph you want into the triple store.

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Parsing;

public class TripleStoreLoadExample
{
	public static void Main(String[] args)
	{
		// Create a Triple Store
		TripleStore store = new TripleStore();

		// Load data from a file into a named graph and add it to the Store
		Graph g = new Graph(new UriNode(new Uri("http://example.org/graph")));
		TurtleParser ttlparser = new TurtleParser();
		ttlparser.Load(g, "Example.ttl");
		store.Add(g);

		// Load a Graph from a URI into the Store.
        // This will create a graph whose name is the same as the URI of the loaded resource.
		store.AddFromUri(new Uri("http://dbpedia.org/resource/Barack_Obama"));

		// Attempt to add another graph with the same name
		// This will cause an error since you can't insert duplicate Graphs in a Triple Store
		Graph h = new Graph(new UriNode(new Uri("http://example.org/graph")));
		ttlparser.Load(h, "Example.ttl");
		try {
			store.Add(h);
		} catch {
			//We get an error
		}

		// You can avoid this by using the second optional boolean parameter to specify behaviour 
		// when a Graph already exists
		// Load the same Graph again but with a merge if it exists in the store
		store.Add(h, true);

		// Try and load an empty Graph that has no name
		// This Graph is then treated as being the default unnamed Graph of the store
		Graph i = new Graph();
		store.Add(i);
	}
}
```

As you'll see from the above example there are a couple of important things to remember when using an `ITripleStore`.
Firstly that if you insert a graph that doesn't have a name then it is treated as the default unnamed graph of the store.
Secondly that you will encounter an error if you try and insert a graph that already exists unless you set the second parameter to true to indicate that the existing graph should be merged with the graph being loaded in.

## Remove 

The [`Remove(Uri graphUri)`](xref:VDS.RDF.ITripleStore.Remove(System.Uri)) method is used to remove a graph that is in the triple store.
Removing a graph that doesn't exist has no effect and does not cause an error.

# In-Memory Triple Stores 

As you have seen the basic triple store interface simply allows you to enumerate over a triple store and to add and remove graphs from it.
While this is useful in itself you'll often want to make queries over the entire store and for this you'll need to use one of the classes that implement [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore).
One of the main things the `IInMemoryQueryableStore` does is to define equivalents of all the various `GetTriples()` methods from the `IGraph` interface for triple stores.
It has two versions of each method, one which operates over all the triples in the triple Store and one which operates over a subset of the triples where the subset is defined by a list of graph names.

> [!WARNING]
> From dotNetRDF version 3.0 the `ExecuteQuery` methods on this interface has been withdrawn. 
> In-memory triple stores can be queried using an [`ISparqlQueryProcessor`](xref:VDS.RDF.Query.ISparqlQueryProcessor) implementation instead as desrcribed in [Querying with SPARQL](querying_with_sparql.md)

# Natively Queryable Stores 

The [`INativelyQueryableStore`](xref:VDS.RDF.INativelyQueryableStore) interface is another extension to the `ITripleStore` interface which is disjoint from the [`IInMemoryQueryableStore`](xref:VDS.RDF.IInMemoryQueryableStore) interface i.e. a Store cannot be both In-Memory and Natively Queryable.
Natively Queryable Stores represents Stores which provide their own SPARQL implementations and so can be queried directly.

An `INativelyQueryableStore` defines two `ExecuteQuery` methods which take a SPARQL query as a string and executes it against the underlying SPARQL implemetnation. The [`ExecuteQuery(string)`](xref:VDS.RDF.INativelyQueryableStore.ExecuteQuery(System.String)) variant returns an object containing the query results.
The [`ExecuteQuery(IRdfHandler, ISparqlResultsHandler, string)`](xref:VDS.RDF.INativelyQueryableStore.ExecuteQuery(VDS.RDF.IRdfHandler,VDS.RDF.ISparqlResultsHandler,System.String)) variant also takes two handler objects which will be invoked to process the query results instead. For more information on handling SPARQL query results, please refer [Querying with SPARQL](querying_with_sparql.md). 
Note that these stores may only be queryable read-only wrappers around an underlying store.

We provide a [`PersistentTripleStore`](xref:VDS.RDF.PersistentTripleStore) class which is an implementation of the `INativelyQueryableStore` that can be used with any of the backing stores we support with [`IQueryableStorage`](xref:VDS.RDF.Storage.IQueryableStorage) implementations.
This class provides an in-memory view of an underlying store where changes to the in-memory view can be persisted to the underlying store (or discarded) as you desire.

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Storage;

public class PersistentTripleStoreExample
{
	public static void Main(String[] args)
	{
		//Create a connection to 4store in this example
		FourStoreConnector 4store = new FourStoreConnector("http://example.com:8080/");
		PersistentTripleStore store = new PersistentTripleStore(4store);

		//See whether a Graph exists in the store
		//If the Graph exists in the underlying store this will cause it to be loaded into memory
		if (store.HasGraph(new Uri("http://example.org/someGraph")))
		{
			//Get the graph out of the in-memory view (note that if it changes in the underlying store in the meantime you will not see those changes)
			Graph g = store.Graph(new Uri("http://example.org/someGraph"));

			//Do something with the Graph...
		}

		//If you were to add a Graph to the store this would be added to the in-memory state only initially
		Graph toAdd = new Graph();
		toAdd.LoadFromUri(new Uri("http://example.org/newGraph"));
		store.Add(toAdd);

		//To ensure that the new graph is saved call Flush()
		store.Flush();

		//You can also use this class to make queries/updates against the underlying store
		//Note - If you've made changes to the in-memory state of the store making a query/update will throw an error unless you've
		//          persisted those changes
		//          Use Flush() or Discard() to ensure the state of the store is consistent for querying

		//Make a Query against the Store
		//Should get a SparqlResultSet back from a SELECT query
		Object results = store.ExecuteQuery("SELECT * WHERE {?s ?p ?o}");
		if (results is SparqlResultSet)
		{
			//Print out the Results
			SparqlResultSet rset = (SparqlResultSet)results;
			foreach (SparqlResult result in rset)
			{
				Console.WriteLine(result.ToString());
			}
		}
	}
}
```

# Loading and Saving Triple Stores 

Often you want the information you place into an in-memory Triple Store to be persisted over time and be able to load/save that triple store as required. Currently we provide the means to save/load a Triple Store in the following ways:

* As a file in a RDF dataset format - TriG, TriX or NQuads
* As explained in [Working with Graphs](working_with_graphs.md] you can also load/save individual Graphs from arbitrary stores for which there is an `IStorageProvider` defined.

## RDF Dataset Storage 

RDF Dataset formats are single file formats which allow storing an RDF dataset represents as a set of named graphs in a single file. We currently support TriG, TriX and NQuads with classes for saving and loading from each format.

For example in TriG (Turtle with Named Graphs) you would can save and load a Triple Store using the [`TriGParser`](xref:VDS.RDF.Parsing.TriGParser) and [`TriGWriter`](xref:VDS.RDF.Writing.TriGWriter) classes as follows:

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

public class TriGExample
{
	public static void Main(String[] args)
	{
		//Read a Store from the TriG file
		TripleStore store = new TripleStore();
		TriGParser trigparser = new TriGParser();
		trigparser.Load(store, "input.trig");

		//Now we want to save to another TriG file
		TriGWriter trigwriter = new TriGWriter();
		trigwriter.Save(store, "output.trig");
	}
}
```

# Standard ITripleStore Implementations 

The Library contains the following standard `ITripleStore` implementations:

| Implementation | Description |
| --- | --- |
| [`DiskDemandTripleStore`](xref:VDS.RDF.DiskDemandTripleStore) | Represents an in-memory store where Graphs are loaded on-demand from the local file system if they are not already in memory and provided the Graph Names are file URIs |
| [`PersistentTripleStore`]xref:VDS.RDF.PersistentTripleStore) | Represents an in-memory view of some store provided by a IStorageProvider instance. |
| [`TripleStore`](xref:VDS.RDF.TripleStore) | In-memory Triple Store representation. |
| [`WebDemandTripleStore`](xref:VDS.RDF.WebDemandTripleStore) | Represents an in-memory Store where Graphs are loaded on-demand from the Web if they are not already in-memory |

# Standard RDF Dataset Parsers & Writers 

The Library contains the following standard [`IStoreReader`](xref:VDS.RDF.IStoreReader) and [`IStoreWriter`](xref:VDS.RDF.IStoreWriter) implementations for RDF dataset formats:

| Implementation | Description |
| --- | --- |
| [`JsonLdParser](xref:VDS.RDF.Parsing.JsonLdParser)  | Parsers JSON-LD |
| [`JsonLdWriter](xref:VDS.RDF.Writing.JsonLdWriter)  | Writes JSON-LD |
| [`NQuadsParser`](xref:VDS.RDF.Parsing.NQuadsParser) | Parses NQuads |
| [`NQuadsWriter`](xref:VDS.RDF.Writing.NQuadsWriter) | Writes NQuads |
| [`TriGParser`](xref:VDS.RDF.Parsing.TriGParser) | Parses TriG |
| [`TriGWriter`](xref:VDS.RDF.Writing.TriGWriter) | Writes TriG |
| [`TriXParser`](xref:VDS.RDF.Parsing.TriXParser) | Parses TriX |
| [`TriXWriter`](xref:VDS.RDF.Writing.TriXWriter) | Writes TriX |

In addition we also provide a [`GraphMLWriter`](xref:VDS.RDF.Writing.GraphMLWriter) class. This writer does not output RDF, but instead [GraphML](http://graphml.graphdrawing.org/) and XML representation of the graphs structure of the store. This file can be loaded into suitable tools for graph visualization.
