# Extension Methods 

The library provides a number of extension methods that can be used to simplify some common tasks and marginally decrease the amount of code you have to write. These extension methods are located in several static class called [Extensions](xref:VDS.RDF.Extensions), [LiteralExtensions](xref:VDS.RDF.LiteralExtensions), [GraphExtensions](xref:VDS.RDF.GraphExtensions) and [TripleStoreExtensions](xref:VDS.RDF.TripleStoreExtensions) in the `VDS.RDF` namespace so anywhere you reference `VDS.RDF` you have the option of using these methods.

> [!NOTE]
> To avoid being over-long and dull, this document does not cover all of the extension methods available in these classes. 
> Interested readers are encouraged to follow the links above to the class API docs.

## Assert 

The [Assert(this IGraph g, INode subj, INode pred, INode obj)](xref:VDS.RDF.Extensions.Assert(VDS.RDF.IGraph,VDS.RDF.INode,VDS.RDF.INode,VDS.RDF.INode)) method is a shorthand way of asserting a single Triple in a Graph without having to instantiate a Triple object yourself e.g.

```csharp

//Create a Graph and set it's Base URI
IGraph g = new Graph();
g.BaseUri = new Uri("http://example.org");

//Create some Nodes
IUriNode thisGraph = g.CreateUriNode();
IUriNode rdfType = g.CreateUriNode("rdf:type");
IUriNode example = g.CreateUriNode("http://example.org/Example");

//Assert a Triple using the Graph's Assert method
g.Assert(new Triple(thisGraph, rdfType, example));

//Assert a Triple using the extension method
g.Assert(thisGraph, rdfType, example);
```

Note that these methods of asserting are semantically identical, they both assert the same Triple in the Graph.

## GetEnhancedHashCode 

The [`GetEnhancedHashCode(this Uri u)`](xref:VDS.RDF.Extensions.GetEnhancedHashCode(System.Uri)) method is used to generate Hash Codes for Uri objects. This method is used internally in favour of the Uri classes own `GetHashCode()` method since the .Net implementation doesn't account for fragement identifiers in computing hash codes.

In most applications this wouldn't matter since the fragment identifier is usually insignificant but in the case of the Semantic Web fragment identifiers are an important part of URIs. Therefore we provide our own hash code function which does use the fragment identifier in computing hash codes.

## LoadFromFile, LoadFromUri, LoadFromEmbeddedResource and LoadFromString 

These extension methods for [`IGraph`](xref:VDS.RDF.IGraph) instances all provide shortcuts for invoking the various static loader classes that can be used to load RDF from various common sources as detailed in the [Reading RDF]reading_rdf.md#reading-rdf-from-common-sources) documentation.


## Retract 

The [`Retract(this IGraph g, INode subj, INode pred, INode obj)`](xref:VDS.RDF.Extensions.Retract(VDS.RDF.IGraph,VDS.RDF.INode,VDS.RDF.INode,VDS.RDF.INode)) method the partner of the `Assert(...)` extension method and like that method is simply a shorthand way of retracting a Triple without having to explicitly instantiate it e.g.

```csharp
IGraph g = new Graph();

//Create some Nodes
IUriNode thisGraph = g.CreateUriNode();
IUriNode rdfType = g.CreateUriNode("rdf:type");
IUriNode example = g.CreateUriNode("http://example.org/Example");

//Retract a Triple using the Graph's Retract method
g.Retract(new Triple(thisGraph, rdfType, example));

//Retract a Triple using the extension method
g.Retract(thisGraph, rdfType, example);
```

Again both methods of retracting are semantically identical.

## ToLiteral 

The [`ToLiteral(...)`](xref:VDS.RDF.LiteralExtensions) methods are a whole family of methods which can be used to turn common .Net types into their equivalent [`ILiteralNode`](xref:VDS.RDF.ILiteralNode) representations.  Methods are provided for `boolean`, `int`, `long`, `byte`, `sbyte`, `decimal`, `float`, `double`, `DateTime`, `DateTimeOffset`, `TimeSpan`.

Some overloads may allow for control over the exact literal generated, for example the `DateTime` has an overload which allows specifying whether to preserve precisely i.e. include fractional seconds

## WithSubject, WithPredicate and WithObject 

The [`WithSubject(this IEnumerable<Triple>, INode Subject)`](xref:VDS.RDF.Extensions.WithSubject(System.Collections.Generic.IEnumerable{VDS.RDF.Triple},VDS.RDF.INode)), [`WithPredicate(IEnumerable<Triple>, INode Predicate)`](xref:VDS.RDF.Extensions.WithPredicate(System.Collections.Generic.IEnumerable{VDS.RDF.Triple},VDS.RDF.INode)), [`WithObject(this IEnumerable<Triple>, INode Object)`](xref:VDS.RDF.Extensions.WithObject(System.Collections.Generic.IEnumerable{VDS.RDF.Triple},VDS.RDF.INode)) methods are used to filter any `IEnumerable<Triple>` to find only the Triples that have the matching Subject, Predicate or Object.