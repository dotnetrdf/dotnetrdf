# Equality and Comparison in dotNetRDF

This page covers how equality and comparison are defined for the various core classes in the library and in some cases how to change the behavior as necessary.

## Nodes

As discussed in the [Core Concepts](core_concepts.md) there are various types of node representing the different types of RDF term that may exist and be represented within a RDF graph.

The [RDF Concepts and Abstract Syntax (W3C Specification)](http://www.w3.org/TR/rdf-concepts/) defines how and when terms are considered equal and so the definition of `Equals()` for nodes is based upon this.

### IUriNode

URI Nodes are considered equal if they represent equal URIs, URI equality is defined in [RFC 3986](http://www.ietf.org/rfc/rfc3986.txt).  Primarily this requires each component of a URI to be equal on a character by character basis with some scheme specific extensions, for example the default port for a HTTP URI is 80 so that may be omitted e.g.

```csharp
IGraph g = new Graph();

IUriNode a = g.CreateUriNode(UriFactory.Create("http://example.com/test"));
IUriNode b = g.CreateUriNode(UriFactory.Create("http://example.com:80/test"));

//This prints true because the URIs are equal according to URI equality rules
Console.WriteLine(a.Equals(b));
```

Comparison on URIs is a lexicographical ordering by Unicode code point taking into account URI components.  So they are ordered first by scheme, then by authentication details, then by host and so on.  This means the ordering will group similar URIs together which is typically the desired behavior.

### IBlankNode

Blank Nodes are scoped specifically to a Graph, Blank Nodes are only considered equal if they have identical IDs.

> [!WARNING]
> In versions of dotNetRDF prior to 3.0, Blank Nodes were considered equal only if they had identical IDs **and** where created by the same `IGraph` instance. In dotNetRDF 3.0, nodes are no longer scoped to a graph and so only ID comparison is used.

```csharp
IGraph g = new Graph();
IGraph h = new Graph();

IBlankNode a = g.CreateBlankNode("test");
IBlankNode b = h.CreateBlankNode("test");

// Under dotNetRDF 1.x and 2.x this prints false since although they have the same ID they come from different graphs
// Under dotNetRDF 3.x this prints true since the nodes are no longer scoped to a graph.
Console.WriteLine(a.Equals(b));
```

Comparison on blank nodes is a simple lexicographical ordering by Unicode code point on the assigned label, it does not take into account source graph so blank nodes which are non-equal may be considered equal for the purposes of ordering.

### ILiteralNode

Literal Node equality is somewhat more complex but basically follows the following rules:

1. If a Language Specifier is present both Nodes must have an identical Language Specifier
1. If a Data Type URI is present both Nodes must have an identical Data Type URI
1. The String value of the Literal must match on a character by character basis (using Ordinal comparison)

While this is an exact implementation of the official RDF Specification in applications this may not always be the desired behaviour. It is possible for literals representing the same actual value to be encoded in different ways, consider the following example:

```csharp
//Need a Graph first
IGraph g = new Graph();

//Create two Literal Nodes which both represent the Integer 1
//You'll need to be using the VDS.RDF.Parsing namespace to reference the constants used here
ILiteralNode one1 = g.CreateLiteralNode("1", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
ILiteralNode one2 = g.CreateLiteralNode("0001", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));

//Are they equal
bool equal = one1.Equals(one2);

//Prints false since the string values are not equal
Console.WriteLine(equal.ToString());
```

If you want to compare Literals based on typed Value rather than literal Value then you can change the (static) [`LiteralEqualityMode`](xref:VDS.RDF.EqualityHelper.LiteralEqualityMode) setting of the [`EqualityHelper`](xref:VDS.RDF.EqualityHelper) static class. above code as follows:

```csharp
//Use Loose Literal Equality Mode
EqualityHelper.LiteralEqualityMode = LiteralEqualityMode.Loose;

//Are they equal
bool equal = one1.Equals(one2);

//Prints true since the typed values are equal
Console.WriteLine(equal.ToString());
```

Be careful when using this since [EqualityHelper.LiteralEqualityMode](xref:VDS.RDF.EqualityHelper.LiteralEqualityMode) is a global static option which affects all Node equality calculations involving Literal Nodes and adds performance overheads.
Reset this to the default value of `LiteralEqualityMode.Strict` as soon as you no longer need it.
Note also that this only works for a limited range of standard data types.

The alternative to changing the literal equality mode is to call `CompareTo()` method instead and check that the value returned equals zero since literal node comparison is akin to loose equality.

Comparison on literal nodes starts with a basic ordering like so:

1. Literals with no language specifier or data type are ordered lowest
1. Literals with a language specifier are ordered next
1. Literals with a data type are ordered last

For literals with no language specifier or data type the ordering within them is by lexical value lexicographically by unicode code point.  Literals with a language specifier are ordered lexicographically on their language specifier first and then by their lexical value.

Data typed literals have the most complex ordering, firstly they are ordered by data type URI using the same ordering for URIs as used for URI nodes.  Then their ordering depends on whether they are a supported or unsupported type.  Supported types like boolean, integer, decimal, double, date time etc will attempt to parse the lexical value into the appropriate .Net type.  If that parsing is successful then the nodes are ordered within their type using the standard ordering for the equivalent .Net type, values which are not valid for the given datatype are sorted lexicographically after valid values.

### Alternative Node Orderings

While node comparison produces a good sort of nodes because it may need to parse a lot of values it can prove quite costly on large amounts of data.  If you just need a fast sort on nodes you may wish to use the [`FastNodeComparer`](xref:VDS.RDF.FastNodeComparer) instead.  This simply orders nodes by their types and simplifies the literal node comparisons to be purely lexicographical on their Unicode code points.

## Triples

Triple equality is relatively simple, triples are considered equal if their subjects, predicates and objects are all equal.  The graph that a triple belongs to is not considered as part of the equality check.

### Alternate Triple Orderings

Often it is useful for presentation or other purposes to order triples in a specific way.
The library provides a small set of alternate triple comparison classes and a base class you can extend to implement your own comparison logic.

| Triple Comparer | Description |
|-----------------|-------------|
| [`BaseTripleComparer`](xref:VDS.RDF.BaseTripleComparer) | An abstract base class for creating your own triple comparison logic. |
| [`FullTripleComparer`](xref:VDS.RDF.FullTripleComparer) | Compares triples first by subject, then by predicate, then by object. |
| [`SubjectComparer`](xref:VDS.RDF.SubjectComparer) | Compares triples on their subject only. |
| [`PredicateComparer`](xref:VDS.RDF.PredicateComparer) | Compares triples on their predicate only. |
| [`ObjectComparer`](xref:VDS.RDF.ObjectComparer) | Compares triples on the object only. |
| [`SubjectPredicateComparer`](xref:VDS.RDF.SubjectPredicateComparer) | Compare triples by subject and then by predicate. |
| [`SubjectObjectComparer`](xref:VDS.RDF.SubjectObjectComparer) | Compare triples by subject and then by object. |
| [`PredicateObjectComparer`](xref:VDS.RDF.PredicateObjectComparer) | Compare triples by predicate and then by object. |
| [`ObjectSubjectComparer`](xref:VDS.RDF.ObjectSubjectComparer) | Compare triples by object and then by subject. |

## Graphs

Graph equality is one of the more complex forms of equality in the library both to understand and calculate.  Graph equality in RDF is defined as graph isomorphism i.e. every triple in one must be present in the other accounting for blank node labels being different.  This means that in the worst case determining that two graphs are equal or non-equal can be an exponentially hard brute force calculation.

Our graph equality algorithms are provided in a separate class called [VDS.RDF.GraphMatcher](xref:VDS.RDF.GraphMatcher) so they can be reused by custom graph implementations.  They attempt to determine whether two graphs are equal by undertaking a series of steps to reduce the isomorphism calculation as much as possible and limit the problem space of possible mappings that must be considered.

Another useful class in this area is the [VDS.RDF.GraphDiff](xref:VDS.RDF.GraphDiff) class which can be used to determine what the differences between two graphs are (including whether they are equal).

There is no comparison operators defined for graphs currently.