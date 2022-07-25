# Inference and Reasoning 

Inference and Reasoning are mechanisms whereby an application can discover additional information that is not explicitly stated in the initial data.
There are various scenarios when you might want to use this and having this capability in the library provides a powerful feature to end users.
Inference and reasoning in dotNetRDF is currently based on the [`IInferenceEngine`](xref:VDS.RDF.Query.Inference.IInferenceEngine) interface which allows for both static and dynamic reasoners i.e. those that use a fixed set of rules and those that create their rules dynamically based on the input data.

# The IInferenceEngine interface 

The [`IInferenceEngine`](xref:VDS.RDF.Query.Inference.IInferenceEngine) interface has two main methods which implementers need to implement in order to integrate your own reasoners into dotNetRDF.
The first of these is the [`Initialise(IGraph g)`](xref:VDS.RDF.Query.Inference.IInferenceEngine.Initialise(VDS.RDF.IGraph)) method which is used to input graphs to the reasoner which define the schema/rules that the reasoner should follow.
The reasoner can process and interpret this Graph in any way it wishes in order to generate the rules that it will use when actually applying inference to a Graph.

The second method is the [`Apply()`](xref:VDS.RDF.Query.Inference.IInferenceEngine.Apply(VDS.RDF.IGraph)) method which applies inference to a Graph outputting the inferred triples into either the same graph or to another graph.
For implementers this method is where the core logic of the reasoner will be located (or at least called from).

## Existing Implementations 

As part of the library three types of reasoners are provided currently - an RDFS reasoner, a SKOS reasoner and a simple N3 Rules reasoner.

### RDFS Reasoner 

RDFS is an RDF vocabulary for expressing schemas for RDF data specified by the W3C.
It allows for the definition of class and property hierarchies and specifying things like the domains and ranges of properties.
We provide both a [`StaticRdfsReasoner`](xref:VDS.RDF.Query.Inference.StaticRdfsReasoner) which has to be initialised with one/more schemas and uses only those to make inferences and a [`RdfsReasoner`](xref:VDS.RDF.Query.Inference.RdfsReasoner) which is dynamic in that every Graph you apply reasoning to can extend the set of rules that the reasoner uses.

The RDFS reasoner does not apply the full range of possible RDFS based inferencing but does do the following:

* Asserts additional type triples for anything which has a type which is a sub-class of another type
* Asserts additional triples where the property (predicate) is a sub-property of another property
* Asserts additional type triples based on the domain and range of properties

Consider the following example schema (schema.ttl):

```
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix : <http://example.org/vehicles/> .

:Vehicle a rdfs:Class .
:Car rdfs:subClassOf :Vehicle .
:SportsCar rdfs:subClassOf :Car .
```

And the following example data (data.ttl):

```
@prefix ex: <http://example.org/vehicles/> .
@prefix : <http://myvehicledata.com/> .

:FordFiesta a ex:Car .
:AudiA8 a ex:Car .
:FerrariEnzo a ex:SportsCar .
```

If you were to add the data into a graph and ask it for things which are cars then it would only give you back `:FordFiesta` and `:AudiA8` despite the fact that `:FerrariEnzo` is also a car. If we apply an RDFS reasoner to the data using the schema given and then ask the same question we'd get back all three things e.g.

```csharp

using System;
using VDS.RDF;
using VDS.RDF.Query.Parsing;
using VDS.RDF.Query.Inference;

public class RdfsReasoningExample
{
	public static void Main(String[] args)
	{
		//First we want to load our data and schema into Graphs
		Graph data = new Graph();
		FileLoader.Load(data, "data.ttl");
		Graph schema = new Graph();
		FileLoader.Load(schema, "schema.ttl");

		//Now we ask for things which are cars from our data Graph
		IUriNode rdfType = data.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
		IUriNode car = data.CreateUriNode("ex:Car");
		foreach (Triple t in data.GetTriplesWithPredicateObject(rdfType, car))
		{
			Console.WriteLine(t.ToString());
		}
		//This will result in the triples defining the type for :FordFiesta
		//and :AudiA8 being printed
		//BUT without inference we don't know that :FerrariEnzo is a car

		//So now we'll go ahead and apply inference
		StaticRdfsReasoner reasoner = new StaticRdfsReasoner();
		reasoner.Initialise(schema);
		reasoner.Apply(data);

		//Now we ask for things which are cars again
		foreach (Triple t in data.GetTriplesWithPredicateObject(rdfType, car))
		{
			Console.WriteLine(t.ToString());
		}
		//This time it will have printed :FerrariEnzo as well as it will have inferred that anything
		//which is of type ex:SportsCar is also of type ex:Car
	}
}
```

