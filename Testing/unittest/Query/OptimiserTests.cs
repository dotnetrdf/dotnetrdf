/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class OptimiserTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        public OptimiserTests()
        {
            VDS.RDF.Options.AlgebraOptimisation = true;
        }
        [Fact]
        public void SparqlOptimiserQuerySimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.False(q.RootGraphPattern.TriplePatterns[0].IsAcceptAll, "First Triple Pattern should not be the ?s ?p ?o Pattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[1].IsAcceptAll, "Second Triple Pattern should be the ?s ?p ?o pattern");
        }

        [Fact]
        public void SparqlOptimiserQuerySimple2()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s a ?type .
  ?s <http://example.org/predicate> ?value .
  ?value rdfs:label ?label .
}";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.True(q.RootGraphPattern.TriplePatterns[0].Variables.Intersect(new String[] { "value", "label" }).Count() == 2, "Both ?label and ?value should be in the first triple pattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[1].Variables.Intersect(new String[] { "s", "value" }).Count() == 2, "Both ?s and ?value should be in the second triple pattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[2].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the third triple pattern");

        }

        [Fact]
        public void SparqlOptimiserQueryJoins()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o
  {
    ?type a rdfs:Class .
    ?s a ?type .
  }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.True(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[0].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the first triple pattern of the child graph pattern");
            Assert.False(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[1].Variables.Contains("s"), "Second triple pattern of the child graph pattern should not contain ?s");
        }

        [Fact]
        public void SparqlOptimiserQueryFilterPlacement()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  FILTER (LANGMATCHES(LANG(?label), 'en'))
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.False(q.RootGraphPattern.TriplePatterns[0].PatternType == TriplePatternType.Filter, "First Triple Pattern should not be the FilterPattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[1].PatternType == TriplePatternType.Filter, "Second Triple Pattern should be the FilterPattern");
            Assert.False(q.RootGraphPattern.TriplePatterns[2].PatternType == TriplePatternType.Filter, "Third Triple Pattern should not be the FilterPattern");
        }

        [Fact]
        public void SparqlOptimiserQueryFilterPlacement2()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  FILTER (LANGMATCHES(LANG(?label), 'en'))
  FILTER (SAMETERM(?type, rdfs:Class))
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.False(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should be a FilterPattern");
            Assert.Single(q.RootGraphPattern.TriplePatterns[1].Variables);
            Assert.Equal("label", q.RootGraphPattern.TriplePatterns[1].Variables.First());
            Assert.False(q.RootGraphPattern.TriplePatterns[2] .PatternType == TriplePatternType.Filter, "Third Triple Pattern should not be a FilterPattern");
            Assert.True(q.RootGraphPattern.TriplePatterns[3] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should be a FilterPattern");
            Assert.Single(q.RootGraphPattern.TriplePatterns[3].Variables);
            Assert.Equal("type", q.RootGraphPattern.TriplePatterns[3].Variables.First());
        }

        [Fact]
        public void SparqlOptimiserQueryFilterPlacement3()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  {
    FILTER (LANGMATCHES(LANG(?label), 'en'))
  }
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.False(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.False(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should not be a FilterPattern");
            Assert.Empty(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns);
            Assert.True(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("Filter("), "Algebra should have a Filter() operator in it");
        }

        [Fact]
        public void SparqlOptimiserQueryFilterPlacement4()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  OPTIONAL
  {
    FILTER (LANGMATCHES(LANG(?label), 'en'))
  }
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.False(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.False(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should not be a FilterPattern");
            Assert.Empty(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns);
            Assert.True(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("LeftJoin("), "Algebra should have a LeftJoin() operator in it");
        }

        [Fact]
        public void SparqlOptimiserQueryFilterPlacement5()
        {
            // given
            var query = new SparqlQuery { QueryType = SparqlQueryType.Select };
            query.AddVariable(new SparqlVariable("s", true));
            query.RootGraphPattern = new GraphPattern();
            var subj = new VariablePattern("s");
            var rdfType = new NodeMatchPattern(new UriNode(null, new Uri(RdfSpecsHelper.RdfType)));
            var type = new VariablePattern("type");
            var triplePattern = new TriplePattern(subj, rdfType, type);
            query.RootGraphPattern.AddTriplePattern(triplePattern);
            query.RootGraphPattern.AddFilter(new UnaryExpressionFilter(new InFunction(new VariableTerm("type"), new[]
                {
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type1"))), 
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type2"))), 
                    new ConstantTerm(new UriNode(null, new Uri("http://example.com/Type3")))
                })));

            // when
            var algebra = query.ToAlgebra();

            // then
            Assert.IsType<Select>(algebra);
            Assert.IsType<Filter>(((Select)algebra).InnerAlgebra);
        }

        [Fact]
        public void SparqlOptimiserAlgebraAskSimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
ASK WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));
            
            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
        }

        [Fact]
        public void SparqlOptimiserAlgebraAskUnion()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
