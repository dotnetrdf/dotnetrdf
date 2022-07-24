# Building SPARQL 

SPARQL is the standard query language for the Semantic Web and can be used to query large volumes of RDF data. [SPARQL Query Language Specification](https://www.w3.org/TR/2013/REC-sparql11-query-20130321) defines the syntax and semantics of the language and explains SPARQL query buildling with numerous examples. SPARQL standard is drafted and maintained by [W3C](https://www.w3.org/Consortium/).

dotNetRDF facilitates SPARQL construction with a set of APIs under [`VDS.RDF.Query.Builder` namespace](xref:VDS.RDF.Query.Builder). We'll look at some of the common queries patterns you can construct using dotNetRDF.

## Hello World
Constructing a simple "Hello World" SPARQL. This query fetches all users whose last name is "John Smith"

```SPARQL
 SELECT ?x
 WHERE { ?x <http://www.w3.org/2001/vcard-rdf/3.0#FN>  "John Smith" }
```

```C#
using System;
using VDS.RDF.Query.Builder;

static void HelloWorld()
{
	string x = "x";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { x })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(x)
					.PredicateUri(new Uri("http://www.w3.org/2001/vcard-rdf/3.0#FN"))
					.Object("John Smith");
			});

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `PREFIX` with multiple triples
The query fetches given names for the family name "Smith".

```SPARQL
PREFIX vcard:      <http://www.w3.org/2001/vcard-rdf/3.0#>

SELECT ?givenName
WHERE
{ 
	?y vcard:Family "Smith" .
	?y vcard:Given ?givenName .
}
```

```C#
using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

static void WithPrefix()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));

	string y = "y";
	var givenName = new SparqlVariable("givenName");
	var queryBuilder =
		QueryBuilder
		.Select(new SparqlVariable[] { givenName })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(y)
					.PredicateUri("vcard:Family")
					.Object("Smith");
				triplePatternBuilder
					.Subject(y)
					.PredicateUri("vcard:Given")
					.Object(givenName);
			});
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## Numeric `FILTER`
This query filters resources above 24 years of age.

```SPARQL
PREFIX info: < http://somewhere/peopleInfo#>

SELECT ?resource
WHERE
{
	?resource info:age ?age .
	FILTER(?age >= 24)
}
```

```C#
using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

static void WithNumericFilter()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("info", new Uri("http://somewhere/peopleInfo#"));

	string resource = "resource";
	string age = "age";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { resource })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(resource)
					.PredicateUri($"info:{age}")
					.Object(age);
			})
		.Filter((builder) => builder.Variable(age) > 24);
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## Regex `FILTER`
This query filters given names based on a case-insensitive regex comparison.

```SPARQL
PREFIX vcard: <http://www.w3.org/2001/vcard-rdf/3.0#>

SELECT ?g
WHERE
{
	?y vcard:Given ?g .
	FILTER regex(?g, "sarah", "i")
}
```

```C#
using System;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

static void WithRegexFilter()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));

	var givenName = new SparqlVariable("givenName");
	var queryBuilder =
		QueryBuilder
		.Select(new SparqlVariable[] { givenName })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject("y")
					.PredicateUri("vcard:Given")
					.Object(givenName);
			})
		.Filter((builder) => builder.Regex(builder.Variable("givenName"), "sarah", "i"));
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `OPTIONAL` clause with `FILTER`
This query fetches name and optional age above 42.

```SPARQL
PREFIX info:        <http://somewhere/peopleInfo#>
PREFIX vcard:      <http://www.w3.org/2001/vcard-rdf/3.0#>

SELECT ?name ?age
WHERE
{
	?person vcard:FN  ?name .
	OPTIONAL { ?person info:age ?age . FILTER ( ?age > 42 ) }
}
```

```C#
static void WithOptionalAndFilter()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("info", new Uri("http://somewhere/peopleInfo#"));
	prefixes.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));

	string name = "name";
	string age = "age";
	string person = "person";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { name, age })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(person)
					.PredicateUri("vcard:FN")
					.Object(name);
			})
		.Optional(
			(optionalBuilder) =>
			{
				optionalBuilder.Where(
					(triplePatternBuilder) =>
					{
						triplePatternBuilder
							.Subject(person)
							.PredicateUri($"info:{age}")
							.Object(age);
					});

				optionalBuilder.Filter((b) => b.Variable(age) > 42);
			});
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
```

