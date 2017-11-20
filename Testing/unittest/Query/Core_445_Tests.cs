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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using System.Diagnostics;

namespace VDS.RDF.Query
{

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
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < randomBodyIterations; i++)
            {
                string countryVar = "country_" + _randomizer.Next().ToString();
                string countryNameVar = "country_name_" + _randomizer.Next().ToString();
                string popNameVar = "population_name_" + _randomizer.Next().ToString();

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
            Random randomizer = new Random();
            SparqlParameterizedString command = new SparqlParameterizedString("SELECT * FROM <urn:with@char> FROM <urn:with?or$char> WHERE { ?subject rdfs:subClassOf ? $c . $c rdfs:label \"string literal with lang tag @inLiteralParamPattern \"@en }");
            command.SetVariable("subject", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetVariable("c", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetVariable("or", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetVariable("char", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetParameter("char", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetParameter("en", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            command.SetParameter("inLiteralParamPattern", g.CreateUriNode(UriFactory.Create("urn:some-uri")));
            string output = command.ToString();
            Assert.True(output.Contains("<urn:with@char>"), "In IRI @ characters should not start a parameter capture");
            Assert.True(output.Contains("<urn:with?or$char>"), "In IRI ? and $ characters should not start a variable capture");
            Assert.True(output.Contains("rdfs:subClassOf ? "), "Property path ? quantifier should not start a variable capture");
            Assert.True(output.Contains("@en"), "Language tags should not start a parameter capture");
            Assert.True(output.Contains("@inLiteralParamPattern"), "In string literal @ characters should not start a parameter capture");
        }

        [Fact]
        public void Core445_Benchmark()
        {
            Stopwatch timer = new Stopwatch();
            string randomQuery = BuildRandomSizeQuery();

            // Parsing performances
            timer.Start();
            SparqlParameterizedString command = new SparqlParameterizedString(baseQuery);
            command.Append(randomQuery);
            command.Append("}");
            timer.Stop();

            Console.WriteLine("Query Size: " + command.CommandText.Length.ToString());
            Console.WriteLine("Variables: " + _variables.Count.ToString());
            Console.WriteLine("Parsing: " + timer.ElapsedMilliseconds.ToString());

            for (int i = 0; i < runs; i++)
            {
                Console.WriteLine("Run #" + i.ToString());
                timer.Reset();
                timer.Start();
                int variablesToSet = _randomizer.Next(_variables.Count);
                if (variablesToSet > _variables.Count / 2)
                {
                    variablesToSet = _variables.Count;
                }
                foreach (string variable in _variables.Take(variablesToSet))
                {
                    command.SetVariable(variable, g.CreateUriNode(UriFactory.Create("urn:test#" + _randomizer.Next(randomBodyIterations).ToString())));
                }
                timer.Stop();
                Console.WriteLine(variablesToSet.ToString() + " Variables set: " + timer.ElapsedMilliseconds.ToString());
                timer.Reset();
                timer.Start();
                string commandString = command.ToString();
                timer.Stop();
                Console.WriteLine("ToString: " + timer.ElapsedMilliseconds.ToString());
            }
        }

    }
}
