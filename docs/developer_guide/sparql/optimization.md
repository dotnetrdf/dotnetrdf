# SPARQL Optimization

This article aims to explain as much of how our SPARQL query optimisation works in the [Leviathan Engine](leviathan_engine.md) as possible. Optimisation can be divided into 3 main stages:

1. Query Optimisation
2. Algebra Optimisation
3. Evaluation Optimisation

If you want to implement your own optimizers see [Implementing Custom Optimisers](implementing_custom_optimizers.md)

# Query Optimisation

Query Optimisation is the first stage of optimisation and occurs automatically at the end of query parsing.
The aim of Query Optimisation is to reorder the Triple Patterns within each Graph Pattern to do several things:

* Evaluate the most selective Triple Patterns first - see Pattern Selectivity
* Evaluate `FILTER` at the earliest possible point - see Filter Placement
* Evaluate `BIND` at the earliest possible point - see Assignment Placement
* Minimise the complexity of joins within Graph Patterns and across Graph Patterns - see Join Complexity Minimisation

The optimiser addresses the first three aims in its first pass through and then does the last optimisation in a second pass.

## Pattern Selectivity

To minimise the size of intermediate results and evaluate queries as quickly as possible it is better if Triple Patterns with higher selectivity are evaluated first wherever possible.
The selectivity of a pattern pertains to how restrictive it is and how likely we consider it to return smaller numbers of results.

Our default selectivity ranking is purely rules based and ranks patterns based on the following order (from highest to lowest selectivity):

1. Subject-Predicate-Object
2. Subject-Predicate
3. Subject-Object
4. Predicate-Object
5. Subject
6. Predicate
7. Object

**Note:** The above represents the ranking implemented in the current release, this has changed in the past and may change in the future based on internal testing and benchmarking.
Sometimes you may be able to get better performance by using the statistics based <xref:VDS.RDF.Query.Optimisation.WeightedOptimiser>.

The name of a pattern refers to the parts of the pattern that are not variables, i.e. an example Subject-Predicate pattern would be the following:

```
<http://example.org/subject> <http://example.org/predicate> ?obj .
```

For example given the following input query:

```sparql
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}
```

The optimiser would reorder it to the following:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s ?p ?o .
}
```

Hopefully it should be easy fairly easy to see that the 2nd form is more optimal to evaluate since the more selective pattern will be evaluated first.

Where two patterns are equally selective the optimiser will order by the names of the variables. The effect of this is to cause variables to be grouped with other patterns that have the same variables. This ordering is not perfect and so requires some adjustment in the second pass optimisation that occurs after the Filter and Assignment placement. The second pass optimisation is discussed in the Join Complexity Minimisation section.

## Filter Placement

Filter Placement refers to the notion expressed in the SPARQL specification here that a `FILTER` has the same meaning wherever it occurs within a group graph pattern. Since the evaluation model for Leviathan engine is block based (i.e. each operator is evaluated in full) it is desirable to evaluate a `FILTER` as soon as is possible in order to minimize the intermediate results that need to be considered. In essence this means we evaluate if at the first point after which all mentioned variables have occurred.

For example given the following query:

```sparql
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

The optimiser will optimise it to the following:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE
{
  ?s rdfs:label ?label .
  FILTER(LANGMATCHES(LANG(?label), "en"))
  ?s a ?type .
}
```

## Assignment Placement

Assignment placement is exactly the same as Filter Placement. It just places `BIND` clauses at the first point after which all mentioned variables occur. For example given:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  BIND(LANG(?label) AS ?lang)
}
```

The optimiser would produce:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  BIND(LANG(?label) AS ?lang)
  ?s a ?type .
}
```

## Join Complexity Minimisation

Join Complexity Minimisation is a 2nd pass optimisation which attempts to further reorder the triple patterns to minimise complex joins. In particular it looks to avoid cross products (joins when the two sides are disjoint) since these are particularly expensive to compute.

As an example consider the following query:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s a ?type .
  ?s <http://example.org/predicate> ?value .
  ?value rdfs:label ?label .
}
```

