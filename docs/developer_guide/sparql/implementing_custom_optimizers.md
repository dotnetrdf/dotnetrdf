# Implementing Custom Optimizers

As discussed on the [SPARQL Optimization](optimization.md) and [Advanced SPARQL Operations](../../user_guide/advanced_sparql_operations.md) pages our [Leviathan SPARQL Engine](leviathan_engine.md) supports two kinds of optimisers which can be customised if desired.

# Implementing Query Optimizers

Query Optimizers implement the <xref:VDS.RDF.Query.Optimisation.IQueryOptimiser?displayProperty=fullName> interface which has a single `Optimise()`method which takes in a Graph Pattern to optimise and an enumeration of variables that have occurred prior to that Graph Pattern.

A Query Optimiser should do three things:

* Reorder Triple Patterns
* Place FILTERs appropriately
* Place assignments (i.e. `BIND`) appropriately

Typically, implementations will only want to alter how triple patterns are reordered and this can be done fairly easily be deriving your implementation from <xref:VDS.RDF.Query.Optimisation.BaseQueryOptimiser>. This class provides the outline for a Query Optimiser and you implement/override various methods and properties in order to customise it.

For example if you wanted to change how the triple patterns are ordered you'd derive from this class and them implement the `GetRankingComparer()` method. This method returns a `IComparer<ITriplePattern>` which is used to order the set of <xref:VDS.RDF.Query.Patterns.ITriplePattern> instances found in the Graph Pattern.

> [!IMPORTANT]
> Note that <xref:VDS.RDF.Query.Optimisation.BaseQueryOptimiser> implements a two-pass reordering by default. The second pass may change the initial ordering provided by your comparer, to disable the second pass you can override the `ShouldReorder` property to return false.

## Useful Methods

The following methods of <xref:VDS.RDF.Query.Patterns.GraphPattern> are designed for use primarily by query optimisers:

* `SwapTriplePatterns()` swaps the position of two Triple Patterns in a Graph Pattern
* `InsertFilter()` inserts a Filter at a specific position in a Graph Pattern
* `InsertAssignment()` inserts an IAssignmentPattern at a specific position in a Graph Pattern

# Implementing Algebra Optimizers

Algebra Optimisers implement the <xref:VDS.RDF.Query.Optimisation.IAlgebraOptimiser> interface which has an `Optimise()` method and an `IsApplicable()` method. The latter is used to determine whether an algebra optimisation can be applied to a query while the former actually applies the Optimiser.

Algebra Optimisers are typically used to transform the algebra to use special operators which help evaluate certain forms of query faster e.g. the built-in <xref:VDS.RDF.Query.Optimisation.AskBgpOptimiser> which transforms the algebra of `ASK` queries, so they evaluate much faster.  The [Advanced SPARQL Operations](/user_guide/advanced_sparql_operations.md) page details some of the default optimizers used.