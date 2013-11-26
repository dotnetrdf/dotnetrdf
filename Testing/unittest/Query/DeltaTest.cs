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
using NUnit.Framework;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class DeltaTest
    {
        private const string Turtle = @"
<http://r1> <http://r1> <http://r1> .
<http://r2> <http://r2> <http://r2> .
";

        private const string MinusQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    MINUS
    {
        GRAPH <http://g0> { ?s ?p ?o }
    }
}
";

        private const string OptionalSameTermQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://g0> { ?s ?p ?o0 . }
        FILTER (SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?o0))
}
";

        private const string OptionalInvertedSameTermQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://g0> { ?s ?p ?o0 . }
    }
    FILTER (!SAMETERM(?o, ?o0))
}
";

        [Test]
        public void SparqlGraphDeltas()
        {
            IGraph g1 = new Graph();
            IGraph g0 = new Graph();

            new TurtleParser().Load(g1, new StringReader(Turtle));
            new TurtleParser().Load(g0, new StringReader(Turtle));

            g1.BaseUri = new Uri("http://g1");
            g0.BaseUri = new Uri("http://g0");

            IInMemoryQueryableStore store = new TripleStore();
            store.Add(g1);
            store.Add(g0);

            using (SparqlResultSet resultSet = (SparqlResultSet)store.ExecuteQuery(MinusQuery))
            {
                Console.WriteLine("Minus: " + resultSet.Count);
                foreach (SparqlResult result in resultSet)
                {
                    Console.WriteLine(result.ToString());
                }
                Assert.AreEqual(0, resultSet.Count);
            }
            Console.WriteLine();

            using (SparqlResultSet resultSet = (SparqlResultSet)store.ExecuteQuery(OptionalSameTermQuery))
            {
                Console.WriteLine("OptionalSameTerm: " + resultSet.Count);
                foreach (SparqlResult result in resultSet)
                {
                    Console.WriteLine(result.ToString());
                }
                Assert.AreEqual(0, resultSet.Count);
            }
            Console.WriteLine();

            using (SparqlResultSet resultSet = (SparqlResultSet)store.ExecuteQuery(OptionalInvertedSameTermQuery))
            {
                Console.WriteLine("OptionalInvertedSameTerm: " + resultSet.Count);
                foreach (SparqlResult result in resultSet)
                {
                    Console.WriteLine(result.ToString());
                }
                Assert.AreEqual(0, resultSet.Count);
            }
        }
    }
}
