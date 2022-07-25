# Dynamic API 

## Introduction

The Dynamic API allows developers to access and update RDF graphs and SPARQL results sets as a collection of C# dynamic objects. The API is capable of mapping between known literal datatypes and .NET datatypes and uses triple predicates to define properties on C# dynamic objects.

## Accessing the Dynamic API

The extension method [`IGraph.AsDynamic()`](xref:VDS.RDF.Dynamic.DynamicExtensions.AsDynamic(VDS.RDF.IGraph,System.Uri,System.Uri)) (found in [`VDS.RDF.Dynamic.DynamicExtensions`](xref:VDS.RDF.Dynamic.DynamicExtensions)) allows a dotNetRDF graph to be accessed as a dynamic collection. This method accepts two optional parameters:

* `subjectBaseUri` - a base URI that will be used to resolve relative subject URI references in dynamic API calls. If not defined, this defaults to the base URI of the graph that is being accessed.
* `predicateBaseUri` - a base URI that will be used to resolve relative predicate URI references in dynamic API calls. If not defined, this defaults to the `subjectBaseUri` (or to the graph base URI if `subjectBaseUri` is also undefined).

### Accessing the nodes of a Graph

The graph dynamic collection is a dynamic object whose properties are the subject nodes in the RDF graph. These nodes can be accessed using index notation or property notation on the dynamic object. As a simple example:

```c#
// This creates the graph that we will access as a dynamic collection
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

// Create the dynamic collection wrapper for the graph, with an overridden base URI for the subject nodes
var d = g.AsDynamic(UriFactory.Create("urn:"));

// Access a the <urn:s> subject node as a property of the dynamic collection
var s = d.s; // s will be an INode instance, the property name "s" is treated as a relative URI

// Access the <urn:s> subject node as an indexed member of the dynamic collection
s = d["s"]; // d["urn:s"] also works
```

The API allows new data to be added to the graph just as easily. The following creates an equivalent graph to
the one loaded in the previous example.

```c#
var graph = new Graph();
var d = graph.AsDynamic(UriFactory.Create("urn:"));
d["s"] = new { p = "o" }; // String property value becomes an RDF string literal
```

To add more properties to an existing node, you can use the Add method instead:

```c#
// This creates the graph that we will access as a dynamic collection
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
var d = graph.AsDynamic(UriFactory.Create("urn:"));
d["s"].Add(new { p = "o2" });
```

If a subject node does not exist, it will be created as needed, and then you can use the Add method of the subject node.

```c#
var graph = new Graph();
var d = graph.AsDynamic(UriFactory.Create("urn:"));
var s = d.s; // A new subject node <urn:s>
s.Add({ p = "o" };) // Add a property to the subject node
```

You can just as easily remove a subject node from the graph:

```c#
// This creates the graph that we will access as a dynamic collection
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
var d = graph.AsDynamic(UriFactory.Create("urn:"));
d.Remove("s");
// Graph is now empty
```

### Accessing nodes

The properties of a node returned by the Dynamic API can be accessed as properties of the node dynamic object:

```c#
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
var d = graph.AsDynamic(UriFactory.Create("urn:"));
var s = d.s; // s is a dynamic node object
var o = s.p; // == "o"; s["p"] also works as does s["urn:p"]
```

Setting a node property is supported through direct assignment:

```c#
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
var d = graph.AsDynamic(UriFactory.Create("urn:"));
var s = d.s;
s["p"] = "o2";
// Graph content is now <urn:s> <urn:p> "o2"
```

Add and remove is also supported and node property values can be collections of values:

```c#
var g = new Graph();
g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
var d = graph.AsDynamic(UriFactory.Create("urn:"));
var s = d.s;
s.p.Add("o2"); // or s["p"].Add("o2"), or s.Add("p", "o2")
// Graph content is now two triples:
// <urn:s> <urn:p> "o" .
// <urn:s> <urn:p> "o2" .
var objectCount = s.p.Count(); // == 2
```

## Nodes as dictionaries

