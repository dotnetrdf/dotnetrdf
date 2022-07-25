# Load RDF from a File 

RDF can be loading from local files using the `LoadFromFile()` extension method.  This method exists for both [IGraph](xref:VDS.RDF.IGraph) and [ITripleStore](xref:VDS.RDF.ITripleStore) e.g.

```csharp
IGraph g = new Graph();
g.LoadFromFile("example.ttl");
```

## Loading in a Specific Format 

dotNetRDF attempts to detect the data format of the file based upon the file extension and failing that using some simple format detection heuristics on the file contents.  You can force the loader to attempt to read RDF in a specific format by specifying the parser you want to use e.g.

```csharp
IGraph g = new Graph();
g.LoadFromFile("example.ttl", new TurtleParser());
```

Learn more by reading the [Reading RDF](/user_guide/reading-rdf.md) documentation.