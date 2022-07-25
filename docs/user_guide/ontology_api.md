# Ontology API 

The Ontology API part of dotNetRDF provides a number of abstractions over graphs, triples and nodes that allow users who don't want to work at the levels of triples to create and manipulate ontologies and schemas.
In general anything that uses RDFS and OWL classes and properties and expresses relationships between them may be more easily manipulated using the Ontology API than directly using the core graph and triple APIs.

## Basic Example 

The following provides a basic example of the differences between the Ontology API and the Graph and Triple APIs.
In this first code sample we use the Ontology API to enumerate the super and sub classes of a given class:

```csharp

using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;

public class OntologyGraphExample()
{
	public static void Main(String[] args)
	{
		//First create an OntologyGraph and load in some data
		//Here we use an imaginary example file Ontology.rdf - substitute in an appropriate filename
		OntologyGraph g = new OntologyGraph();
		FileLoader.Load(g, "Ontology.rdf");

		//Get the class of interest
		//Again we use an imaginary class URI, substitute in an appropriate URI
		OntologyClass someClass = g.CreateOntologyClass(new Uri("http://example.org/someClass"));

		//Write out Super Classes
		foreach (OntologyClass c in someClass.SuperClasses)
		{
			Console.WriteLine("Super Class: " + c.Resource.ToString());
		}
		//Write out Sub Classes
		foreach (OntologyClass c in someClass.SubClasses)
		{
			Console.WriteLine("Sub Class: " + c.Resource.ToString());
		}
	}
}
```

Now we'll show code to achieve the same thing using the Graph and Triple APIs:

```csharp

using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

public class GraphExample()
{
	public static void Main(String[] args)
	{
		//First get a Graph and load in our data
		//Here we use an imaginary example file Ontology.rdf - substitute in an appropriate filename
		IGraph g = new Graph();
		FileLoader.Load(g, "Ontology.rdf");

		//Get the Node representing the class of Interest
		//Again we use an imaginary class URI, substitute in an appropriate URI
		INode someClass = g.GetUriNode(new Uri("http://example.org/someClass"));

		//Note - GetUriNode returns null if no such URI exists, make sure to check for this or use
		//CreateUriNode instead
		if (someClass == null) return;

		//Write out the Super Classes
		INode subClassOf = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"));
		foreach (Triple t in g.GetTriplesWithSubjectPredicate(someClass, subClassOf))
		{
			Console.WriteLine("Super Class: " + t.Object.ToString());
		}
		//Write out the Sub Classes
		foreach (Triple t in g.GetTriplesWithPredicateObject(subClassOf, someClass))
		{
			Console.WriteLine("Sub Class: " + t.Subject.ToString());
		}
	}
}
```

While the 2nd example may not seem like much more code it requires the end user to understand how the RDF is structured at the triple level in order to retrieve the data.

An additional advantage of the Ontology API approach is that it will automatically traverse the class hierarchy for you so using the 1st method you get all super and sub classes regardless of distance from the original class while using the 2nd method you only get direct super and sub classes.
Getting all super and sub classes in the class hierarchy using the 2nd method would require significantly more code or the application of an RDFS reasoner.

# Concepts 

The API provides the following concepts which are wrappers over graphs, triples and nodes:

| Class | Purpose |
| --- | --- |
| [OntologyGraph](xref:VDS.RDF.Ontology.OntologyGraph) | Represents a graph whose ontology elements may be accessed |
| [Ontology](xref:VDS.RDF.Ontology.Ontology) | Represents information about an ontology |
| [OntologyResource](xref:VDS.RDF.Ontology.OntologyResource) | Represents a resource in the ontology, specific sub-classes expand the basic capabilities further |
| [OntologyClass](xref:VDS.RDF.Ontology.OntologyClass) | Represents a class in an ontology |
| [OntologyProperty](xref:VDS.RDF.Ontology.OntologyProperty) | Represents a property in an ontology |
| [Individual](xref:VDS.RDF.Ontology.Individual) | Represents an instance of a class in an ontology |