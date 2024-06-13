# SPARQL Extensions

This page provides an overview and links to further resources about SPARQL extensions supported in dotNetRDF.

## Full Text Query

Full Text Query is an extension which allows full text search of RDF data to be integrated into SPARQL queries, please see the [Full Text Querying with SPARQL](../../user_guide/full_text_querying_with_sparql.md) page for more information on this.

## LET Assignment

`LET` is a extension that predates the SPARQL 1.1 `BIND` clause, it performs assignment like `BIND` but has slightly different semantics in that if you use `LET` to assign to an existing variable it acts like a `SAMETERM()` based `FILTER` rather than being a syntax error.

Generally it is preferred that you use `BIND` wherever possible since this is standard SPARQL and will be portable across different SPARQL implementations.

> [!NOTE]
> We intend to remove `LET` at some point in the future now it is superseded by standard SPARQL 1.1

## Function Libraries

We support a wide variety of extension functions and aggregates, see [Function Libraries](function_libraries.md) for available libraries.

## Custom Extension Functions

You can also create your own extension functions. See [Extension Functions](extension_functions.md) for more details.