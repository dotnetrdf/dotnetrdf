# Load OWL 

While dotNetRDF does not support OWL in terms of axioms, OWL ontologies and reasoning it can still be used to load the RDF triples representing the OWL ontology.

Load this the same way you load any other file:

```csharp
//Create a graph to load into
IGraph g = new Graph();

//Load the OWL file
g.LoadFromFile("example.owl");
```

The library assumes that any file with a `.owl` extension is encoded as RDF/XML, if this is not the case you will need to use the overload method and supply a specific parser e.g.

```csharp
//Create a graph to load into
IGraph g = new Graph();

//Load the OWL file encoded in a non-standard syntax
g.LoadFromFile("example.owl", new TurtleParser());
```