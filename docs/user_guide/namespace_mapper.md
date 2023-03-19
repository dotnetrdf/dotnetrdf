# Namespace Mapper

Namespaces are a concept from XML which are used in the Semantic Web as the standard way of simplifying URIs into QNames. A QName (Qualified Name) is a way of abbreviating a URI in a simple readable form which makes it easier for humans to understand the data while still being machine readable. e.g.

`http://www.w3.org/1999/02/22-rdf-syntax-ns#type` can be represented as `rdf:type`

More generally in RDF there is the concept of a PName (Prefixed Name), this is an extension to XML QNames which is more flexible and allows more abbreviations since it does not have to conform to the strict XML naming rules.

A Prefixed Name is composed of two parts:

* The Namespace Prefix appears before the colon and may be empty.
* The Local Name appears after the colon and may be empty.

> [!NOTE]
> Exact definitions of what is and isn't a valid Prefixed Name vary depending on the serialization syntax being used.

A prefixed name is resolved into a URI by concatenating the Namespace URI with the Local Name. A Namespace Prefix is used to refer to a Namespace URI, prefixes must be associated with URIs before they can be used. The following example shows a fragment of Turtle which defines some namespaces and then uses them to define a Triple.

```
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix ex: <http://example.org/>.

ex:this rdf:type ex:Example.
```

The above represents the following Triple:

```
<http://example.org/this> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://example.org/Example>
```

## Empty Namespace Prefix

The empty namespace prefix has a special meaning, it either refers to the default Namespace (a Namespace defined with an empty prefix) or if no default Namespace is defined it **may** refer to the Base URI of the Graph.  The usage of the empty namespace prefix does vary between different serialization syntaxes.

## The Namespace Mapper

In order that prefixed names can be resolved into URIs various parts of the library must maintain a table of associations of Namespace Prefixes and their corresponding Namespace URIs. This function is provided by the `VDS.RDF.INamespaceMapper` interface.

The most common place to encounter this interface is when working with graphs, any implementation of the [`VDS.RDF.IGraph`](xref:VDS.RDF.IGraph) interface provides a property called [`NamespaceMap`](xref:VDS.RDF.INodeFactory.NamespaceMap) which returns the [`INamespaceMapper`](xref:VDS.RDF.INamespaceMapper) associated with the graph.

> [!NOTE]
> This property is actually inherited from the [`INodeFactory`](xref:VDS.RDF.INodeFactory) interface which is described in more detail on the [Node Factory](node_factory.md) page .

The [`INamespaceMapper`](xref:VDS.RDF.INamespaceMapper) can be used to introduce new Namespaces into the graph to allow you to more easily define URI Nodes by using QNames e.g.

```csharp
IGraph g = new Graph();

//Define the Namespaces we want to use
g.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org"));

//Define the same Triple as the previous example
UriNode exThis = g.CreateUriNode("ex:this");
UriNode rdfType = g.CreateUriNode("rdf:type");
UriNode exExample = g.CreateUriNode("ex:Example");
g.Assert(new Triple(exThis, rdfType, exExample));
```

You can also use the [`GetNamespaceUri(String prefix)`](xref:VDS.RDF.INamespaceMapper.GetNamespaceUri(System.String)) and [`GetPrefix(Uri uri)`](xref:VDS.RDF.INamespaceMapper.GetPrefix(System.Uri))) methods to retrieve Namespace URIs based on a Prefix and vice versa.

Another important feature of the [`INamespaceMapper`](xref:VDS.RDF.INamespaceMapper) is its ability to reduce URIs to prefixed names via the [`ReduceToQName`](xref:VDS.RDF.INamespaceMapper.ReduceToQName(System.String,System.String@,System.Func{System.String,System.Boolean})) function. This function allows you to take a URI and attempts to turn it into a prefixed name returning true if this succeeds and setting the out variable `qname` to be the prefixed name. All of the writer classes provided in the library make use of this function to help them generate prefixed names for output. The function also optionally takes a validation function as input, if provided the validation function will be called to validate any generated QName (returning true if the QName is valid and false if it is not). By default QName validation rejects any QNames that contain '/' or '#' characters (to ensure compatibility with XML).

## Merging Namespace Maps

If you wish to combine two namespaces maps then you may wish to use the [`Import(INamespaceMapper nsmap)`](xref:VDS.RDF.INamespaceMapper.Import(VDS.RDF.INamespaceMapper)) method which imports namespaces from one map into another.