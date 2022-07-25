# Configuring SPARQL Operators 

SPARQL Operators are a SPARQL extension that allows you to extend how certain operators in SPARQL are evaluated.  You can learn more about operators in the [SPARQL Operators](/developer_guide/sparql/operators.md) page of the [Developer Guide](/developer_guide/index.md).

These may be configured quite simply provided they implement the [`ISparqlOperator`](xref:VDS.RDF.Query.Operators.ISparqlOperator) interface and have an unparameterized constructor.

```turtle

@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

[] a dnr:SparqlOperator ;
  dnr:type "VDS.RDF.Query.Operators.DateTime.DateTimeAddition" .
```