# dotNetRDF 2.x => 3.0 changes

## Core model changes
* Literal nodes now follow the RDF 1.1 data model and always have a Datatype value (xsd:string for string literals with no explicitly defined datatype, rdf:langString for langauge-tagged string literals)
* Added a new `Namespace` class for creating full URIs from a base URI and suffix. There are static instances for common RDF namespaces (RDF, RDFS, XSD)
* Removed C# binary and XML serialization support (currently just from LiteralNode...will be extended to the rest of the Node / graph classes)