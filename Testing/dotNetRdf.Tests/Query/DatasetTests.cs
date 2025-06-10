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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query;


public class DatasetTests
{
    private const string Data = @"<ex:default> <ex:default> <ex:default>.
<ex:from> <ex:from> <ex:from> <ex:from> .
<ex:named>  <ex:named> <ex:named> <ex:named> .
<ex:other>  <ex:other> <ex:other> <ex:other> .";

    private readonly ISparqlDataset _dataset;
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly ISparqlQueryProcessor _processor;

    public DatasetTests()
    {
        var store = new TripleStore();
        store.LoadFromString(Data);
        _dataset = new InMemoryQuadDataset(store, false);
        _processor = new LeviathanQueryProcessor(_dataset);
    }

    private void RunTest(string query, string[] expected, int expectedCount)
    {
        //Parse the query
        SparqlQuery q = _parser.ParseFromString(query);
        Console.WriteLine(query);

        //Then execute the query
        var results = _processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);

        var found = new List<string>();
        var count = 0;
        foreach (SparqlResult r in results)
        {
            count++;
            if (r.HasValue("s")) found.Add(r["s"].ToString());
        }

        var dumped = false;
        if (expectedCount != count)
        {
            //If incorrect dump output for debugging
            Dump(expectedCount, count, expected, found);
            dumped = true;
        }
        Assert.Equal(expectedCount, count);
        foreach (var e in expected)
        {
            if (!found.Contains(e))
            {
                if (!dumped)
                {
                    Dump(expectedCount, count, expected, found);
                    dumped = true;
                }
                Assert.Fail("Did not find expected result " + e);
            }
        }
    }

    private void Dump(int expectedCount, int actualCount, string[] expected, List<string> actual)
    {
        if (expectedCount != actualCount)
        {
            Console.WriteLine("Got incorrect number of results, expected " + expectedCount + " but got " + actualCount);
        }
        else
        {
            Console.WriteLine("Did not find an expected result");
        }
        Console.Write("Expected: ");
        foreach (var e in expected)
        {
            Console.Write(e + " ");
        }
        Console.WriteLine();
        Console.Write("Actual: ");
        foreach (var a in actual)
        {
            Console.Write(a + " ");
        }
        Console.WriteLine();
    }

    /*
     * This block of tests are for the case where we have all of FROM, FROM NAMED and GRAPH clause present
     */

    [Fact]
    public void SparqlDatasetResolutionFromAndNamedAndGraphUriExists()
    {
        //FROM
        //FROM NAMED
        //GRAPH clause with URI of an existing graph which is in the FROM NAMED list

        //Yields triples from <ex:named>
        RunTest("SELECT * FROM <ex:from> FROM NAMED <ex:named> { GRAPH <ex:named> { ?s ?p ?o } }", new string[] { "ex:named" }, 1);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndNamedAndGraphUriExistsNotInList()
    {
        //FROM
        //FROM NAMED
        //GRAPH clause with URI of an existing graph which is NOT in the FROM NAMED list

        //Yields no triples, tries to access a named graph not in the named graph list
        RunTest("SELECT * FROM <ex:from> FROM NAMED <ex:named> { GRAPH <ex:other> { ?s ?p ?o } }", new string[] { }, 0);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndNamedAndGraphUriMissing()
    {
        //FROM
        //FROM NAMED
        //GRAPH clause with URI of an existing graph which is in the FROM NAMED list

        //Yields no triples
        RunTest("SELECT * FROM <ex:from> FROM NAMED <ex:named> { GRAPH <ex:missing> { ?s ?p ?o } }", new string[] { }, 0);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndNamedAndGraphVar()
    {
        //FROM
        //FROM NAMED
        //GRAPH clause with variable

        //Yields triples from <ex:named>
        RunTest("SELECT * FROM <ex:from> FROM NAMED <ex:named> { GRAPH ?g { ?s ?p ?o } }", new string[] { "ex:named" }, 1);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndNamedAndGraphsVar()
    {
        //FROM
        //FROM NAMED
        //GRAPH clause with variable

        //Yields triples from <ex:named>
        RunTest("SELECT * FROM <ex:from> FROM NAMED <ex:named> FROM NAMED <ex:other> { GRAPH ?g { ?s ?p ?o } }", new string[] { "ex:named", "ex:other" }, 2);
    }

    /*
     * This block of tests are for the case where we have FROM and a GRAPH clause present
     */

    [Fact]
    public void SparqlDatasetResolutionFromAndGraphUriExists()
    {
        //FROM
        //No FROM NAMED
        //GRAPH clause with URI of an existing graph

        //Yields no triples
        RunTest("SELECT * FROM <ex:from> { GRAPH <ex:named> { ?s ?p ?o } }", new string[] { }, 0);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndGraphUriMissing()
    {
        //FROM
        //No FROM NAMED
        //GRAPH clause with URI of an existing graph

        //Yields no triples
        RunTest("SELECT * FROM <ex:from> { GRAPH <ex:missing> { ?s ?p ?o } }", new string[] { }, 0);
    }

    [Fact]
    public void SparqlDatasetResolutionFromAndGraphVar()
    {
        //FROM
        //No FROM NAMED
        //GRAPH clause with variable

        //Yields no triples
        RunTest("SELECT * FROM <ex:from> { GRAPH ?g { ?s ?p ?o } }", new string[] { }, 0);
    }

    /**
     * This block of tests are for the case where we have FROM NAMED and a GRAPH clause
     */

    [Fact]
    public void SparqlDatasetResolutionNamedGraphVar()
    {
        //No FROM
        //FROM NAMED
        //GRAPH clause with variable

        //Yields triples in <ex:named>
        RunTest("SELECT * FROM NAMED <ex:named> WHERE { GRAPH ?g { ?s ?p ?o } }", new string[] { "ex:named" }, 1);
    }

    [Fact]
    public void SparqlDatasetResolutionNamedGraphsVar()
    {
        //No FROM
        //FROM NAMED
        //GRAPH clause with variable

        //Yields triples in <ex:named> and <ex:other>
        RunTest("SELECT * FROM NAMED <ex:named> FROM NAMED <ex:other> WHERE { GRAPH ?g { ?s ?p ?o } }", new string[] { "ex:named", "ex:other" }, 2);
    }

    [Fact]
    public void SparqlDatasetResolutionNamedGraphUriExists()
    {
        //No FROM
        //FROM NAMED
        //GRAPH clause with URI of existing graph

        //Yields triples in <ex:named>
        RunTest("SELECT * FROM NAMED <ex:named> WHERE { GRAPH <ex:named> { ?s ?p ?o } }", new string[] { "ex:named" }, 1);
    }

    [Fact]
    public void SparqlDatasetResolutionNamedGraphUriMissing()
    {
        //No FROM
        //FROM NAMED
        //GRAPH clause with URI of non-existent graph

        //Yields triples in <ex:named>
        RunTest("SELECT * FROM NAMED <ex:named> WHERE { GRAPH <ex:missing> { ?s ?p ?o } }", new string[] { }, 0);
    }

    /**
     * This block of tests are for the case where we have a FROM only
     */

    [Fact]
    public void SparqlDatasetResolutionFrom()
    {
        //FROM
        //No FROM NAMED
        //No GRAPH clause

        //Yields triples from <ex:from>
        RunTest("SELECT * FROM <ex:from> WHERE { ?s ?p ?o }", new string[] { "ex:from" }, 1);
    }

    /**
     * This block of tests are for the cases where we only have a GRAPH clause
     */

    [Fact]
    public void SparqlDatasetResolutionGraphVar()
    {
        //No FROM
        //No FROM NAMED
        //GRAPH clause with variable

        //Yields triples in all named graphs
        RunTest("SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }", new string[] { "ex:from", "ex:named", "ex:other" }, 3);
    }

    [Fact]
    public void SparqlDatasetResolutionGraphUriExists()
    {
        //No FROM
        //No FROM NAMED
        //GRAPH clause with URI of an existing graph

        //Yields triples in the specific named graph
        RunTest("SELECT * WHERE { GRAPH <ex:named> { ?s ?p ?o } }", new string[] { "ex:named" }, 1);
    }

    [Fact]
    public void SparqlDatasetResolutionGraphUriMissing()
    {
        //No FROM
        //No FROM NAMED
        //GRAPH clause with URI of a non-existent graph

        //Yields no triples
        RunTest("SELECT * WHERE { GRAPH <ex:missing> { ?s ?p ?o } }", new string[] { }, 0);
    }

    /**
     * Tests where we have no explicit dataset definition of any kind
     */

    [Fact]
    public void SparqlDatasetResolutionNoDataset()
    {
        //No FROM
        //No FROM NAMED
        //No GRAPH clause

        //Yields only triples in the default graph
        RunTest("SELECT * WHERE { ?s ?p ?o }", new string[] { "ex:default" }, 1);
    }
}
