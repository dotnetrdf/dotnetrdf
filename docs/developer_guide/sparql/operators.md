# SPARQL Operators

SPARQL Operators are an extension to SPARQL permitted by the specification which allow for implementations to extend the definition of certain operators - namely `=`, `!=`, `<`, `<=`, `>`, `>=`, `+`, `-`, `*` and `/`.

Currently dotNetRDF only supports extending the arithmetic operators i.e. `+`, `-`, `*` and `/`.  In the future we will likely extend this functionality to allow any operator to be overloaded.

## Extended Operators

Right now the only operators which dotNetRDF provides extensions for are `+` and `-` when used with `xsd:dateTime`, `xsd:duration`, `xsd:dayTimeDuration`.  This means that when using our SPARQL engine you can add/subtract durations from date times and add/subtract durations from each other.

## Implementing Custom Operators

Custom operators are implemented by implementing the <xref:VDS.RDF.Query.Operators.ISparqlOperator> interface and registering your implementation with the <xref:VDS.RDF.Query.Operators.SparqlOperators> registry.

### The ISparqlOperator Interface

The `ISparqlOperator` interface is relatively simple containing a single property and two methods.  Once this is implemented you register your implementation like so:

```csharp
SparqlOperators.AddOperator(new MyCustomOperator());
```

#### Operator

The `Operator` property returns a <xref:VDS.RDF.Query.Operators.SparqlOperatorType> which indicates which operator your implementation overloads.

#### IsApplicable()

The `IsApplicable()` method takes in the proposed arguments for the operator and determines whether the operator can be applied to those arguments e.g. are the argument types compatible for the implementation.

#### Apply()

The `Apply()` method takes the arguments for the operator and applies the operator returning either the resulting value or throwing a <xref:VDS.RDF.Query.RdfQueryException> if an error occurs.