/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Query
{

    public class DeltaTest
    {
        private readonly TurtleParser _parser = new TurtleParser();
        private readonly SparqlQueryParser _sparqlParser = new SparqlQueryParser();

        private const string TestData = @"
<http://r1> <http://r1> <http://r1> .
<http://r2> <http://r2> <http://r2> .
";

        private const string TestData2 = @"
<http://r1> <http://r1> <http://r1> , <http://r2> .
<http://r2> <http://r2> <http://r2> .
";

        private const string TestData3 = @"
<http://r1> <http://r1> <http://r1> , ""value"" .
<http://r2> <http://r2> <http://r2> , 1234 .
";

        private const string TestData4 = @"
<http://r1> <http://r1> <http://r1> , ""value"" , 1234, 123e4, 123.4, true, false .
<http://r2> <http://r2> <http://r2> .
";


        private const string MinusQuery = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    MINUS
    {
        GRAPH <http://b> { ?s ?p ?o }
    }
}
";

        private const string OptionalSameTermQuery1 = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://b> { ?s0 ?p0 ?o0 . }
        FILTER (SAMETERM(?s, ?s0) && SAMETERM(?p, ?p0) && SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?s0))
}
";

        private const string OptionalSameTermQuery2 = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://b> { ?s ?p ?o0 . }
        FILTER (SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?o0))
}
";

        private const string NotExistsQuery = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    FILTER NOT EXISTS { GRAPH <http://b> { ?s ?p ?o } }
}
";

        private void TestQuery(IInMemoryQueryableStore store, String query, String queryName, int differences)
        {
            Console.WriteLine(queryName);
            SparqlQuery q = this._sparqlParser.ParseFromString(query);
            Console.WriteLine(q.ToAlgebra().ToString());

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            using (SparqlResultSet resultSet = processor.ProcessQuery(q) as SparqlResultSet)
            {
                Assert.NotNull(resultSet);
                TestTools.ShowResults(resultSet);
                Assert.Equal(differences, resultSet.Count);
            }
            Console.WriteLine();
        }

        private void TestDeltas(IGraph a, IGraph b, int differences)
        {
            a.BaseUri = new Uri("http://a");
            b.BaseUri = new Uri("http://b");

            IInMemoryQueryableStore store = new TripleStore();
            store.Add(a);
            store.Add(b);

            this.TestQuery(store, MinusQuery, "Minus", differences);
            this.TestQuery(store, OptionalSameTermQuery1, "OptionalSameTerm1", differences);
            this.TestQuery(store, OptionalSameTermQuery2, "OptionalSameTerm2", differences);
            this.TestQuery(store, NotExistsQuery, "NotExists", differences);
        }

        [Fact]
        public void SparqlGraphDeltas1()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData));
            this._parser.Load(b, new StringReader(TestData));

            this.TestDeltas(a, b, 0);
            this.TestDeltas(b, a, 0);
        }

        [Fact]
        public void SparqlGraphDeltas2()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData));
            this._parser.Load(b, new StringReader(TestData2));

            this.TestDeltas(a, b, 0);
            this.TestDeltas(b, a, 1);
        }

        [Fact]
        public void SparqlGraphDeltas3()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData));
            this._parser.Load(b, new StringReader(TestData));
            b.Retract(b.GetTriplesWithSubject(new Uri("http://r1")).ToList());

            this.TestDeltas(a, b, 1);
            this.TestDeltas(b, a, 0);
            // TODO This should pass
        }

        [Fact]
        public void SparqlGraphDeltas4()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData));

            this.TestDeltas(a, b, 2);
            this.TestDeltas(b, a, 0);
        }

        [Fact]
        public void SparqlGraphDeltas5()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData3));
            this._parser.Load(b, new StringReader(TestData));

            this.TestDeltas(a, b, 2);
            this.TestDeltas(b, a, 0);
        }

        [Fact]
        public void SparqlGraphDeltas6()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            this._parser.Load(a, new StringReader(TestData4));
            this._parser.Load(b, new StringReader(TestData));

            this.TestDeltas(a, b, 6);
            this.TestDeltas(b, a, 0);
        }
    }
}