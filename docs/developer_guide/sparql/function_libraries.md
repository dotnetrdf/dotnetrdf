# Function Libraries

Function Libraries are collections of extension functions that may be used in SPARQL queries and updates.
These are supported by our in-memory [Leviathan Engine](leviathan_engine.md) and some may be supported by other triple stores as well.

If you are looking to add your own extension functions please see the [Extension Functions](extension_functions.md) documentation for details on how to do that.

| Function Library | Description |
|------------------|-------------|
| [ARQ](http://jena.apache.org/documentation/query/library-function.html) | The ARQ function library contains several useful extension functions |
| [XPath](xpath_functions.md) | The XPath Function library has a wide variety of functions, many of these now have SPARQL equivalents as of SPARQL 1.1 |
| [Leviathan](leviathan_functions.md) | Our own function library with a number of useful numeric and trigonometric functions |