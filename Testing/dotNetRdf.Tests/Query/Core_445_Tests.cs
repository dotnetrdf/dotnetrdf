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
using System.Linq;
using System.Text;
using Xunit;
using System.Diagnostics;

namespace VDS.RDF.Query;


public class Core_445_Tests
{

    public static string baseQuery = @"
PREFIX type: <http://dbpedia.org/class/yago/>
PREFIX prop: <http://dbpedia.org/property/>

SELECT ?country_name ?population
WHERE {
";

    private static string randomizedPattern = @"
    ?{0} a type:LandlockedCountries ;
             rdfs:label ?{1} ;
             prop:populationEstimate ?{2} .
";

    private static IGraph g = new NonIndexedGraph();

    private static int randomBodyIterations = 1000;

    private static int runs = 100;
    //private static int averageExecutionTime;
    //private static int maxExecutionTime;

    private readonly HashSet<string> _variables = new HashSet<string>();
    private readonly Random _randomizer = new Random();

    private string BuildRandomSizeQuery()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < randomBodyIterations; i++)
        {
            var countryVar = "country_" + _randomizer.Next().ToString();
            var countryNameVar = "country_name_" + _randomizer.Next().ToString();
            var popNameVar = "population_name_" + _randomizer.Next().ToString();

            _variables.Add(countryVar);
            _variables.Add(countryNameVar);
            _variables.Add(popNameVar);

            sb.AppendFormat(randomizedPattern, countryVar, countryNameVar, popNameVar);
        }
        return sb.ToString();
    }

    [Fact]
    public void Core445_ParsingTest()
    {
        var randomizer = new Random();
        var command = new SparqlParameterizedString("SELECT * FROM <urn:with@char> FROM <urn:with?or$char> WHERE { ?subject rdfs:subClassOf ? $c . $c rdfs:label \"string literal with lang tag @inLiteralParamPattern \"@en }");
        command.SetVariable("subject", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetVariable("c", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetVariable("or", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetVariable("char", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetParameter("char", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetParameter("en", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        command.SetParameter("inLiteralParamPattern", g.CreateUriNode(UriFactory.Root.Create("urn:some-uri")));
        var output = command.ToString();
        Assert.True(output.Contains("<urn:with@char>"), "In IRI @ characters should not start a parameter capture");
        Assert.True(output.Contains("<urn:with?or$char>"), "In IRI ? and $ characters should not start a variable capture");
        Assert.True(output.Contains("rdfs:subClassOf ? "), "Property path ? quantifier should not start a variable capture");
        Assert.True(output.Contains("@en"), "Language tags should not start a parameter capture");
        Assert.True(output.Contains("@inLiteralParamPattern"), "In string literal @ characters should not start a parameter capture");
    }

    [Fact]
    public void Core445_Benchmark()
    {
        var timer = new Stopwatch();
        var randomQuery = BuildRandomSizeQuery();

        // Parsing performances
        timer.Start();
        var command = new SparqlParameterizedString(baseQuery);
        command.Append(randomQuery);
        command.Append("}");
        timer.Stop();

        Console.WriteLine("Query Size: " + command.CommandText.Length.ToString());
        Console.WriteLine("Variables: " + _variables.Count.ToString());
        Console.WriteLine("Parsing: " + timer.ElapsedMilliseconds.ToString());

        for (var i = 0; i < runs; i++)
        {
            Console.WriteLine("Run #" + i.ToString());
            timer.Reset();
            timer.Start();
            var variablesToSet = _randomizer.Next(_variables.Count);
            if (variablesToSet > _variables.Count / 2)
            {
                variablesToSet = _variables.Count;
            }
            foreach (var variable in _variables.Take(variablesToSet))
            {
                command.SetVariable(variable, g.CreateUriNode(UriFactory.Root.Create("urn:test#" + _randomizer.Next(randomBodyIterations).ToString())));
            }
            timer.Stop();
            Console.WriteLine(variablesToSet.ToString() + " Variables set: " + timer.ElapsedMilliseconds.ToString());
            timer.Reset();
            timer.Start();
            var commandString = command.ToString();
            timer.Stop();
            Console.WriteLine("ToString: " + timer.ElapsedMilliseconds.ToString());
        }
    }

}
