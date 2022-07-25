# Typed Valued and Lists

While RDF is a powerful data model capable of representing any data you care to encode in it the RDF serialization of some typed values and lists can be somewhat arcane. They can also be unwieldly to access and manipulate using just the APIs we've showed you so far in this User Guide. This document covers some helpful ways the library helps you work with these things in a more user friendly way.

## Encoding Values into RDF

For common types values e.g. integers, booleans, date times etc. RDF uses the XML schema datatypes, these can be directly created by a user using explicit code like the following:

```csharp
//Assuming we have a Graph in variable g already
ILiteralNode value = g.CreateLiteralNode("12345", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
```

While this is relatively simple it's a pain to type out particularly if you have a lot of typed values you want to represent. To make the encoding of typed values into RDF the library offers a number of extension methods in the [`VDS.RDF.LiteralExtensions`](xref:VDS.RDF.LiteralExtensions) class which provide `ToLiteral()` methods which you can apply on a variety of .Net types to get the equivalent RDF encoding of them e.g.

```csharp
//Assuming we have a Graph in variable g already
INode intNode = (12345).ToLiteral(g);
INode dateNode = DateTime.Now.ToLiteral(g);
```

This yields code that is much more readable and in the case of types like `DateTime` and `TimeSpan` where the RDF encodings have to be in a precise format remove any possibility of user error in encoding the values as appropriate literals.

## Decoding Values from RDF

To decode values from RDF you can make use of the [`VDS.RDF.Nodes`](xref:VDS.RDF.Nodes) namespace, while this is primarily used internally in our SPARQL engine it is also very useful to the end user.
This namespace primarily provides a [`AsValuedNode()`](xref:VDS.RDF.Nodes.ValuedNodeExtensions.AsValuedNode(VDS.RDF.INode)) extension method which will give you back a [`VDS.RDF.Nodes.IValuedNode`](xref:VDS.RDF.Nodes.IValuedNode) instance.
A [`IValuedNode`](xref:VDS.RDF.Nodes.IValuedNode) is simply a node that has a strongly typed value associated with it and provides a number of methods to retrieve the value of a node as a strongly typed .Net type e.g.

```csharp
// Assuming we have a Node in the variable n already
IValuedNode value = n.AsValuedNode();

//Get the integer value
long i = value.AsInteger();
```

## Manipulating RDF Lists

RDF Lists of Collections are a way of expressing lists in RDF but the serialization into triples is somewhat cumbersome. For example the following Turtle snippet expresses a simple list:

```
@prefix ex: <http://example.org/ns#> .

ex:subj ex:pred ( 1 2 3 ) .
```

But this is just syntactic sugar for the following triples:

```
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix ex: <http://example.org/ns#> .

ex:subj ex:pred _:b1 .
_:b1 rdf:first 1 .
_:b1 rdf:rest _:b2 .
_:b2 rdf:first 2 .
_:b2 rdf:rest _:b3 .
_:b3 rdf:first 3 .
_:b3 rdf:rest rdf:nil .
```

As you can probably appreciate accessing that purely through the triple-centric APIs we showed you in the User Guide so far is going to be very ugly.

To ameliorate this problem we provide a number of methods in the [`VDS.RDF.Extensions`](xref:VDS.RDF.Extensions) class that allow you to manipulate RDF lists in a user friendly manner.

Accessing a list requires a reference to the graph and the list root, i.e. the Node in the graph that represents the first item in the list.
All the subsequent examples will assume the RDF graph given above is the data being used and is available in a variable `g` and that the list root is in the variable `root`.

### Retrieving a List

You can retrieve a list in several ways, the most common of which is used to retrieve the items of your collection e.g.

```csharp
//This will give us the Nodes for 1, 2 and 3
IEnumerable<INode> ns = g.GetListItems(root);
```

In some cases you may want to retrieve the intermediate nodes of the list rather than the items themselves e.g.

```csharp
//This will give us the Nodes _:b1, _:b2 and _:b3
IEnumerable<INode> ns = g.GetListNodes(root);
```

Or you may wish to get all the triples that compose the list:

```csharp
//This will give us all the rdf:first and rdf:rest triples associated with the list
IEnumerable<Triple> ts = g.GetListTriples(root);
```

### Asserting and Retracting Lists

We also provide methods that allow you to easily assert/retract a list. Asserting a list requires an enumeration of nodes to act as the item of the lists, you can either use an existing Node as the list root or have a new list root generated for you e.g.

```csharp
INode newRoot = g.AssertList(new List<INode>() { (true).ToLiteral(g), (false).ToLiteral(g) });
//Now we can use newRoot as we wish in our graph
```

Retracting a list is much simpler provide we know the list root:

```csharp
//Retracts all the triples associated with the list
g.RetractList(root);
```

### Adding and Removing Items from Lists

You can add and remove items from lists using convenience methods also. To add new items to the end of a list you can do the following:

```csharp
g.AddToList(root, new List<INode>() { (true).ToLiteral(g), (false).ToLiteral(g) });
```

To remove items from a list regardless of where in the list they occur do the following:

```csharp
g.RemoveFromList(root, new List<INode>() { (true).ToLiteral(g), (false).ToLiteral(g) });
```

Note that if an item exists in the list multiple times all occurrences of it are removed by this method.