- [Simple](#simple)
- [Resource node recursion](#resource-node-recursion)
- [Literal node translation](#literal-node-translation)
- [Dictionary key conversion](#dictionary-key-conversion)
- [Querying node dictionaries](#querying-node-dictionaries)

### Simple

Nodes in a graph can be thought of as dictionaries (maps, hashes) where keys are predicates of outgoing statements (where given node is subject) and values are collections of objects of those statements.

For example the following graph
```turtle
@prefix : <http://example.com/> .

:s
  :p1
    :o1 ,  # t1
    "o2" , # t2
    [] ;   # t3
  :p2
    :o4 ,  # t4
    "o5" , # t5
    [] .   # t6
```

![image of graph](~/images/dynamic_api/1.svg)

is eqivalent to this dictionary:
```csharp
var f = new NodeFactory();
var p1 = f.CreateUriNode(new Uri("http://example.com/p1"));
var o1 = f.CreateUriNode(new Uri("http://example.com/o1");
var o2 = f.CreateLiteralNode("o2");
var o3 = f.CreateBlankNode();
var p2 = f.CreateUriNode(new Uri("http://example.com/p2"));
var o4 = f.CreateUriNode(new Uri("http://example.com/o4");
var o5 = f.CreateLiteralNode("o5");
var o6 = f.CreateBlankNode();

var s = new Dictionary<INode, ICollection<INode>>
{
  {
    p1,
    new INode[]
    {
      o1, // t1
      o2, // t2
      o3, // t3
    }
  },
  {
    p2,
    new INode[]
    {
      o4, // t4
      o5, // t5
      o6, // t6
    }
  },
};
```

[top](#nodes-as-dictionaries)

### Resource node recursion

In the case of resource objects (IRI or blank nodes, not literals), the process can be repeated recursively, i.e. items in the value collection are dictionaries themselves.

For example the following graph
```turtle
@prefix : <http://example.com/> .

:s1
  :p
    :s2 ,  # t1
    _:s3 . # t2

:s2
  :p
    "o1" . # t3

_:s3
  :p
    "o2" . # t4
```

![image of graph](~/images/dynamic_api/2.svg)

is eqivalent to this dictionary:
```csharp
var f = new NodeFactory();
var p = f.CreateUriNode(new Uri("http://example.com/p"));
var o1 = f.CreateLiteralNode("o1");
var o2 = f.CreateLiteralNode("o2");

var s1 = new Dictionary<INode, ICollection<object>>
{
  {
    p,
    new object[]
    {
      new Dictionary<INode, ICollection<object>> // t1
      {
        p,
        new INode[]
        {
          o1,                                    // t3
        }
      },
      new Dictionary<INode, ICollection<object>> // t2
      {
        p,
        new INode[]
        {
          o2,                                    // t4
        }
      }
    }
  },
};
```

[top](#nodes-as-dictionaries)

### Literal node translation 

Literal objects on the other hand can be translated to their corresponding primitive types.

For example the following graph
```turtle
@prefix : <http://example.com/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s
  :p
    "o" ,                                              # t1
    true ,                                             # t2
    9223372036854775807 ,                              # t3
    1.79769313486232E+308 ,                            # t4
    "255"^^xsd:unsignedByte ,                          # t5
    "9999-12-31T23:59:59.999999+00:00"^^xsd:dateTime , # t6
    "79228162514264337593543950335"^^xsd:decimal ,     # t7
    "3.402823E+38"^^xsd:float ,                        # t8
    "P10675199DT2H48M5.4775807S"^^xsd:duration .       # t9
```

![image of graph](~/images/dynamic_api/3.svg)

is eqivalent to this dictionary:
```csharp
var f = new NodeFactory();
var p = f.CreateUriNode(new Uri("http://example.com/p"));

var s = new Dictionary<INode, ICollection<object>>
{
  {
    p,
    new object[]
    {
      "o",                     // t1
      true,                    // t2
      long.MaxValue,           // t3
      double.MaxValue,         // t4
      byte.MaxValue,           // t5
      DateTimeOffset.MaxValue, // t6
      decimal.MaxValue,        // t7
      float.MaxValue,          // t8
      TimeSpan.MaxValue        // t9
    }
  },
};
```

[top](#nodes-as-dictionaries)

### Dictionary key conversion

When looking at nodes as dictionaries, keys represent statement predicates, and so they are ultimately Uri nodes.

```csharp
var key = f.CreateUriNode(new Uri("http://example.com/p"));
```

But it's safe to assume that plain Uris used as node dictionary keys also correspond to statement predicates, so they can be automatically converted to Uri nodes.

```csharp
var key = new Uri("http://example.com/p");
```

Furthermore, plain strings used as node dictionary keys can be safely assumed to be Uri strings of statement predicate nodes, so they can also be automatically converted.

```csharp
var key = "http://example.com/p";
```

Assuming a base Uri, relative Uris can also be used as node dictionary indices.

```csharp
// assuming @base <http://example.com/>
var key = new Uri("p", UriKind.Relative);
```

Again assuming a base Uri, node dictionary key strings that are not absolute Uris can be interpreted as relative Uris of predicate nodes.

```csharp
// assuming @base <http://example.com/>
var key = "p";
```

Finally, string keys that look like QNames can be expanded against a graph's namespace manager.

```csharp
// assuming @prefix ex: <http://example.com/>
var key = "ex:p";
```

The QName notation also works for the default (empty) prefix.

```csharp
// assuming @prefix : <http://example.com/>
var key = ":p";
```

To summarise, the following graph

```turtle
@prefix : <http://example.com/> .
@prefix ex: <http://example.com/> .
@base <http://example.com/> .

<s> <p> "o" .
```

![image of graph](~/images/dynamic_api/4.svg)

is equivalent to all of these dictionaries:

```csharp
var objects = new [] { "o" };

// Uri node
var s = new Dictionary<INode, ICollection<object>> { {
  f.CreateUriNode(new Uri("http://example.com/p")),
  objects } };

// Absolute Uri
var s = new Dictionary<Uri, ICollection<object>> { {
  new Uri("http://example.com/p"),
  objects } };

// Relative Uri
var s = new Dictionary<Uri, ICollection<object>> { {
  new Uri("p", UriKind.Relative), 
  objects } };

// Absolute Uri string
var s = new Dictionary<string, ICollection<object>> { {
  "http://example.com/p", 
  objects } };

// Relative Uri string
var s = new Dictionary<string, ICollection<object>> { {
  "p", 
  objects } };

// QName
var s = new Dictionary<string, ICollection<object>> { {
  "ex:p", 
  objects } };

// QName with default (empty) prefix
var s = new Dictionary<string, ICollection<object>> { {
  ":p", 
  objects } };
```

[top](#nodes-as-dictionaries)

### Querying node dictionaries

Assuming the following graph

```turtle
:s1
  :p1
    "o1" ;     # t1
    :s2 ,      # t2
    [          # t3
      :p2
        "o2"   # t4
    ] .

:s2 :p3 "o3" . # t5
```

![image of graph](~/images/dynamic_api/5.svg)

one could get the values of the literal objects at `t1`, `t4` and `t5` like so:

```csharp
var o1 = s1.Graph
  .GetTriplesWithSubjectPredicate(s1, s1.Graph.CreateUriNode(new Uri("http://example.com/p1")))
  .Select(t => t.Object)
  .First()
  .AsValuedNode()
  .AsString();

var o3 = s1.Graph
  .GetTriplesWithSubjectPredicate(s1, s1.Graph.CreateUriNode(new Uri("http://example.com/p1")))
  .Skip(1)
  .SelectMany(o => s1.Graph
    .GetTriplesWithSubjectPredicate(t.Object, s1.Graph.CreateUriNode(new Uri("http://example.com/p3")))
    .Select(t => t.Object))
  .Single()
  .AsValuedNode()
  .AsString();

var o3 = g
  .GetTriplesWithSubjectPredicate(s1, s1.Graph.CreateUriNode(new Uri("http://example.com/p1")))
  .Last()
  .SelectMany(o => s1.Graph
    .GetTriplesWithSubjectPredicate(t.Object, s1.Graph.CreateUriNode(new Uri("http://example.com/p2")))
    .Select(t => t.Object))
  .Single()
  .AsValuedNode()
  .AsString();
```

Looking at the same graph as a dictionary, we can obtain the same values like so:

```csharp
var o1 = s1["p1"].First();                // t1
var o3 = s1["p1"].Skip(1)["p3"].Single(); // t2 & t5
var o2 = s1["p1"].Last()["p2"].Single();  // t3 & t4
```

[top](#nodes-as-dictionaries)
