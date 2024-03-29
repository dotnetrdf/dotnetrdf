# Getting Started

The simplest way to get hold of dotNetRDF is via our NuGet packages.

We have split dotNetRDF into a number of separate packages to give developers a little more control over exactly which APIs and which additional third-party dependencies they need to include in their projects.

  * To do basic reading/writing and in-memory processing and querying with SPARQL, get the core `dotNetRdf.Core` package.
  * To work with dotNetRDF as a client interacting with one of the supported triple stores (other than Virtuoso), get the `dotNetRdf.Client` package.
  * To work with Virtuoso, get the `dotNetRdf.Data.Virtuoso` package.
  * For a single dependency that pulls in all of the dotNetRDF libraries, use the `dotNetRdf` meta-package.
  
> [!NOTE]
> You may have spotted that although this project is called dotNetRDF, for package names we follow the common .NET approach to camel-casing the TLA RDF. Hence our package names all use dotNetRdf.

We also provide a number of additional packages that extend the core APIs with additional functionality and support for other RDF-related standards.

  `dotNetRdf.Data.DataTables`
  : Supports integration between the dotNetRDF API and `System.Data.DataTables`
  
  `dotNetRdf.Dynamic`
  : Provides a dynamic objects-based API for working with RDF graphs.
  
  `dotNetRdf.Inferencing`
  : Provides an API and some support for simple reasoning over RDF graps.

  `dotNetRdf.LDF`
  : Provides an API for consuming and querying RDF data from a remote source that implements the [Linked Data Fragments](https://linkeddatafragments.org/) specification.
  
  `dotNetRdf.Ontology`
  : Provides a set of high-level APIs for working with RDF Schema and OWL ontologies.
  
  `dotNetRdf.Query.FullText`
  : Extends the in-memory SPARQL store with support for full-text queries using a Lucene.NET text index.
  
  `dotNetRdf.Query.SPIN`
  : An implentation of [SPARQL Inferencing Notation](https://spinrdf.org/) over the dotNetRdf SPARQL API.
  
  `dotNetRdf.Shacl`
  : Implements the [Shape Constraint Language](https://www.w3.org/TR/shacl/) standard.
  
  `dotNetRdf.Skos`
  : Provides a high-level API for working with the [Simple Knowledge Organization System](https://www.w3.org/2004/02/skos/) ontology.
  
  `dotNetRdf.Writing.HtmlSchema`
  : Provides a writer that generates HTML documentation from an OWL ontology or RDF Schema instance. (This writer is separated out from the core library as it introduces a dependency on System.Web) 