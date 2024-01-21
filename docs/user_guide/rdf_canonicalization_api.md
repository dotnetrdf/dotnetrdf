# RDF Canonicalization API

## Introduction

The [RDF Dataset Canonicalization spec](https://www.w3.org/TR/rdf-canon/) defines a process for creating a canonical serialization of an RDF dataset (a collection of graphs).
Canonicalization is typically required when wishing to determine if one serialization of a graph (or collection of graphs) is isomorphic to another.
Scenarios such as the signing of RDF data independent of serialization or format rely on generating a canonical form of the data as input to cryptographic functions.

DotNetRDF includes an implementation of the specification which currently conforms to the [30 November 2023 draft of the specification](https://www.w3.org/TR/2023/CRD-rdf-canon-20231130/).
This implementation is provided by the class [`RdfCanonicalizer`](xref:VDS.RDF.RdfCanonicalizer).

## Creating an `RdfCanonicalizer`

The constructor for `RdfCanonicalizer` takes a single optional argument which is the moniker of the hashing algorithm to use when comparing nodes.
The default value for this argument is `SHA256` (which is the default algorithm defined by the specification).

## Canonicalization

The `RdfCanonicalizer` provides a single public [`Canonicalize`](xref:VDS.RDF.RdfCanonicalizer.Canonicalize(VDS.RDF.ITripleStore)) method which accepts the dataset to be canonicalized as an [`ITripleStore`](xref:VDS.RDF.ITripleStore) instance.
This method creates a new in-memory `ITripleStore` instance that contains the canonical form of the input dataset. 
The input dataset is not modified by this process.
The return value is a [`RdfCanonicalizer.CanonicalizedRdfDataset`](xref:VDS.RDF.RdfCanonicalizer.CanonicalizedRdfDataset) instance which provides the following properties:

* `InputDataset` - the [`ITripleStore`](xref:VDS.RDF.ITripleStore) instance provided as input to the canonicalizer.
* `OutputDataset` - the [`ITripleStore`](xref:VDS.RDF.ITripleStore) instance created by the canonicalizer containing the canonical form of the input dataset.
* `SerializedNQuads` - the serialization of `OutputDataset` in NQuads format (conforming to the W3C NQuads 1.1 specification).
* `IssuedIdentifiersMap` - a dictionary mapping the blank node identifiers in the input dataset to their canonical counterpartsin the output dataset.