# SPARQL Conformance

This page details dotNetRDF conformance with the SPARQL specifications.

| Specification | Conformance |
|---------------|-------------|
| SPARQL 1.0 Query | 2 conformance tests fail due to an issue with the .Net `Uri` class and UTF normalization |
| SPARQL 1.1 Query| 100% of conformance tests passed |
| SPARQL 1.1 Update | 100% of conformance tests passed |
| SPARQL 1.1 CSV and TSV Results | 100% of conformance tests passed |
| SPARQL 1.1 JSON Results | 100% of conformance tests passed |
| SPARQL 1.1 Service Description | 100% of conformance tests passed |
| SPARQL 1.1 Protocol | 0.8.2 fails 4 conformance tests, 0.9.0 onwards passes 100% of conformance tests |

Officially reported conformance status for dotNetRDF against SPARQL 1.1 can be seen in the [SPARQL 1.1 Implementations Report](http://www.w3.org/2009/sparql/implementations/)