ASK WHERE
{
  { ?s a ?type }
  UNION
  { ?s rdfs:label ?label }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
            Assert.True(algebra.Contains("AskUnion("), "Algebra should be optimised to use AskUnion()'s");
        }

        [Fact]
        public void SparqlOptimiserAlgebraSelectSimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
} LIMIT 10";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
        }

        [Fact]
        public void SparqlOptimiserAlgebraSelectUnion()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  { ?s a ?type }
  UNION
  { ?s rdfs:label ?label }
} LIMIT 10";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
            Assert.True(algebra.Contains("LazyUnion("), "Algebra should be optimised to use LazyUnion()'s");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinSimple1()
        {
            String query = "SELECT * WHERE { ?x a ?type . ?y a ?type . FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinSimple2()
        {
            String query = "SELECT * WHERE { ?x a ?type . ?y a ?type . FILTER(SAMETERM(?x, ?y)) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinSimple3()
        {
            String query = "SELECT * WHERE { ?x a ?a . ?y a ?b . FILTER(?a = ?b) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
            Assert.True(algebra.Contains("Filter"), "Algebra should be optimised to use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinSimple4()
        {
            String query = "SELECT * WHERE { ?x a ?a . ?y a ?b . FILTER(SAMETERM(?a, ?b)) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.False(algebra.Contains("Filter("), "Algebra should not be optimised to not use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinComplex1()
        {
            String query = "SELECT * WHERE { ?x a ?type . { SELECT ?y WHERE { ?y a ?type } }. FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplicitJoinComplex2()
        {
            String query = "SELECT * WHERE { ?x a ?type . OPTIONAL { ?y a ?type } . FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraImplictJoinComplex3()
        {
            String query = @"SELECT ?s ?v ?z
WHERE
{
  ?z a ?type .
  {
    ?s a ?v .
    FILTER(?v = ?z)
  }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
            Assert.True(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [Fact]
        public void SparqlOptimiserAlgebraFilteredProduct1()
        {
            String query = @"SELECT *
WHERE
{
    ?s1 ?p1 ?o1 .
    ?s2 ?p2 ?o2 .
    FILTER(?o1 = ?o2)
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.True(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [Fact]
        public void SparqlOptimiserAlgebraFilteredProduct2()
        {
            String query = @"SELECT *
WHERE
{
    ?s1 ?p1 ?o1 .
    ?s2 ?p2 ?o2 .
    FILTER(?o1 + ?o2 = 4)
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.True(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [Fact]
        public void SparqlOptimiserAlgebraFilteredProduct3()
        {
            String query = @"SELECT *
WHERE
{
    { ?s1 ?p1 ?o1 . }
    { ?s2 ?p2 ?o2 . }
    FILTER(?o1 = ?o2)
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.True(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [Fact]
        public void SparqlOptimiserAlgebraFilteredProduct4()
        {
            String query = @"SELECT *
WHERE
{
    { ?s1 ?p1 ?o1 . }
    { ?s2 ?p2 ?o2 . }
    FILTER(?o1 + ?o2 = 4)
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.False(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.True(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByDistinct1()
        {
            String query = "SELECT DISTINCT ?p WHERE { ?s ?p ?o } ORDER BY ?p";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.StartsWith("OrderBy("), "Algebra should be optimised to start with OrderBy");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByDistinct2()
        {
            String query = "SELECT DISTINCT ?s ?p WHERE { ?s ?p ?o } ORDER BY CONCAT(?s, ?p)";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.StartsWith("OrderBy("), "Algebra should be optimised to start with OrderBy");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByDistinct3()
        {
            String query = "SELECT DISTINCT * WHERE { ?s ?p ?o } ORDER BY ?p";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);

            //Should not apply since it does not have a fixed project list
            Assert.False(algebra.StartsWith("OrderBy("), "Algebra should not be optimised to start with OrderBy");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByReduced1()
        {
            String query = "SELECT REDUCED ?p WHERE { ?s ?p ?o } ORDER BY ?p";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.StartsWith("OrderBy("), "Algebra should be optimised to start with OrderBy");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByReduced2()
        {
            String query = "SELECT REDUCED ?s ?p WHERE { ?s ?p ?o } ORDER BY CONCAT(?s, ?p)";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.True(algebra.StartsWith("OrderBy("), "Algebra should be optimised to start with OrderBy");
        }

        [Fact]
        public void SparqlOptimiserAlgebraOrderByReduced3()
        {
            String query = "SELECT REDUCED * WHERE { ?s ?p ?o } ORDER BY ?p";
            SparqlQuery q = this._parser.ParseFromString(query);

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);

            //Should not apply since it does not have a fixed project list
            Assert.False(algebra.StartsWith("OrderBy("), "Algebra should not be optimised to start with OrderBy");
        }
    }
}
