# Working with Graphs

As previously introduced in the [Core Concepts](core_concepts.md) the graph is the basic representation of a set of Triples used in the Library. All the graph classes are based on the [`IGraph`](xref:VDS.RDF.IGraph) interface and mostly descend from the abstract base class [`BaseGraph`](xref:VDS.RDF.BaseGraph) - the most commonly used descendant being the basic [`Graph`](xref:VDS.RDF.Graph) class.

## Properties

To start this discussion of working with Graphs we're going to look at the properties that an `IGraph` implementation offers us:

### BaseUri

The `BaseUri` property gets/sets the Base URI of the Graph.
A Base URI is the URI against which any relative URIs are resolved as well as any prefixed names in the default namespace (if the default namespace is not explicitly defined).
Graphs are not required to have a Base URI and by default this property returns null.
Typically the Base URI gets set when you read RDF from a file and the RDF syntax defines a Base URI or you retrieve RDF from a URI using the `UriLoader` in which case the URI retrieved is the Base URI.

> [!WARNING]
> In versions prior to dotNetRDF 3.0, the Base URI of a Graph was also considered to be the Graph name wherever code that handles Graphs works with Named Graphs.
> This is no longer the case and the graph name must be set in the constructor when creating the graph and cannot be modified later.

You can use this property to set the Base URI to anything you want:

```csharp
Graph g = new Graph();
g.BaseUri = new Uri("http://example.org/base");
```

### IsEmpty

The `IsEmpty` property is a boolean indicating whether any Triples are contained in this Graph.

### NamespaceMap

