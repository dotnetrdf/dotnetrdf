# Leviathan Engine

Leviathan is the code name for our block based in-memory SPARQL engine, it supports full SPARQL 1.0 and SPARQL 1.1.  Leviathan is a designed to follow the SPARQL algebra closely and so executes complex queries correctly and accurately.

## Development History

* The 0.4.x releases improved the engine significantly with many additional SPARQL 1.1 features and enhancements/fixes to our join logic.
* The 0.5.0 release made some internal changes to make the engine more extensible and provided a 25-30% performance boost for most queries.
* The 0.6.0 release added various improved optimisations and made a 10x performance improvement to our join algorithms and added new SPARQL 1.1 features to the engine.
* The 0.7.0 release added new optimisations and also enabled parallel evaluation enhancements to various aspects of the engine which can yield significant performance improvements depending on the query and dataset.
* The 0.8.0 release further improved parallel evaluation particularly for `OPTIONAL`, improved how `GROUP BY` is calculated and removed unnecessary logic from the engine to boost performance.

## SPARQL Optimization

SPARQL Optimization is discussed on the [SPARQL Optimization](optimization.md) page.

## SPARQL Algebra

Leviathan follows the SPARQL Algebra relatively closely with a couple of minor differences. We model the solution modifiers differently and order them slightly differently to the SPARQL specification. 

All algebra representations are based on the `VDS.RDF.Query.Algebra.ISparqlAlgebra` interface which provides a single method Evaluate(). You can obtain the algebra representation of a `VDS.RDF.Query.SparqlQuery` object by using it's `ToAlgebra()` method. `VDS.RDF.Patterns.GraphPattern` objects also have a `ToAlgebra()` method that you can use to get the algebra just for that pattern.

Given an `ISparqlAlgebra` instance you can use either the `ToGraphPattern()` or the `ToQuery()` methods to attempt to convert it back into a `GraphPattern` or `SparqlQuery` but note that not all algebras can be successfully converted back into these.

Developers can if they wish use the classes from the `VDS.RDF.Query.Algebra` namespace to manually compose queries.

If you do compose an algebra expression you can evaluate it by creating an instance of `VDS.RDF.Query.LeviathanQueryProcessor` and then invoking the `ProcessAlgebra()` method on the algebra expression you created e.g.

```csharp
using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

public class AlgebraEvaluationExample
{
	public static void Main(String[] args)
	{
		TripleStore store = new TripleStore();

		//Assume we fill the Triple Store with data from somewhere...

		//Build up an Algebra Expression
		TriplePattern tp = new TriplePattern(new VariablePattern("?s"), new VariablePattern("?p"), new VariablePattern("?o"));
		BGP selectAll = new BGP(tp);
		UnaryExpressionFilter filter = new UnaryExpressionFilter(new IsLiteralFunction(new VariableExpressionTerm("o")));
		Filter requireObjectLiteral = new Filter(selectAll, filter);

		//Evaluate the Query to get the resulting Multiset
		LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
		BaseMultiset result = processor.ProcessAlgebra(requireObjectLiteral, null);

		//Display the Results
		foreach (Set s in result.Sets)
		{
			Console.WriteLine(s.ToString());
		}
	}
}
```

## Further Reading

* [SPARQL Extensions](extensions.md)
* [Leviathan Function Library](leviathan_functions.md)