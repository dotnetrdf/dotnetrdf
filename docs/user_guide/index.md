# dotNetRDF User Guide

dotNetRDF is a comprehensive library for working with [RDF](https://www.w3.org/RDF/) and its related standards. This includes support for:

  * Reading and writing RDF in a variety of standard formats.
  * Working with RDF data in-memory on a machine.
  * Querying RDF data in-memory using the standard [SPARQL](https://www.w3.org/TR/sparql11-overview/) query language
  * Validating RDF using [SHACL](https://www.w3.org/TR/shacl/)
  * Working with a variety of commercial and open-source RDF triple stores as well as support for querying and updating any remote HTTP(S) endpoint that supports [SPARQL Protocol](https://www.w3.org/TR/sparql11-protocol/) and/or [SPARQL Graph Store Protocol](https://www.w3.org/TR/sparql11-http-rdf-update/)

This document is the **User Guide** which will take you through the features of the library and how to make use of them. If you want to understand how to extend dotNetRDF (e.g. to provide support for another third-party triple store, or to add support for reading/writing a particular format), then please consult the *Developers Guide*.

> [!NOTE]
> This guide assumes that you are at familiar with the core terminology of RDF and its related standards. So if terms like "graph", "blank node" and "triple store" mean nothing to you, you may find some of the descriptions in the documentation confusing. There are a number of good tutorial resources on RDF available on the Web which will help you get the grounding you need to make sense of these docs!

We also provide a full set of [API Documentation](~/api/VDS.RDF.yml) which you can use to get the nitty-gritty on every individual class and method in the dotNetRDF API, but we definitely recommend that you start here! 