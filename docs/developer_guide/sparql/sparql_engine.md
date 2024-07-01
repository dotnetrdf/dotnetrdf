# SPARQL Engine

This page acts as a hub for topics about our SPARQL Engine, if you are interested in learning how to make queries and updates please see the [Querying with SPARQL](../../user_guide/querying_with_sparql.md) and [Updating with SPARQL](../../user_guide/updating_with_sparql.md) pages.

## Leviathan

Leviathan is the code name for our block based in-memory SPARQL evaluation engine, it has the following capabilities:

* Execution
  * Full SPARQL 1.0 and SPARQL 1.1 queries and updates are supported. See [SPARQL Conformance](conformance.md) for current conformance status.
  * Support for adding [Extension Functions](extension_functions.md) per the [SPARQL specification](http://www.w3.org/TR/sparql11-query/#extensionFunctions)
* Optimisation
  * Powerful query optimizations described on the [SPARQL Optimization](optimization.md) page
* Extensions
  * Full Text Query, see [Full Text Querying with SPARQL](../../user_guide/full_text_querying_with_sparql.md)
  * `LET` assignments permitted with semantics equivalent to [ARQ](http://jena.apache.org/documentation/query/index.html)
  * Additional `NMAX` and `NMIN` aggregates (Numeric Maximum and Minimum)
  * Additional `MEDIAN` and `MODE` aggregates
  * Additional [Function libraries](function_libraries.md)
    * Support for some of the [XPath function library](xpath_functions.md)
    * Support for the [ARQ function library](http://jena.apache.org/documentation/query/library-function.html)
    * Support for our own [Leviathan function library](leviathan_functions.md)

You can learn more about this engine on the [Leviathan Engine](leviathan_engine.md) page.

## Further Reading

* [SPARQL Optimization](optimization.md)
  * [Implementing Custom Optimizers](implementing_custom_optimizers.md)
* [SPARQL Extensions](extensions.md)
* [SPARQL Performance](performance.md)
* [SPARQL Conformance](conformance.md)