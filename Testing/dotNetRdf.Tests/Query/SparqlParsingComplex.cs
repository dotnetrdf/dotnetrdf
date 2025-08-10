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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class SparqlParsingComplex
{
    [Fact]
    public void SparqlParsingNestedGraphPatternFirstItem()
    {
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile(Path.Combine("resources", "childgraphpattern.rq"));
    }

    [Fact]
    public void SparqlParsingNestedGraphPatternFirstItem2()
    {
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile(Path.Combine("resources", "childgraphpattern2.rq"));
     }

    [Fact]
    public void SparqlParsingSubQueryWithLimitAndOrderBy()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var query = "SELECT * WHERE { { SELECT * WHERE {?s ?p ?o} ORDER BY ?p ?o LIMIT 2 } }";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet)
        {
            var rset = (SparqlResultSet)results;
            TestTools.ShowResults(rset);

            Assert.True(rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o")), "All Results should have had ?s, ?p and ?o variables");
        }
        else
        {
            Assert.Fail("Expected a SPARQL Result Set");
        }
    }

    //[Fact]
    //public void SparqlNonNormalizedUris()
    //{
    //    try
    //    {
    //        //Options.UriNormalization = false;

    //        SparqlQueryParser parser = new SparqlQueryParser();
    //        SparqlRdfParser rdfParser = new SparqlRdfParser(new TurtleParser());

    //        foreach (String file in Directory.GetFiles(Environment.CurrentDirectory))
    //        {
    //            if (Path.GetFileName(file).StartsWith("normalization") && Path.GetExtension(file).Equals(".ttl") && !Path.GetFileName(file).EndsWith("-results.ttl"))
    //            {
    //                QueryableGraph g = new QueryableGraph();
    //                FileLoader.Load(g, file);

    //                Console.WriteLine("Testing " + Path.GetFileName(file));

    //                SparqlQuery query = parser.ParseFromFile(Path.GetFileNameWithoutExtension(file) + ".rq");

    //                Object results = g.ExecuteQuery(query);
    //                if (results is SparqlResultSet)
    //                {
    //                    SparqlResultSet rset = (SparqlResultSet)results;

    //                    SparqlResultSet expected = new SparqlResultSet();
    //                    rdfParser.Load(expected, Path.GetFileNameWithoutExtension(file) + "-results.ttl");

    //                    if (!rset.Equals(expected))
    //                    {
    //                        Console.WriteLine("Expected Results");
    //                        Console.WriteLine();
    //                        foreach (SparqlResult r in expected)
    //                        {
    //                            Console.WriteLine(r.ToString());
    //                        }
    //                        Console.WriteLine();
    //                        Console.WriteLine("Actual Results");
    //                        Console.WriteLine();
    //                        foreach (SparqlResult r in rset)
    //                        {
    //                            Console.WriteLine(r.ToString());
    //                        }
    //                        Console.WriteLine();
    //                    }
    //                    Assert.Equal(rset, expected);
    //                }
    //                else
    //                {
    //                    Assert.True(false, "Didn't get a SPARQL Result Set as expected");
    //                }
    //            }
    //        }
    //    }
    //    finally
    //    {
    //        //Options.UriNormalization = true;
    //    }
    //}

    [Fact]
    public void SparqlParsingDescribeHangingWhere()
    {
        var valid = new List<string>()
        {
            "DESCRIBE ?s WHERE { ?s a ?type }",
            "DESCRIBE <http://example.org/>",
            "PREFIX ex: <http://example.org/> DESCRIBE ex:"
        };

        var invalid = new List<string>()
        {
            "DESCRIBE ?s WHERE"
        };

        var parser = new SparqlQueryParser();
        var formatter = new SparqlFormatter();
        foreach (var v in valid)
        {
                SparqlQuery q = parser.ParseFromString(v);
                Console.WriteLine(formatter.Format(q));
                Console.WriteLine();
        }

        foreach (var iv in invalid)
        {
            try
            {
                SparqlQuery q = parser.ParseFromString(iv);
                Assert.Fail("Should have thrown a Parsing Error");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine("Errored as expected");
                TestTools.ReportError("Parsing Error", parseEx);
            }
        }
    }

    [Fact]
    public void SparqlParsingConstructShortForm()
    {
        var valid = new List<string>()
        {
            "CONSTRUCT WHERE {?s ?p ?o }",
            "CONSTRUCT WHERE {?s a ?type }",
        };

        var invalid = new List<string>()
        {
            "CONSTRUCT {?s ?p ?o}",
            "CONSTRUCT WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) }",
            "CONSTRUCT WHERE { GRAPH ?g { ?s ?p ?o } }",
            "CONSTRUCT WHERE { ?s ?p ?o . OPTIONAL {?s a ?type}}",
            "CONSTRUCT WHERE { ?s a ?type . BIND (<http://example.org> AS ?thing) }",
            "CONSTRUCT WHERE { {SELECT * WHERE { ?s ?p ?o } } }"
        };

        var parser = new SparqlQueryParser();
        var formatter = new SparqlFormatter();
        foreach (var v in valid)
        {
            Console.WriteLine("Valid Input: " + v);
                SparqlQuery q = parser.ParseFromString(v);
                Console.WriteLine(formatter.Format(q));
            Console.WriteLine();
        }

        foreach (var iv in invalid)
        {
            Console.WriteLine("Invalid Input: " + iv);
            try
            {
                SparqlQuery q = parser.ParseFromString(iv);
                Assert.Fail("Should have thrown a Parsing Error");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine("Errored as expected");
                TestTools.ReportError("Parsing Error", parseEx);
            }
            Console.WriteLine();
        }
    }

    [Fact]
    public void SparqlEvaluationMultipleOptionals()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "multiple-options.trig"), new TriGParser(TriGSyntax.MemberSubmission));

        var parser = new SparqlQueryParser();
        SparqlQuery query = parser.ParseFromFile(Path.Combine("resources", "multiple-optionals.rq"));

        var processor = new LeviathanQueryProcessor(store);
        var results = processor.ProcessQuery(query);
        if (results is SparqlResultSet)
        {
            TestTools.ShowResults(results);
        }
        else
        {
            Assert.Fail("Expected a SPARQL Result Set");
        }
    }

    [Fact]
    public void SparqlEvaluationMultipleOptionals2()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "multiple-options.trig"), new TriGParser(TriGSyntax.MemberSubmission));

        var parser = new SparqlQueryParser();
        SparqlQuery query = parser.ParseFromFile(Path.Combine("resources", "multiple-optionals-alternate.rq"));

        var processor = new LeviathanQueryProcessor(store);
        var results = processor.ProcessQuery(query);
        if (results is SparqlResultSet)
        {
            TestTools.ShowResults(results);
        }
        else
        {
            Assert.Fail("Expected a SPARQL Result Set");
        }
    }

    [Fact]
    public void SparqlParsingSubqueries1()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
        var parser = new SparqlQueryParser();
        Assert.Throws<RdfParseException>(() => parser.ParseFromString(query));
    }

    [Fact]
    public void SparqlParsingSubqueries2()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        Console.WriteLine("Parsed original input OK");

        var query2 = q.ToString();
        SparqlQuery q2 = parser.ParseFromString(query2);
        Console.WriteLine("Parsed reserialized input OK");
    }

    [Fact]
    public void SparqlParsingSubqueries3()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
        var parser = new SparqlQueryParser();
        Assert.Throws<RdfParseException>(() => { SparqlQuery q = parser.ParseFromString(query); });
    }

    [Fact]
    public void SparqlParsingSubqueries4()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        Console.WriteLine("Parsed original input OK");

        var formatter = new SparqlFormatter();
        var query2 = formatter.Format(q);
        SparqlQuery q2 = parser.ParseFromString(query2);
        Console.WriteLine("Parsed reserialized input OK");
    }

    [Fact]
    public void SparqlParsingUpdateWithDeleteWhereIllegal()
    {
        const string update = "WITH <http://graph> DELETE WHERE { ?s ?p ?o }";
        var parser = new SparqlUpdateParser();
        Assert.Throws<RdfParseException>(() => parser.ParseFromString(update));
    }

    [Fact]
    public void SparqlParsingToStringRoundTripCore458()
    {
        const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
SELECT * 
WHERE 
{
  ?s ?p ?o .
  FILTER(?p = rdf:type) 
}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        var output = q.ToString();
        SparqlQuery q2 = parser.ParseFromString(output);
    }

    [Fact]
    public void SparqlParsingFilterWithTrailingDotInGroupGraphPatternSub()
    {
        new SparqlQueryParser().ParseFromString(@"
ASK 
{
    { }
    FILTER(true) .
}
");
    }
}