The first pass only optimisation of this would produce the following:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?value rdfs:label ?label .
  ?s a ?type .
  ?s <http://example.org/predicate> ?value .
}
```

This is a poor optimisation as it requires computation of a cross product between the results of the first two patterns which makes the evaluation engine work much harder than necessary.

So the second pass optimisation is designed to look specifically for these cases and reorder the patterns further to eliminate or reduce the complexity of the products. The example query actually optimises to the following:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?value rdfs:label ?label .
  ?s <http://example.org/predicate> ?value .
  ?s a ?type .
}
```

In this version there are no cross products because at each join there is at least one variable that has been previously mentioned so the results of the two patterns are never disjoint.

# Algebra Optimisation
 
The 2nd stage of optimisation occurs each time you evaluate a query. In order to evaluate a query it must be first transformed into its SPARQL Algebra form. Once the Query has been transformed the algebra can then be modified to use specialised algebra operators which are optimised for certain forms of query.

For example consider the following query from earlier:

```sparql

PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s ?p ?o .
}
```

This query translates into the following algebra:

```
Select(Bgp({ ?s rdfs:label ?label . ?s ?p ?o }))
```

In a case like this there is absolutely nothing we can do other than evaluate the algebra as-is. But if we were to modify the query so it instead has a `LIMIT` clause then we could optimise the algebra like so:

```
Slice(Select(LazyBgp({ ?s rdfs:label ?label . ?s ?p ?o })), LIMIT 10, OFFSET 0)
```

<xref:VDS.RDF.Query.Algebra.LazyBgp> is a specialised implementation of <xref:VDS.RDF.Query.Algebra.IBgp> which does the minimum amount of work necessary to answer a query. In this case it needs only find the first 10 solutions that satisfy the pattern. Since a query may operate over millions of triples if we can stop processing earlier then we can get better performance. If we process the same query without this optimisation we have to find every possible solutions that satisfies the pattern and then discard all but 10.

The algebra optimiser uses a variety of these kinds of optimisations depending on the query. Obviously they are not applicable in every case (e.g. if there is an `ORDER BY` clause you need all the solutions in order to do the ordering before you can apply the `LIMIT`) but they can be used to significantly improve performance in various cases.

There are a similar set of optimisations that are used for `ASK` queries since they aim to do the minimal amount of work possible as they only need to find one result satisfying the query. For a fuller list of available optimisers see the [Advanced SPARQL Operations](/user_guide/advanced_sparql_operations.md) page.

# Evaluation Optimisation

The final stage of optimisation occurs during the actual evaluation of the queries. A number of techniques are used to try and reduce the amount of work the engine has to do as far as possible.

## Dynamic Restriction of Triple Patterns

When evaluating triple patterns the engine will in many cases already have values bound in the intermediate results for variables occurring in the next triple pattern to be evaluated. In this case the engine will substitute each possible combination into the next triple pattern and evaluate it in turn in order to build up the overall set of results. This typically results in much less work for the query engine since the results returned are much more likely to be be preserved by the subsequent join operation than if the pattern was evaluated without the variable substitution. Since the joins are typically the most costly part of the query evaluation this produces significant performance boosts.

## Lazy Evaluation

The engine is optimised to use lazy evaluation wherever possible. What this means in practise is that if the engine can skip/abort an evaluation step (usually when there are no results found or sufficient results are found) it will always do so.

## Parallel Evaluation

The more recent releases leverage the .Net PLINQ framework to parallelize parts of the query evaluation wherever possible.  This means queries can take full advantage of the processors available on your machine, see [Performance](performance.md) for performance data which shows just how much of an impact this can have.

Currently, the following operations are parallelized:

* Join
* Left Join (`OPTIONAL`)
* Product
* Filter
* Bind