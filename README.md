# Welcome

![Build status](https://github.com/dotnetrdf/dotnetrdf/actions/workflows/build.yaml/badge.svg)

---

dotNetRDF is a powerful and flexible API for working with RDF and SPARQL in .NET environments.

dotNetRDF is licensed under the MIT License, see the LICENSE.txt file in this repository

The `main` branch of this repository is now used for development of dotNetRDF 3.x. This is a major update that introduces a number of breaking API changes, new features including support for [RDF-Star and SPARQL-Star](https://w3c.github.io/rdf-star/cg-spec), and it also restructures the packaging of the code to minimize dependencies and better separate out core functionality from higher-level APIs.
For more details on what has changed, including the breaking changes that may require you to update your code, please read the section on [Upgrading to dotNetRDF 3.0](https://dotnetrdf.org/docs/latest/user_guide/upgrading_to_3_0.html) in the documentation.

If you are looking for the code for the dotNetRDF 2.x release series [please check out the `maintenance/2.x` branch](https://github.com/dotnetrdf/dotnetrdf/tree/maintenance/2.x)

The restructured NuGet packages for dotNetRDF 3.0 are:

- **dotNetRdf** - a meta-package that pulls in all of the packages listed below.
- **dotNetRdf.Core** - contains the core libraries. This includes support for reading and writing RDF; and for managing and querying RDF data in-memory.
- **dotNetRdf.AspNet** - provides a framework for hosting RDF data in an IIS web application. This includes implementations of the SPARQL Protocol and SPARQL Graph Store Protocol.
- **dotNetRdf.Client** - provides support for working with a range of triple stores. 
- **dotNetRdf.Data.DataTables** - a package which integrates RDF data with System.Data.DataTable
- **dotNetRdf.Dynamic** - provides an API for accessing and updating RDF graphs using .NET's dynamic objects.
- **dotNetRdf.HtmlSchema** - provides an RDF writer that generates HTML documentation for an ontology that uses the RDF Schema vocabulary.
- **dotNetRdf.Inferencing** - provides some basic inferencing support including RDF-Schema, SKOS and a small subset of OWL reasoning.
- **dotNetRdf.Ontology** - provides an API for manipulating an OWL ontology.
- **dotNetRdf.Query.FullText** - provides a full-text query plugin for dotNetRDF's Leviathan SPARQL query engine. The text indexing is provided by Lucene.
- **dotNetRdf.Query.Spin** - provides an implementation of [SPIN](http://spinrdf.org/) using dotNetRDF's Leviathan SPARQL query engine.
- **dotNetRdf.Shacl** - provides an API for validating a graph using [SHACL](https://www.w3.org/TR/shacl/).
- **dotNetRdf.Skos** - provides an API for working with a [SKOS](https://www.w3.org/TR/skos-reference/) taxonomy.

As of release 3.0 of dotNetRDF, we provide support for .NET Standard 2.0. This ensures compatibility of the libraries with .NET Framwork from 4.7 forwards, .NET Core from 2.0 forwards and .NET from 5.0 forwards.

The documentation and examples will be gradually updated and published on the "latest" branch of the documentation repository:

 - [User Guide](https://dotnetrdf.org/docs/latest/user_guide/index.html) - Series of articles detailing how to use various features of the library
 - [Developer Guide](https://dotnetrdf.org/docs/latest/developer_guide/index.html) - Some advanced documentation
 - [API Documentation](https://dotnetrdf.org/docs/latest/api/) - Class-by-Class API documentation


## Asking Questions and Reporting Bugs

If you have a question about using dotNetRDF, please post it on [StackOverflow using the tag `dotnetrdf`](https://stackoverflow.com/questions/tagged/dotnetrdf).

Bugs and feature requests can be submitted to our [issues list on GitHub](https://github.com/dotnetrdf/dotnetrdf/issues). When submitting a bug report, please
include as much detail as possible. Code and/or data that reproduces the problem you are reporting will make it much more likely that your issue gets addressed 
quickly.

## Developers

dotNetRDF is developed by the following people:

 - Rob Vesse
 - Ron Michael Zettlemoyer
 - Khalil Ahmed
 - Graham Moore
 - Tomasz Pluskiewicz
 - Samu Lang

dotNetRDF also benefits from many community contributors who contribute in the form of bug reports, patches, suggestions and other feedback, 
please see the [Acknowledgements](https://github.com/dotnetrdf/dotnetrdf/blob/master/Acknowledgments.txt) file for a full list.

## Pull Requests

We are always pleased to receive pull requests that fix bugs or add features. 
When fixing a bug, please make sure that it has been reported on the [issues list](https://github.com/dotnetrdf/dotnetrdf/issues) first.
If you plan to work on a new feature for dotNetRDF, it would be good to raise that on the issues list before you commit too much time to it.