The `NamespaceMap` property returns the `INamespaceMapper` instance which is associated with this Graph (properly, it actually returns the instance which is associated with the `INodeFactory` instance that the Graph is using when creating new nodes.
The `INamespaceMapper` is used to map prefixes to URIs in order to allow namespacing and prefixed name resolution, learn about using this class by reading the [Namespace Mapper](namespace_mapper.md) page.

### Nodes and AllNodes

The [`Nodes`](xref:VDS.RDF.IGraph.Nodes) property returns an `IEnumerable<INode>` which returns `INode` instances that appear in the Subject/Object position of Triples.
The `Nodes` method is so named because it returns those `INode` instances which are considered to be the graph nodes in the RDF graph (the predicates are considered to be arcs in the graph-view of RDF).
However, the [`AllNodes`](xref:VDS.RDF.IGraph.AllNodes) does provide an enumeration over subject, predicate and object `INode` instances.

There are extension methods defined in the [`Extensions`](xref:VDS.RDF.Extensions) static class such as [`BlankNodes()`](xref:VDS.RDF.Extensions.BlankNodes(System.Collections.Generic.IEnumerable{VDS.RDF.INode})) that allows you to enumerate over specific types of Nodes from any `IEnumerable<INode>`.

Here's a quick example of iterating over all the URI Nodes in the Graph:

```csharp
//Assuming we have some Graph g find all the URI Nodes
foreach (IUriNode u in g.Nodes.UriNodes())
{
	//Write the URI to the Console
	Console.WriteLine(u.Uri.ToString());
}
```

### Triples

The `Triples` property returns a `BaseTripleCollection` object which is a collection of the Triples contained in the Graph and is probably the most important and most used property of a Graph. This allows you to enumerate over the Triples in the Graph in various ways but is most commonly used as previously seen for simple enumerating the Triples in the Graph and performing some action with each of them.

## Asserting and Retracting Triples

As already seen in the [Core Concepts](core_concepts.md) and [Hello World](hello_world.md) one of the key functions of the Graph is to allow Triples to be asserted and retracted. For this the `Assert(…)` and `Retract(…)` methods are provided. Both of these methods can take either a single Triple or an enumerable of Triples.

Asserting a triple causes it to be added to the triple collection of the graph, retracting a triple causes the reverse to occur. Depending on the `IGraph` implementation being used additional actions may also be taken as part of the assertion and retraction process.

*Note:* When using the `Assert(IEnumerable<Triple> ts)` or `Retract(IEnumerable<Triple> ts)` methods be mindful of where those triples are coming from.  A graph like most collection classes in the .Net world does not allow itself to be modified while it is being enumerated over.  Therefore you may need to call `ToList()` on an enumerable before attempting to assert/retract it.

## Creating Nodes

As also seen in [Core Concepts](core_concepts.md) all Nodes must be created by an `INodeFactory` which all `IGraph` implementations must also implement. Therefore a graph provides `CreateBlankNode(…)`, `CreateLiteralNode(…)` and `CreateURINode(…)`, see the earlier [Core Concepts](core_concepts.md) for usage examples.

## Selecting Nodes

To select Nodes there are methods which can be used to find a Node from a Graph (if it exists) which are the `GetXNode()` methods where `X` is the type of the node to be retrieved. Note that this method only returns a value if the given value exists as a node in the Graph i.e. it occurs in the Subject/Object position of a triple in this graph.

*Note:* If you just want to get a Node instance for other usages regardless of whether it already exists in the Graph you should use the `CreateXNode()` methods instead.

For example:

```csharp
//Assuming we have some Graph g

//Selecting a Blank Node
IBlankNode b = g.GetBlankNode("myNodeID");
if (b != null)
{
	Console.WriteLine("Blank Node with ID " + b.InternalID + " exists in the Graph");
}
else
{
	Console.WriteLine("No Blank Node with the given ID existed in the Graph");
}

//Selecting Literal Nodes

//Plain Literal with the given Value
ILiteralNode l = g.GetLiteralNode("Some Text");

//Literal with the given Value and Language Specifier
ILiteralNode l2 = g.GetLiteralNode("Some Text", "en");

//Literal with the given Value and DataType
ILiteralNode l3 = g.GetLiteralNode("1", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

//Selecting URI Nodes

//By URI
IUriNode u = g.GetUriNode(new Uri("http://example.org/select"));

//By Prefixed Name
IUriNode u2 = g.GetUriNode("ex:select");
```

As you might notice from the part of the example with selecting Blank Nodes if the Node you try and select doesn't exist the return value will be null.

## Selecting Triples

The `IGraph` interface provides a large number of selection methods which allow you to get the results of your selection as an `IEnumerable<Triple>`. The following example shows the use of a few of these methods:

```csharp
//Assuming we have some Graph g

//Get all Triples involving a given Node
IUriNode select = g.CreateUriNode(new Uri("http://example.org/select"));
IEnumerable<Triple> ts = g.GetTriples(select);

//Get all Triples which meet some criteria
//Want to find everything that is rdf:type ex:Person
IUriNode rdfType = g.CreateUriNode("rdf:type");
IUriNode person = g.CreateUriNode("ex:Person");
ts = g.GetTriplesWithPredicateObject(rdfType, person);

//Get all Triples with a given Subject
//We're reusing the node we created earlier
ts = g.GetTriplesWithSubject(select);

//Get all the Triples with a given Predicate
ts = g.GetTriplesWithPredicate(rdfType);

//Get all the Triples with a given Object
ts = g.GetTriplesWithObject(person);
```

From dotNetRDF 3.1 the `Triples` property of a graph also supports tuple-based indexing.
You can use a three-tuple of INode instances (or null in any of the three positions) to return an enumeration of all triples with matching nodes.

```csharp
// Building on the preceding example we can also find triples in the following ways:

// All triples with a given subject
ts = g.Triples[(select, null, null)];

// All triples with a given predicate and object
ts = g.Triples[(null, rdfType, person)];
```

## Merging Graphs

The `Merge(…)` method allows for Graphs to be merged together. The method takes an IGraph as an argument and then has an optional second argument which is a Boolean indicating whether to preserve the original Graph URIs associated with Nodes.

`Merge(…)` implements Graph merging as described in the RDF specification: Triples contained no Blank Nodes are copied from the input Graph if they don't exist in the Graph on which `Merge()` is called, Triples containing Blank Nodes have their Blank Node IDs rewritten so that they don't collide with Blank Nodes already in the Graph.

## Graph Equality

Graph Equality (aka Isomorphism) is supported through use of the standard `Equals(Object obj)` method. We also provide an additional overload `Equals(IGraph g, out Dictionary mapping)` which determines equality and if the Graphs are equivalent returns the mapping of Blank Nodes between the two graphs.  See [Equality and Comparison](equality_and_comparison.md) for more discussion of this.

## Graph Difference

If you know two Graphs are different it may be useful to know how they are different. The `Difference(IGraph g)` method determines the differences between two graphs returning a `GraphDiffReport` which details the differences.

## Loading Graphs

The most common way of loading a Graph is to read RDF from a file/URI as described in [Reading RDF](reading_rdf.md) but there are other ways to read graphs, for example from persistent storage.

### Triple Store Backed Graphs

Graphs can also be loaded from native Triple Stores which are accessible through the dotNetRDF API. To use a native Triple Store you'll need to use one of the `IStorageProvider` implementations located in the `VDS.RDF.Storage` namespace. Graphs can be loaded using an appropriate overload of the `LoadGraph()` method, see [Triple Store Integration](triple_store_integration.md) for more details.

### Using the StoreGraphPersistenceWrapper

Alternatively the `StoreGraphPersistenceWrapper` is a wrapper class that can be placed around any `IGraph` instance and will persist any changes made when it is disposed of unless you discard them using it's `Discard()` method.

```csharp
using VDS.RDF;
using VDS.RDF.Storage;

public class StoreGraphExample
{
	public static void Main(String[] args)
	{
		//Create our Storage Provider - this example uses Virtuoso Universal Server
		VirtuosoManager virtuoso = new VirtuosoManager("localhost", 1111, "DB", "username", "password");

		//Load the Graph into an ordinary graph instance first
		Graph g = new Graph();
		virtuoso.LoadGraph(g, new Uri("http://example.org/"));

		//Then place the Graph into a wrapper
		StoreGraphPersistenceWrapper wrapper = new StoreGraphPersistenceWrapper(virtuoso, g);

		//Now make changes to this Graph as desired...

		//Remember to call Dispose() to ensure changes get persisted when you are done
		wrapper.Dispose();
	}
}
```

### Loading from SPARQL

It is also possible to get a Graph by making a SPARQL query to some SPARQL endpoint where the results of that query will be a Graph i.e. a CONSTRUCT or DESCRIBE query.

```csharp
using VDS.RDF;
using VDS.RDF.Query;

public class SparqlLoadExample
{
	public static void Main(String[] args)
	{
		//First define a SPARQL Endpoint for DBPedia
		SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"));

		//Next define our query
		//We're going to ask DBPedia to describe the first thing it finds which is a Person
		String query = "DESCRIBE ?person WHERE {?person a <http://dbpedia.org/ontology/Person>} LIMIT 1";

		//Get the result
		Graph g = endpoint.QueryWithResultGraph(query);
	}
}
```

## Saving Graphs

The most common way of saving a Graph is to save it to a file as described in [Writing RDF](writing_rdf.md) but you can also save it to other forms of persistent storage.

### Triple Store Backed Graphs

The easiest way to save a Graph to a Triple Store is to use the `SaveGraph()` method of an `IStorageProvider` implementation, see [Triple Store Integration](triple_store_integration.md) for more details.

Alternatively you can use the `StoreGraphPersistenceWrapper` class described earlier since any changes made to it are automatically saved to the Store (if your store supports this).

# Standard IGraph Implementations

The following is a partial list of concrete `IGraph` implementations provided in the library:

| Class | Description |
|-------|-------------|
| `VDS.RDF.Graph` | In-memory graph with triple indexing |
| `VDS.RDF.NonIndexedGraph` | In-memory graph without triple indexing |
| `VDS.RDF.StoreGraphPersistenceWrapper` | A wrapper around another graph where changes are persisted to a backing store using an `IStorageProvider`. |
| `VDS.RDF.ThreadSafeGraph` | A thread safe wrapper around another graph instance |

----

# Tutorial Navigation

The next topic is [Typed Values and Lists](typed_values_and_lists.md), the previous topic was [Writing RDF](writing_rdf.md).

User interesting in learning more may want to jump to the following topics:

* [Namespace Mapper](namespace_mapper.md)
* [Triple Store Integration](triple_store_integration.md)