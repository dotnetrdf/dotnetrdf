# Load RDF from the Web 

RDF can be loading from web via a URI using the `LoadFromUri()` extension method.  This method exists for both [IGraph](xref:VDS.RDF.IGraph) and [ITripleStore](xref:VDS.RDF.ITripleStore) e.g.

```csharp
IGraph g = new Graph();
g.LoadFromUri(new Uri("http://example.org/file.rdf"));
```

## Loading in a Specific Format 

Occasionally some server may misinterpret the `Accept` header that dotNetRDF normally uses and return a HTML page instead of a RDF graph.  You can force the loader to attempt to retrieve RDF in a specific format by specifying the parser you want to use e.g.

```csharp
IGraph g = new Graph();
g.LoadFromUri(new Uri("http://example.org/file.rdf"), new TurtleParser());
```

Learn more by reading the [Reading RDF](../user_guide/Reading-RDF.md) documentation.