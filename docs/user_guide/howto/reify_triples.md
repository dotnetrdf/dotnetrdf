# Reify Triples 

Reified triples are not special in any sense from dotNetRDF's point of view they are merely triples asserted in a graph.

For example consider the following Turtle fragment:

```turtle
@prefix : <http://example.org> .

:s :p :o .
```

Reifying this graph simply transforms it to the following form:

```turtle
@prefix : <http://example.org> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

:s :p :o .

[] a rdf:Statement ;
   rdf:subject :s ;
   rdf:predicate :p ;
   rdf:object :o .
```

In API terms this simply means you need to assert additional triples e.g.

```csharp
IGraph g = new Graph();
g.NamespaceMap.AddNamespace(String.Empty, new Uri("http://example.org"));

// Create necessary URI nodes
INode rdfType = g.CreateUriNode("rdf:type");
INode rdfStatement = g.CreateUriNode("rdf:Statement");
INode rdfSubj = g.CreateUriNode("rdf:subject");
INode rdfPred = g.CreateUriNode("rdf:predicate");
INode rdfObj = g.CreateUriNode("rdf:object");

INode s = g.CreateUriNode(":s");
INode p = g.CreateUriNode(":p");
INode o = g.CreateUriNode(":o");

// Assert triple
g.Assert(s, p, o);

// Assert reification triples
INode id = g.CreateBlankNode();
g.Assert(id, rdfType, rdfStatement);
g.Assert(id, rdfSubject, s);
g.Assert(id, rdfPredicate, p);
g.Assert(id, rdfObject, o);
```

