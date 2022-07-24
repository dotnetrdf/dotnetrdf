# Configuring Reasoners 

Reasoners are classes that can perform reasoning on Graphs/Triple Stores to infer additional triples. These classes must implement the [`IInferenceEngine`](xref:VDS.RDF.Query.Inference.IInferenceEngine) interface.

Reasoners are attached to Graphs/Triple Stores using the `dnr:reasoner` property as described in [Configuration API - Graphs](graphs.md) and [Configuration API - Triple Stores](triple_stores.md).

# Basic Configuration 

Basic Configuration for a reasoner looks like the following:

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

_:reasoner a dnr:Reasoner ;
  dnr:type "VDS.RDF.Query.Inference.StaticRdfsReasoner" ;
  dnr:usingGraph _:schema .

_:schema a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromFile "schema.rdf" .
```

In the above example we configure a static RDFS reasoner which is initialised with the given graph which is itself loaded from the file schema.rdf

Any of the basic reasoner implementations provided in the library can be configured in this way. Any number of `dnr:usingGraph` properties can be used to initialise the reasoner with multiple input graphs if desired.