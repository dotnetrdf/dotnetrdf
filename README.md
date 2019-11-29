# Welcome

[![Build status](https://ci.appveyor.com/api/projects/status/f8wtq0qh4k6620sl/branch/master?svg=true)](https://ci.appveyor.com/project/dotNetRDFadmin/dotnetrdf/branch/master)


dotNetRDF is a powerful and flexible API for working with RDF and SPARQL in .NET environments.

dotNetRDF is licensed under the MIT License, see the LICENSE.txt file in this repository

## Getting Started

The easiest way to get dotNetRDF is via NuGet. We provide the following packages:

- **dotNetRDF** - contains the core libraries. This includes support for reading and writing RDF; and for managing and querying RDF data in-memory.
- **dotNetRDF.Data.DataTables** - a package which integrates RDF data with System.Data.DataTable
- **dotNetRDF.Data.Virtuoso** - provides support for using OpenLink Virtuoso as a backend store with dotNetRDF.
- **dotNetRDF.Query.FullText** - provides a full-text query plugin for dotNetRDF's Leviathan SPARQL query engine. The text indexing is provided by Lucene.
- **dotNetRDF.Query.Spin** - provides an implementation of [SPIN](http://spinrdf.org/) using dotNetRDF's Leviathan SPARQL query engine.
- **dotNetRDF.Web** - provides a framework for hosting RDF data in an IIS web application. This includes implementations of the SPARQL Protocol and SPARQL Graph Store Protocol.

We currently provide support for the following .NET frameworks:

- .NET 4.0
- .NET 4.0 Client Profile
- .NET Standard 2.0
	
## Read The Docs!

To get started with using dotNetRDF you may want to check out the following resources:

 - [User Guide](https://github.com/dotnetrdf/dotnetrdf/wiki/UserGuide) - Series of articles detailing how to use various features of the library
 - [Developer Guide](https://github.com/dotnetrdf/dotnetrdf/wiki/DeveloperGuide) - Some advanced documentation

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