## `UNION`
This query unions two triple patterns.

```SPARQL
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
PREFIX vcard: <http://www.w3.org/2001/vcard-rdf/3.0#>

SELECT ?name
WHERE
{
   { [] foaf:name ?name } UNION { [] vcard:FN ?name }
}
```

```C#
public static void WithUnion()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
	prefixes.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));

	string name = "name";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { name })
		.GetQueryBuilder();

	queryBuilder
		.Union(
			(unionBuilder) =>
			{
				unionBuilder.Where(
					(tripleBuilder) =>
				{
					tripleBuilder
					.Subject<IBlankNode>("abc")
					.PredicateUri($"foaf:{name}")
					.Object(name);
				});
			},
			(unionBuilder) =>
			{
				unionBuilder.Where(
					c =>
					{
						c
						.Subject<IBlankNode>("abc")
						.PredicateUri("vcard:FN")
						.Object(name);
					});
			});
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `ORDER BY`
This query orders by name.

```SPARQL
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>

SELECT ?name
WHERE { ?x foaf:name ?name }
ORDER BY ?name
```

```C#
static void WithOrderBy()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

	string name = "name";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { name })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject("x")
					.PredicateUri($"foaf:{name}")
					.Object(name);
			})
		.OrderBy(name);
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `DISTINCT`
This query enforces distinctness on names.

```SPARQL
PREFIX foaf:   < http://xmlns.com/foaf/0.1/>
SELECT DISTINCT ?name WHERE { ?x foaf:name? name }
```

```C#
static void WithDistinct()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

	string name = "name";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { name })
		.Distinct()
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject("x")
					.PredicateUri($"foaf:{name}")
					.Object(name);
			});
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `LIMIT`
This query limits the result set to 20.

```SPARQL
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

SELECT ?name
WHERE { ?x foaf:name ?name }
LIMIT 20
```

```C#
static void WithLimit()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

	string name = "name";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { name })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject("x")
					.PredicateUri($"foaf:{name}")
					.Object(name);
			})
		.Limit(20);
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## `BIND`
This query creates a computed column with an alias using `BIND`.

```SPARQL
PREFIX  dc:  <http://purl.org/dc/elements/1.1/>
PREFIX  ns:  <http://example.org/ns#>

SELECT  ?price
WHERE
{  ?x ns:price ?p .
   ?x ns:discount ?discount
   BIND (?p*(1-?discount) AS ?price)
}
```

```C#
static void WithBind()
{
	var prefixes = new NamespaceMapper(true);
	prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
	prefixes.AddNamespace("ns", new Uri("http://example.org/ns#"));

	string p = "p";
	string x = "x";
	string discount = "discount";
	string price = "price";
	var queryBuilder =
		QueryBuilder
		.Select(price)
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(x)
					.PredicateUri($"ns:{price}")
					.Object(p);
				triplePatternBuilder
					.Subject(x)
					.PredicateUri($"ns:{discount}")
					.Object(discount);
			})
		.Bind((builder) => builder.Variable(p) * (1 - builder.Variable(discount)))
			.As(price);
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```

## Term Constraints
This query demonstrates enforcing type constraints on terms.
```SPARQL
PREFIX info: < http://somewhere/peopleInfo#>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
SELECT ?resource
WHERE
  {
	?resource info:bornafter ?bornafter .
	FILTER(?bornafter >= "1970-01-01T00:00:00.000000"^^xsd:dateTime)
  }
```

```C#
static void WithTermConstraints()
{
	var prefixes = new NamespaceMapper(false);
	prefixes.AddNamespace("info", new Uri("http://somewhere/peopleInfo#"));

	string resource = "resource";
	string bornafter = "bornafter";
	var queryBuilder =
		QueryBuilder
		.Select(new string[] { resource })
		.Where(
			(triplePatternBuilder) =>
			{
				triplePatternBuilder
					.Subject(resource)
					.PredicateUri($"info:{bornafter}")
					.Object(bornafter);
			})
		.Filter((builder) => builder.Variable(bornafter) >= builder.Constant(new DateTime(1970, 1, 1)));
	queryBuilder.Prefixes = prefixes;

	Console.WriteLine(queryBuilder.BuildQuery().ToString());
}
```