### SKOS Reasoner 

SKOS is another RDF vocabulary specified by the W3C which is intended for use in defining taxonomies for classifying data.
The SKOS reasoner included in the library is a simple concept hierarchy reasoner which can infer additional triples where the subject has an object which is a `skos:Concept` in the taxonomy by following `skos:narrower` and `skos:broader` links as appropriate.

As with RDFS there is a [`StaticSkosReasoner`](xref:VDS.RDF.Query.Inference.StaticSkosReasoner) and a dynamic variant called [`SkosReasoner`](xref:VDS.RDF.Query.Inference.SkosReasoner).

Consider the following classification of vehicles based on the earlier examples (taxonomy.ttl):

```
@prefix skos: <http://www.w3.org/2004/02/skos/core#>
@prefix : <http://example.org/vehicles/> .

:Vehicle a skos:Concept .
:Vehicle skos:narrower :Car .
:Car skos:narrower :SportsCar .
```

And the following data (data2.ttl):

```
@prefix ex: <http://example.org/vehicles/> .
@prefix : <http://myvehicledata.com/> .

:FordFiesta ex:vehicleType ex:Car .
:AudiA8 ex:vehicleType ex:Car .
:FerrariEnzo ex:vehicleType ex:SportsCar .
```

As seen in the RDFS example without inference we don't automatically know that anything which stated it was related to the concept `:SportsCar` is also related to the concept :Car but by applying SKOS concept hierarchy reasoning we can do this.

### Simple N3 Rules Reasoner 

The [`SimpleN3RulesReasoner`](xref:VDS.RDF.Query.Inference.SimpleN3RulesReasoner) is a reasoner that is able to apply simple N3 Rules.
The reasoner must be initialised with a Graph that has been parsed from an input N3 file in order to contain any rules.
A simple rule is expressed as follows:

```
{ ?x a ex:Car } => { ?x a ex:Vehicle }
```

The above rule would match anything that is a ex:Car and assert that it is also a ex:Vehicle.

Any rule which is composed of two graph literals and connected via => or <= can be processed and applied. Note that nested graph literals are not currently supported.

Additionally if you use `@forsome` or `@forall` to define tokens as being variables these will be processed e.g.

```
@forall :x .
{ :x a ex:Car } => { :x a ex:Vehicle }
```

The above is equivalent to the previous example since we've used a `@forall` directive to specify that :x is a variable.
Generally speaking specifying variables directly is always preferred.

> [!NOTE]
> The current N3 reasoner implementation only supports the implication operation (`http://www.w3.org/2000/10/swap/log#implies`).
> Other N3 logic operations, string functions and math functions are not currently implemented by the reasoner.

# Using Inference with Triple Stores 

The library also provides an [`IInferencingTripleStore`](xref:VDS.RDF.IInferencingTripleStore) interface which extends the basic [`ITripleStore`](xref:VDS.RDF.ITripleStore) interface with methods which allow for the attachment of reasoners (instances of [`IInferenceEngine`](xref:VDS.RDF.Query.Inference.IInferenceEngine) implementations) to a Triple Store.
Reasoning when used in this sense is static in that inference is applied only at certain points:

* When you add a reasoner the `IInferencingTripleStore` the implementations in the library will apply the reasoner to all existing Graphs in the Store
* When you add a new Graph to the Store the reasoner will be applied to that Graph

Current implementations store the inferred information in a special Graph inside the Triple Store so that the existing Graphs are not themselves altered - there is no guarantee/requirement that 3rd party implementations of this interface will do this.
Also there is a limitation in that inferences will not be made when a graph changes or is removed so you can have some data over which no inferences have been made or some data which is inferred from data you've removed from your Store.

# Native Stores and Inferencing 

Currently there is no formal API support in the library for using inference with 3rd party stores, primarily because this feature is not available in many stores and there is not currently a standardised mechanism for specifying that you wish to use inference with a store.

However some stores such as [StarDog](http://stardog.com) do support reasoning when making queries, see [Triple Store Integration](triple_store_integration.md) for more information.

# Reasoners and SPARQL Endpoints 

Please see [Configuration API - Reasoners](configuration/reasoners.md) for information on how to specify reasoner configuration and how to attach them to HTTP Handlers.