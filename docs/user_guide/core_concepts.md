# Core Concepts

The core classes of the Library can be found in the [VDS.RDF](xref:VDS.RDF) namespace. All of the core classes are based either on interfaces or abstract classes to make the library as extensible as possible. These key interfaces are as follows:

| Interface | Purpose |
|-----------|----------|
| [INode](xref:VDS.RDF.INode)   | Represents a node in a RDF Graph, represents the value of a RDF term |
| [IGraph](xref:VDS.RDF.IGraph)  | Interface for Graphs, an RDF document forms a Graph in the mathematical sense - see [RDF Concepts and Abstract Syntax (W3C Specification)](http://www.w3.org/TR/rdf-concepts/) - so we represents sets of Triples as Graphs |
| [ITripleStore](xref:VDS.RDF.ITripleStore) | A Triple Store is a collection of one/more Graphs |

> [!NOTE]
> All code examples presented in this section require you to add the `using VDS.RDF;` statement to the start of your code file.

## Graphs

An RDF Document can be considered to form a mathematical graph and so we represent sets of RDF triples as graphs. All graphs in the library are implementations of the [IGraph](xref:VDS.RDF.IGraph) interface and generally derive from the abstract [BaseGraph](xref:VDS.RDF.BaseGraph) class which implements some of the core methods of the interface allowing specific implementations to concentrate on specifics such as persistence to storage/thread safety.

An [IGraph](xref:VDS.RDF.IGraph) implementation is an in-memory representation of an RDF document.

The most commonly used `IGraph` implementation is the [Graph](xref:VDS.RDF.Graph) class. It can be used as follows:

```csharp
// Create an unamed Graph
IGraph g = new Graph();

//Create a named Graph
IGraph h = new Graph(new UriNode(UriFactory.Create("http://example.org/")));

```

> [!WARNING]
> Prior to 3.0, the BaseUri property was used both as the graph name and as a base URI for relative URI resolution.
> With the 3.0 release the graph name is now specified in the constructor as shown in the example above
> When migrating code from a previous release, you will need to ensure that code that was previously setting the BaseUri property to set the graph name is changed to pass the graph name as a new `UriNode` into the `Graph` constructor.

Triples can be added to a Graph using the `Assert(...)` method, the method takes a single Triple or an array/list/enumerable of Triples. Examples of using this method are given in the section of this page on Triples.

Once your Graph contains some data you can enumerate through it using a foreach loop:

```csharp
//Loop through Triples
foreach (Triple t in g.Triples)
{
    Console.WriteLine(t.ToString());
}
```

Any `IGraph` implements `IEnumerable<Triple>` and so can be used with all the LINQ extension methods for `IEnumerable<T>`.


## Nodes

An [INode](xref:VDS.RDF.INode) represents a node in a RDF graph, this is sometimes referred to as a RDF term.  The interface is quite sparse providing primarily information about the type of the node and the graph it is associated with.  There are then a number of specialized interfaces which extend the basic interface to provide node type specific information:

| Interface | Node Type |
|-----------|------------|
| [IBlankNode](xref:VDS.RDF.IBlankNode) | Blank Node, an anonymous node |
| [ILiteralNode](xref:VDS.RDF.ILiteralNode) | Literal Node, a node with a textual value and optionally a datatype or language tag |
| [IUriNode](xref:VDS.RDF.IUriNode) | URI Node |
| [IRefNode](xref:VDS.RDF.IRefNode) | Reference node, a common base interface for the types of node that can be used to name a graph (`IBlankNode` and `IUriNode`) |
| [ITripleNode](xref:VDS.RDF.ITripleNode) | Triple Node, represents a quoted triple in an RDF-Star graph |
| [IGraphLiteralNode](xref:VDS.RDF.IGraphLiteralNode) | Graph Literal Node, represents a sub-graph |
| [IVariableNode](xref:VDS.RDF.IVariableNode) | Variable Node, represents a variable |

*NB* The latter two go beyond the basic RDF model and are rarely used.

As stated each RDF term can be treated as a node in a graph. As such all RDF terms are modeled as concrete implementations of the `INode` interface and of a relevant sub-interface from the list above e.g. `IUriNode`.

Nodes can be created directly by constructing instances of an implementing type, but it is both more efficient and more convenient to use an [INodeFactory](xref:VDS.RDF.INodeFactory) instance to create nodes.
More convenient, because you don't have to worry about which implementing class to use; and more efficient because the factory can track the nodes created so far and avoid creating duplicates.

All `IGraph` instances also implement `INodeFactory` and by default an in-memory `Graph()` instance has its own factory scoped to that graph. However under more advanced scenarios it is possible to pass the factory instance to use to a `Graph` when it is constructed - potentially allowing multiple graphs to all share the same set of nodes.

> [!NOTE]
> `INode` instances are immutable, so can be freely shared between graphs.

> [!WARNING]
> Prior to dotNetRDF 3.0, `INode` instances could only be created through the `INodeFactory` mehtods exposed by an `IGraph` and could not be used across multiple graphs.
> When updating code written for a previous version of the library, this pattern should still work but it is worth being aware of the possibility of using a common `INodeFactory` if your code is sharing a lot of nodes between multiple graphs.

Currently all nodes are scoped to a particular graph and so must be created through a [INodeFactory](xref:VDS.RDF.INodeFactory), an `IGraph` is by definition an `INodeFactory`.

### URI Nodes

The core of RDF is the use of URIs to refer to resources so you will find that you use the [IUriNode](xref:VDS.RDF.IUriNode) interface the majority of the time.

An URI node can be constructed in several ways as follows:

```csharp
// Create a UriN	ode using its public constructor:
IUriNode exampleOrg = new UriNode(UriFactory.Create("http://example.org/"));

// Create an unamed graph with a non-null BaseUri
IGraph g = new Graph() { BaseUri = UriFactory.Create("http://example.org/") };

//Create a URI Node that refers to the Base URI of the Graph
//Only valid when the Graph has a non-null Base URI
IUriNode thisGraph = g.CreateUriNode();

//Create a URI Node that refers to some specific URI
IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));

// Create a URI Node using a relative IRI
// Only valid when the graph has a non-null BaseUri
IUriNode resolvedRelative = g.CreateUriNode(new Uri("relative/path", UriKind.Relative));
// Reulting URI is http://example.org/relative/path

//Create a URI Node using a Prefixed Name
//Need to define a Namespace first
g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/namespace/"));
IUriNode pname = g.CreateUriNode("ex:demo");
//Resulting URI is http://example.org/namespace/demo
```

Note that because we use the standard .Net `Uri` class to store URIs .Net will automatically normalise URIs for us ensuring that equivalent URIs are equal.

> [!NOTE]
> Notice that we use the [Create()](xref:VDS.RDF.UriFactory.Create(System.String)) method of the [UriFactory](xref:VDS.RDF.UriFactory) class to create URIs since this takes advantage of dotNetRDF's URI interning feature to reduce memory usage and speed up equality comparisons on URIs.

### Blank Nodes

Blank Nodes are used to refer to anonymous resources or to resources where it is unnecessary to assign a URI to identify some resource. Blank Nodes may either have an automatically assigned ID (truly anonymous) or may be given a user assigned ID.

A [IBlankNode](xref:VDS.RDF.IBlankNode) may be constructed as follows:

```csharp

// Create a blank node with an ID using the public constructor
IBlankNode bnode = new BlankNode("ID");

//Other options require a Graph/NodeFactory first
IGraph g = new Graph();

//Create an anonymous Blank Node
//Each call to this method generates a Blank Node with a new unique identifier within the NodeFactory
IBlankNode anon = g.CreateBlankNode();

//Create a named Blank Node
//Reusing the same ID results in the same Blank Node within the NodeFactory
//Note that if the ID refers to an automatically assigned ID that is already in use the returned
//Blank Node will be given an alternative ID
IBlankNode named = g.CreateBlankNode("ID");
```

> [!NOTE]
> Be careful of the above proviso about ID collisions between automatically assigned Blank Node IDs (those created with `CreateBlankNode()`) and those created with an explicit ID. If you try and create a Blank Node with the same explicit ID as an automatically assigned ID you will get back a different Blank Node ID. If you create an anonymous Blank Node you need to hold onto the reference to it as long as you want to use that Blank Node. You can get around this by using `GetBlankNode(String id)` to return the Blank Node with the given ID provided it exists in the Graph

### Literal Nodes

Literal Nodes are used to refer to actual data values.
Values may be plain, language specific or typed. A plain literal is simply textual content while a language specific literal is textual content with a language specified in the form of a country code eg. en-GB, en-US, fr.
Finally a typed literal is textual content associated with a Data Type URI which indicates the type of the data represented by the literal.
Note that a typed literal's Data Type does not guarantee that the content of that literal will be of that type.

A [ILiteralNode](xref:VDS.RDF.ILiteralNode) is constructed as follows:

```csharp
// Standalone plain literal
ILiteralNode simple = new LiteralNode("simple");

// Standalone language-specified literal
ILiteralNode hello = new LiteralNode("hello","en");
ILiteralNode bonjour = new LiteralNode("bonjour","fr");

// Standalone datatyped literals
//You'll need to be using the VDS.RDF.Parsing namespace to reference the constants used here
ILiteralNode one = g.CreateLiteralNode("1", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
ILiteralNode t = g.CreateLiteralNode("true", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));

// Other options require a Graph/NodeFactory
IGraph g = new Graph();

//Create a Plain Literal
ILiteralNode plain = g.CreateLiteralNode("some value");

//Create some Language Specified Literal
ILiteralNode goodbye = g.CreateLiteralNode("goodbye","en");
ILiteralNode aurevoir = g.CreateLiteralNode("au revior","fr");

//Create some typed Literals
ILiteralNode two = g.CreateLiteralNode("2", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
ILiteralNode f = g.CreateLiteralNode("false", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
```

> [!NOTE]
> When you create a literal node using a `Graph` or `NodeFactory`'s `CreateLiteralNode` method, any langauge tag you provide will be validated according to that factory's langauge tag validation mode.
> Creating a `LiteralNode` using its public constructor does not provide this additional layer of validation.
> For more information about the langauge tag validation please refer to [Node Factory](node_factory.md).

### Triple Nodes

Triple Nodes are used in RDF-Star graphs to refer to an RDF statement. 
The value of a Triple Node is a triple (see below for more guidance on creating triples).

A [ITripleNode](xref:VDS.RDF.ITripleNode) can be constructed as follows:

```csharp
// Assuming that `triple` is a variable of type Triple

// Create standalone triple node with the public constructor:
var standalone = new TripleNode(triple);

// Create a triple node with a NodeFactory
IGraph g = new Graph();
var tn = g.CreateTripleNode(triple);
```

## Triples

A Triple is the basic unit of RDF data, nodes on their own have no meaning but used in a Triple form a statement which asserts some knowledge. A Triple is formed of a Subject, Predicate and Object. It is interpreted as stating that some Subject is related to some Object by a relationship specified by the Predicate. The components of the Triple class are Nodes which means that any of the Node classes discussed in the previous section can be used in any of the positions.

In practice the RDF specification restricts which types of Node can appear in which position but since some advanced RDF syntaxes like Notation 3 extend the specification and relax these rules so we allow for any Node type in any position, except for `ITripleNode` which can only appear as the subject or object of a triple.


A [Triple](xref:VDS.RDF.Triple) can be constructed and asserted into a Graph as follows:

```csharp

//Need a Graph first
IGraph g = new Graph();

//Create some Nodes
IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"));
IUriNode createdBy = g.CreateUriNode(UriFactory.Create("http://example.org/createdBy"));
ILiteralNode robVesse = g.CreateLiteralNode("Rob Vesse");

//Assert this Triple
Triple t = new Triple(dotNetRDF, createdBy, robVesse);
g.Assert(t);
```

> [!NOTE]
> Prior to version 3.0, a triple had to be created using nodes from the same graph.
> From version 3.0 onwards a triple can be created with nodes from any Graph or NodeFactory and can be asserted on any Graph regardless of the original source of the nodes.

If you wish to remove a Triple from a Graph you create it in the same manner shown above and call the `Retract(...)` method which like the `Assert(...)` method takes a single Triple or an enumerable of Triples.

Triples also have a property named `Context` which can be used to store arbitrary application specific data in a class which implements the [ITripleContext](xref:VDS.RDF.ITripleContext) interface.

> [!WARNING]
> Prior to version 3.0 a triple also had a `Graph` property which referenced the Graph that it was created for.
> This value was not necessarily always the same as the reference to the `Graph` that the triple was asserted in, but some old code may make that assumption.
> The property has been removed in 3.0 and code making use of this property should be updated to track the source graph of a triple by other means.

## Triple Store

A Triple Store represents a collection of Graphs and is used to work with larger quantities of RDF. Triple Stores are designed to be less tangible than Graphs in terms of their interface and implementations. While a specific implementation may represent some Triple Store it does not necessarily provide direct access to all the data in that Store i.e. a Triple Store is not necessarily in-memory.

Triple Stores are actually based on several interfaces, the base interface for them is [ITripleStore](xref:VDS.RDF.ITripleStore). This interface defines properties and methods relating to adding & removing Graphs and the retrieval of Graphs and Triples contained in the Store.

If you have a Store that is partially/fully in-memory then it will implement the [IInMemoryQueryableStore](xref:VDS.RDF.IInMemoryQueryableStore) interface which is an extension of [ITripleStore](xref:VDS.RDF.ITripleStore). The [IInMemoryQueryableStore](xref:VDS.RDF.IInMemoryQueryableStore) interface defines a swathe of additional methods which provide for various forms of selection of Triples from the Store, it also provides two query methods which allow for executing SPARQL queries on the Triple Store using the libraries in-memory SPARQL implementation. The library contains a class called [TripleStore](xref:VDS.RDF.TripleStore) which is the basic implementation of this interface.

If you have a Store which is a representation of some backing Store which provides its own SPARQL implementation and where the data will not be loaded in-memory (by dotNetRDF at least) then it will implement the [INativelyQueryableStore](xref:VDS.RDF.INativelyQueryableStore) interface. This interface also extends [ITripleStore](xref:VDS.RDF.ITripleStore) but it only provides a single query method for executing SPARQL queries on the Store.

