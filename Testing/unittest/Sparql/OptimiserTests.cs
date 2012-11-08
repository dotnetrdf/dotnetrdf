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
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class OptimiserTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
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

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0].IsAcceptAll, "First Triple Pattern should not be the ?s ?p ?o Pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].IsAcceptAll, "Second Triple Pattern should be the ?s ?p ?o pattern");
        }

        [TestMethod]
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

            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[0].Variables.Intersect(new String[] { "value", "label" }).Count() == 2, "Both ?label and ?value should be in the first triple pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Intersect(new String[] { "s", "value" }).Count() == 2, "Both ?s and ?value should be in the second triple pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[2].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the third triple pattern");

        }

        [TestMethod]
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

            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[0].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the first triple pattern of the child graph pattern");
            Assert.IsFalse(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[1].Variables.Contains("s"), "Second triple pattern of the child graph pattern should not contain ?s");
        }

        [TestMethod]
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

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0].PatternType == TriplePatternType.Filter, "First Triple Pattern should not be the FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].PatternType == TriplePatternType.Filter, "Second Triple Pattern should be the FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[2].PatternType == TriplePatternType.Filter, "Third Triple Pattern should not be the FilterPattern");
        }

        [TestMethod]
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

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should be a FilterPattern");
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns[1].Variables.Count, "First Filter should have 1 Variable");
            Assert.AreEqual("label", q.RootGraphPattern.TriplePatterns[1].Variables.First(), "First Filters Variable should be label");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[2] .PatternType == TriplePatternType.Filter, "Third Triple Pattern should not be a FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[3] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should be a FilterPattern");
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns[3].Variables.Count, "Second Filter should have 1 Variable");
            Assert.AreEqual("type", q.RootGraphPattern.TriplePatterns[3].Variables.First(), "Second Filters Variable should be type");
        }

        [TestMethod]
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

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should not be a FilterPattern");
            Assert.AreEqual(0, q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count, "Child Graph Pattern should have no Triple Patterns");
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Filter("), "Algebra should have a Filter() operator in it");
        }

        [TestMethod]
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

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] .PatternType == TriplePatternType.Filter, "First Triple Pattern should not be a FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[1] .PatternType == TriplePatternType.Filter, "Second Triple Pattern should not be a FilterPattern");
            Assert.AreEqual(0, q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count, "Child Graph Pattern should have no Triple Patterns");
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("LeftJoin("), "Algebra should have a LeftJoin() operator in it");
        }

        [TestMethod]
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
            Assert.IsInstanceOfType(algebra, typeof(Select));
            Assert.IsInstanceOfType(((Select)algebra).InnerAlgebra, typeof(Filter));
        }

        [TestMethod]
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
            Assert.IsTrue(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
        }

        [TestMethod]
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
            Assert.IsTrue(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
            Assert.IsTrue(algebra.Contains("AskUnion("), "Algebra should be optimised to use AskUnion()'s");
        }

        [TestMethod]
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
            Assert.IsTrue(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
        }

        [TestMethod]
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
            Assert.IsTrue(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
            Assert.IsTrue(algebra.Contains("LazyUnion("), "Algebra should be optimised to use LazyUnion()'s");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinSimple1()
        {
            String query = "SELECT * WHERE { ?x a ?type . ?y a ?type . FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinSimple2()
        {
            String query = "SELECT * WHERE { ?x a ?type . ?y a ?type . FILTER(SAMETERM(?x, ?y)) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinSimple3()
        {
            String query = "SELECT * WHERE { ?x a ?a . ?y a ?b . FILTER(?a = ?b) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsFalse(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
            Assert.IsTrue(algebra.Contains("Filter"), "Algebra should be optimised to use Filter");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinSimple4()
        {
            String query = "SELECT * WHERE { ?x a ?a . ?y a ?b . FILTER(SAMETERM(?a, ?b)) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should not be optimised to not use Filter");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinComplex1()
        {
            String query = "SELECT * WHERE { ?x a ?type . { SELECT ?y WHERE { ?y a ?type } }. FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsFalse(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplicitJoinComplex2()
        {
            String query = "SELECT * WHERE { ?x a ?type . OPTIONAL { ?y a ?type } . FILTER(?x = ?y) }";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Extend("), "Algebra should be optimised to use Extend");
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraImplictJoinComplex3()
        {
            String query = @"SELECT ?s ?v ?z
WHERE
{
  ?z a ?type .
  {    ?s a ?v .    FILTER(?v = ?z)  }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsFalse(algebra.Contains("Extend("), "Algebra should not be optimised to use Extend");
            Assert.IsTrue(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
        }

        [TestMethod]
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
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.IsTrue(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [TestMethod]
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
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.IsTrue(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [TestMethod]
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
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.IsTrue(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }

        [TestMethod]
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
            Assert.IsFalse(algebra.Contains("Filter("), "Algebra should be optimised to not use Filter");
            Assert.IsTrue(algebra.Contains("FilteredProduct("), "Algebra should be optimised to use FilteredProduct");
        }
    }
}
