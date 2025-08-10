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
using System.IO;
using VDS.RDF.Nodes;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;

namespace VDS.RDF.Query;

/// <summary>
/// Summary description for SparqlNewFunctions
/// </summary>

public class SparqlNewFunctions
{
    [Fact]
    public void SparqlFunctionsIsNumeric()
    {
        var g = new Graph();
        IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
        IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));

        g.Assert(subj, pred, (12).ToLiteral(g));
        g.Assert(subj, pred, g.CreateLiteralNode("12"));
        g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)));
        g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)));
        g.Assert(subj, pred, g.CreateLiteralNode("1200", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
        g.Assert(subj, pred, ((byte)50).ToLiteral(g));
        g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
        g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)));
        g.Assert(subj, pred, g.CreateUriNode(new Uri("http://example.org")));

        var store = new TripleStore();
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString("SELECT ?obj (IsNumeric(?obj) AS ?IsNumeric) WHERE { ?s ?p ?obj }");

        var processor = new LeviathanQueryProcessor(store);
        var results = processor.ProcessQuery(q);

        Assert.True(results is SparqlResultSet, "Result should be a SPARQL Result Set");
        TestTools.ShowResults(results);
    }

    [Fact]
    public void SparqlFunctionsNow()
    {
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromFile(Path.Combine("resources", "now01.rq"));
        var store = new TripleStore();
        var processor = new LeviathanQueryProcessor(store);
        if (processor.ProcessQuery(q) is SparqlResultSet results)
        {
            Assert.True(results.Result, "Result should be true");
        }
        else
        {
            Assert.Fail("Expected a non-null result");
        }
    }

    [Fact]
    public void SparqlFunctionsRand()
    {
        const string query = "SELECT ?s (RAND() AS ?rand) WHERE { ?s ?p ?o } ORDER BY ?rand";
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        object results = g.ExecuteQuery(query);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
    }

    [Fact]
    public void RandFunctionCachesResultAgainstBindingId()
    {
        var rand = new RandFunction();
        var context = new SparqlEvaluationContext(null, null, new LeviathanQueryOptions());
        var processor = new LeviathanExpressionProcessor(new LeviathanQueryOptions(), null);
        IValuedNode resultNode = rand.Accept(processor, context, 0);
        IValuedNode repeatResultNode = rand.Accept(processor, context, 0);
        Assert.Same(resultNode, repeatResultNode);
    }
}
