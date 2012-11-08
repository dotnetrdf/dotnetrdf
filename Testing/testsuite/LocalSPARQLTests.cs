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
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class LocalSparqlTests
    {
        public static List<String> TestQueries = new List<string>();
        public const long QueryTimeout = 60000;
        public static ISparqlResultsWriter[] ResultWriters = { new SparqlXmlWriter(), new SparqlJsonWriter(), new SparqlHtmlWriter() };
        public static String[] ResultExtensions = { ".srx", ".json", ".html" };

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("LocalSPARQLTests.txt");
            try
            {
                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## Local SPARQL Test Suite");

                //Create a directory for the Output
                if (!Directory.Exists("localsparql_tests"))
                {
                    Directory.CreateDirectory("localsparql_test");
                }

                //Get a Triple Store
                IInMemoryQueryableStore store = new TripleStore();

                Console.WriteLine();
                Console.WriteLine("Loading two Graphs into the Triple Store");
            
                //Load it with Data
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/1");
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");
                store.Add(g);

                Notation3Parser n3parser = new Notation3Parser();
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/2");
                n3parser.Load(h, "test.n3");
                store.Add(h);

                Console.WriteLine("# Triple Store Loading Done");
                Console.WriteLine();

                //Show the Triples
                Console.WriteLine("# Following Triples are in the Store");
                foreach (Triple t in store.Triples)
                {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }
                Console.WriteLine();
                Console.WriteLine(new String('-', 150));
                Console.WriteLine();

                //Create the Test Queries
                String stdprefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\nPREFIX rdfs: <" + NamespaceMapper.RDFS + ">\n";
                String xsdprefix = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n";
                String exprefix = "PREFIX eg: <http://example.org/vehicles/>\n";
                String vdsprefixes = "PREFIX vds: <http://www.vdesign-studios.com/dotNetRDF#>\nPREFIX ecs: <http://id.ecs.soton.ac.uk/person/>";
                String fnprefix = "PREFIX fn: <http://www.w3.org/2005/xpath-functions#>\n";
                String afnprefix = "PREFIX afn: <http://jena.hpl.hp.com/ARQ/function#>\n";

                //Simple SELECT and ASK
                TestQueries.Add("SELECT * WHERE {?s ?p ?o}");
                TestQueries.Add("ASK WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT ?s WHERE { }");
                TestQueries.Add("ASK WHERE { }");
                TestQueries.Add(stdprefixes + "ASK WHERE { ?s rdf:type rdf:NoSuchType }");

                //Simple DESCRIBE
                TestQueries.Add("DESCRIBE <http://wwww.nosuchthing.org>");
                TestQueries.Add(exprefix + "DESCRIBE eg:FordFiesta");
                TestQueries.Add(stdprefixes + exprefix + "DESCRIBE ?car WHERE {?car rdf:type eg:Car}");
                TestQueries.Add(stdprefixes + exprefix + "DESCRIBE eg:FordFiesta eg:AirbusA380 ?plane WHERE {?plane rdf:type eg:Plane}");
                TestQueries.Add("DESCRIBE ?s WHERE {?s ?p ?o FILTER(isBlank(?s))}");

                //Simple CONSTRUCT
                TestQueries.Add("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");
                TestQueries.Add(stdprefixes + "CONSTRUCT {?s ?p ?o} WHERE {?s rdf:type rdfs:Class . ?s ?p ?o}");
                TestQueries.Add(stdprefixes + exprefix + "CONSTRUCT {?s ?p ?o} WHERE {?s rdf:type eg:Car . ?s ?p ?o}");
                TestQueries.Add(stdprefixes + exprefix + "CONSTRUCT {_:bnodeTest rdfs:label ?s; a eg:Car; eg:Speed ?o} WHERE {?s rdf:type eg:Car . ?s eg:Speed ?o}");
                TestQueries.Add(stdprefixes + exprefix + "CONSTRUCT {[rdfs:label ?s; a eg:Car; eg:Speed ?o]} WHERE {?s rdf:type eg:Car . ?s eg:Speed ?o}");

                //DISTINCT and REDUCED
                TestQueries.Add("SELECT DISTINCT ?s WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT DISTINCT ?s WHERE {?s ?p ?o} LIMIT 10");
                TestQueries.Add("SELECT REDUCED ?s WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT REDUCED ?s WHERE {?s ?p ?o} LIMIT 10");

                //ORDER BY
                TestQueries.Add("SELECT ?s WHERE {?s ?p ?o} ORDER BY ?s LIMIT 10");
                TestQueries.Add("SELECT DISTINCT ?s WHERE {?s ?p ?o} ORDER BY ?s LIMIT 10");
                TestQueries.Add("SELECT ?s ?p ?o WHERE {?s ?p ?o} ORDER BY ?s ?p LIMIT 10");
                TestQueries.Add(stdprefixes + "SELECT ?o WHERE {?s ?p ?o FILTER IsLiteral(?o)} ORDER BY ASC(?o)");
                TestQueries.Add(stdprefixes + "SELECT ?o WHERE {?s ?p ?o FILTER IsLiteral(?o)} ORDER BY DESC(?o)");
                TestQueries.Add(stdprefixes + "SELECT ?o WHERE {?s ?p ?o FILTER IsLiteral(?o)} ORDER BY ASC(STR(?o))");
                TestQueries.Add(stdprefixes + exprefix + "SELECT * {?vehicle rdf:type ?vehicleclass OPTIONAL {?vehicle eg:Speed ?speed}} ORDER BY BOUND(?speed)");
                TestQueries.Add(stdprefixes + exprefix + "SELECT * {?vehicle rdf:type ?vehicleclass OPTIONAL {?vehicle eg:Speed ?speed}} ORDER BY DESC(BOUND(?speed))");
                TestQueries.Add(exprefix + "SELECT * {?vehicle eg:Speed ?speed} ORDER BY (STR(?speed))");
                TestQueries.Add(stdprefixes + exprefix + "SELECT * {?vehicle rdf:type ?vehicleclass OPTIONAL {?vehicle eg:Speed ?speed}} ORDER BY DATATYPE(?speed)");

                //More complex SELECTS
                TestQueries.Add(stdprefixes + "SELECT ?s WHERE {?s rdf:type rdfs:Class}");
                TestQueries.Add(stdprefixes + "SELECT ?class ?subclass WHERE {?class rdf:type rdfs:Class . ?subclass rdfs:subClassOf ?class .}");
                TestQueries.Add(stdprefixes + "SELECT ?class ?subclass WHERE {?subclass rdfs:subClassOf ?class . ?class rdf:type rdfs:Class .}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?plane WHERE {?planes rdfs:subClassOf eg:Plane . ?plane rdf:type ?planes}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?prop ?value WHERE {?prop rdfs:range eg:Vehicle . ?vehicle ?prop ?value}");
                TestQueries.Add(stdprefixes + "SELECT * {?subclass rdfs:subClassOf ?class . ?subclass1 rdfs:subClassOf ?class1 } ORDER BY ?subclass ?subclass1");
                //The next one takes a long time to execute, it should error due to Execution Timeout
                //TestQueries.Add(stdprefixes + "SELECT * {?subclass rdfs:subClassOf ?class . ?subclass1 rdfs:subClassOf ?class1 . ?subclass2 rdfs:subClassOf ?class2} ORDER BY ?subclass ?subclass1 ?subclass2");
                TestQueries.Add(stdprefixes + exprefix + "SELECT * {_:a rdf:type eg:Car . _:a ?p ?o }");

                //LIMIT and OFFSET
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass}");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} LIMIT 5");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} LIMIT 5 OFFSET 5");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} OFFSET 10 LIMIT 5");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} LIMIT 0");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} LIMIT 10 OFFSET 15");
                TestQueries.Add(stdprefixes + "SELECT ?vehicle {?vehicle rdf:type ?vehicleclass} LIMIT 10 OFFSET 25");

                //FROM and FROM Named
                TestQueries.Add(stdprefixes + "SELECT DISTINCT ?class FROM <http://example.org/1> WHERE {?class rdf:type rdfs:Class}");
                TestQueries.Add(stdprefixes + "SELECT DISTINCT ?class FROM NAMED <http://example.org/food/> WHERE {?class rdf:type rdfs:Class}");

                //OPTIONAL
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed}}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER (!BOUND(?speed))}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed ?passengers WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} OPTIONAL { ?vehicle eg:PassengerCapacity ?passengers}}");

                //FILTER
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER BOUND(?speed)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed FILTER (?speed > 500)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed FILTER (?speed > 100 && ?speed < 200)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed FILTER (?speed < 100 || ?speed > 200)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed FILTER (?speed + 50 >= 200)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass . ?vehicle eg:Speed ?speed FILTER (?speed + (50 * 3) >= 200)}");
                TestQueries.Add(stdprefixes + "SELECT ?o WHERE {?s ?p ?o FILTER(<http://what>)}");
                TestQueries.Add(stdprefixes + "SELECT ?o WHERE {?s ?p ?o FILTER IsLiteral(?o)}");
                TestQueries.Add(stdprefixes + "SELECT DISTINCT ?o WHERE {?s ?p ?o FILTER IsURI(?o)}");
                TestQueries.Add(stdprefixes + "SELECT * WHERE {?s ?p ?o FILTER (STR(?o) = \"150\")}");
                TestQueries.Add(stdprefixes + xsdprefix + "SELECT * WHERE {?s ?p ?o FILTER (DATATYPE(?o) = xsd:integer)}");
                TestQueries.Add(stdprefixes + "SELECT ?subclass ?subclass1 {?subclass rdfs:subClassOf ?class . ?subclass1 rdfs:subClassOf ?class1 FILTER SAMETERM(?subclass,?subclass1)}");
                TestQueries.Add("SELECT * {?s ?p ?o FILTER(LANG(?o) = \"\")}");
                TestQueries.Add("SELECT * {?s ?p ?o FILTER(LANGMATCHES(?o,\"*\"))}");
                TestQueries.Add("SELECT * {?s ?p ?o FILTER(LANGMATCHES(?o,\"en\"))}");
                TestQueries.Add("SELECT * {?s ?p ?o FILTER(LANGMATCHES(?o,\"fr\"))}");
                TestQueries.Add("SELECT * {?s ?p ?o FILTER(LANGMATCHES(?o,\"\"))}");
                TestQueries.Add(exprefix + "SELECT * {?vehicle eg:Speed ?speed FILTER REGEX(?speed,\"1\\d+\")}");
                TestQueries.Add(exprefix + "SELECT * {?vehicle eg:Speed ?speed FILTER(REGEX(?speed,\"1\\d+\"))}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed . FILTER(?speed < 200)}}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?vehicle ?speed ?passengers WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed . FILTER(?speed < 200) OPTIONAL {?vehicle eg:PassengerCapacity ?passengers}}}");

                //XPath Casting
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:boolean(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:dateTime(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:decimal(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:double(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:float(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:integer(?speed))}");
                TestQueries.Add(stdprefixes + xsdprefix + exprefix + "SELECT ?vehicle ?speed WHERE {?vehicle rdf:type ?vehicleclass OPTIONAL { ?vehicle eg:Speed ?speed} FILTER(xsd:string(?speed))}");

                //GRAPH
                TestQueries.Add(stdprefixes + "SELECT ?class FROM NAMED <http://example.org/1> WHERE {?class rdf:type rdfs:Class}");
                TestQueries.Add(stdprefixes + "SELECT ?class FROM NAMED <http://example.org/1> WHERE {GRAPH <http://example.org/1> {?class rdf:type rdfs:Class}}");
                TestQueries.Add(stdprefixes + vdsprefixes + "SELECT * FROM NAMED <http://example.org/1> FROM NAMED <http://example.org/2> WHERE {GRAPH <http://example.org/1> {?class rdf:type rdfs:Class} GRAPH <http://example.org/2> {?supervisor vds:supervises ?student}}");
                TestQueries.Add("SELECT DISTINCT ?s ?src WHERE {GRAPH ?src {?s ?p ?o}}");
                TestQueries.Add("SELECT * FROM NAMED <http://example.org/1> FROM NAMED <http://example.org/2> WHERE { GRAPH ?src {?s ?p ?o}} ORDER BY ?src ?s ?p");
                TestQueries.Add(stdprefixes + "SELECT ?s ?p ?o FROM NAMED <http://example.org/1> FROM NAMED <http://example.org/2> WHERE {GRAPH <http://example.org/2> {?subj rdfs:seeAlso ?src} GRAPH ?src {?s ?p ?o}}");
                TestQueries.Add(stdprefixes + "SELECT ?s ?src FROM <http://example.org/2> WHERE {GRAPH ?src {?s ?p ?o}}");
                TestQueries.Add("SELECT ?g WHERE {GRAPH ?g { }}");

                //UNION
                TestQueries.Add("SELECT * {{?s ?p ?o} UNION {?s ?p ?o}}");
                TestQueries.Add("SELECT * {{?s ?p ?o} UNION {?s ?p ?o} FILTER(IsLiteral(?o))} ORDER BY ?s ?p");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?plane WHERE {{?plane rdf:type eg:Plane} UNION {?subclass rdfs:subClassOf eg:Plane . ?plane rdf:type ?subclass}}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?plane ?speed WHERE {{?plane rdf:type eg:Plane} UNION {?subclass rdfs:subClassOf eg:Plane . ?plane rdf:type ?subclass} OPTIONAL {?plane eg:Speed ?speed}}");

                //Aggregates
                TestQueries.Add("SELECT COUNT(*) WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT COUNT(?s) WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT COUNT(DISTINCT *) WHERE {?s ?p ?o}");
                TestQueries.Add("SELECT COUNT(DISTINCT ?s) AS ?Subjects WHERE {?s ?p ?o}");
                TestQueries.Add(exprefix + "SELECT MAX(?speed) WHERE {?vehicle eg:Speed ?speed .}");
                TestQueries.Add(exprefix + "SELECT MIN(?speed) WHERE {?vehicle eg:Speed ?speed .}");
                TestQueries.Add("SELECT ?o {?s ?p ?o} ORDER BY ?o");
                TestQueries.Add("SELECT MAX(?o) WHERE {?s ?p ?o .}");
                TestQueries.Add("SELECT MEDIAN(?o) WHERE {?s ?p ?o.}");
                TestQueries.Add("SELECT MIN(?o) WHERE {?s ?p ?o .}");
                TestQueries.Add("SELECT MODE(?o) WHERE {?s ?p ?o .}");
                TestQueries.Add("SELECT NMAX(?o) WHERE {?s ?p ?o .}");
                TestQueries.Add("SELECT NMIN(?o) WHERE {?s ?p ?o .}");
                TestQueries.Add(exprefix + "SELECT AVG(?speed) AS ?AverageSpeed WHERE {?vehicle eg:Speed ?speed }");
                TestQueries.Add(exprefix + "SELECT SUM(?speed) AS ?TotalSpeeds WHERE {?vehicle eg:Speed ?speed }");

                //Projection
                TestQueries.Add(exprefix + "SELECT ?vehicle (?speed * 2) AS ?DoubleSpeed WHERE {?vehicle eg:Speed ?speed }");
                TestQueries.Add(exprefix + "SELECT ?vehicle (?speed * 2) AS ?DoubleSpeed WHERE {?vehicle eg:Speed ?speed } ORDER BY DESC(?DoubleSpeed)");
                TestQueries.Add(exprefix + "SELECT ?vehicle (1 / ?speed) AS ?Test WHERE {?vehicle eg:Speed ?speed } ORDER BY ?Test");
                TestQueries.Add(exprefix + "SELECT ?vehicle (?speed >= 700) AS ?BreaksSoundBarrier WHERE {?vehicle eg:Speed ?speed } ORDER BY ?BreaksSoundBarrier");
                TestQueries.Add(exprefix + "SELECT ?vehicle (?speed >= 700) AS ?BreaksSoundBarrier WHERE {?vehicle eg:Speed ?speed } ORDER BY DESC(?BreaksSoundBarrier)");

                //Optimisable Queries
                TestQueries.Add(exprefix + "SELECT * WHERE {?s ?p ?o . ?s a eg:Car}");
                TestQueries.Add(exprefix + "SELECT ?s ?speed WHERE {?s eg:Speed ?speed . ?s a eg:Car}");
                TestQueries.Add(exprefix + "SELECT * WHERE {?vehicle ?prop ?value . ?vehicle eg:Speed ?speed . ?vehicle eg:PassengerCapacity ?passengers }");
                TestQueries.Add(vdsprefixes + "SELECT * WHERE {ecs:11471 vds:has ?prop . ?prop vds:is \"a test\"}");

                //Testing FILTER placement
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . ?car eg:Speed ?speed . ?car eg:PassengerCapacity ?passengers . FILTER(?speed > 100) . FILTER(?passengers > 2)}");

                //EXISTS and NOT EXISTS
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . EXISTS { ?car eg:Speed ?speed }}");
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . NOT EXISTS { ?car eg:Speed ?speed }}");
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . EXISTS {?car eg:PassengerCapacity ?passengers }}");
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . UNSAID {?car eg:PassengerCapacity ?passengers }}");

                //LET
                TestQueries.Add("SELECT ?z WHERE { LET(?b := (2)) LET(?a := (?b*4)) LET(?c := (1)) LET(?z := (?a+?c)) }");
                TestQueries.Add(exprefix + "SELECT ?speed ?doubleSpeed WHERE {?car a eg:Car . ?car eg:Speed ?speed . LET (?doubleSpeed := (?speed * 2)) }");
                TestQueries.Add(exprefix + "SELECT ?speed ?doubleSpeed WHERE {?car a eg:Car . OPTIONAL {?car eg:Speed ?speed }. OPTIONAL{ LET (?doubleSpeed := (?speed * 2))} }");
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . OPTIONAL { ?car eg:Speed ?speed . LET(?doubleSpeed := (?speed * 2))}}");
                TestQueries.Add("SELECT * WHERE { LET(?a := (2)) . LET(?a := (\"Some String\")) }");
                TestQueries.Add(exprefix + "SELECT * WHERE {?vehicle a ?type . LET (?type := (eg:Plane))}");
                TestQueries.Add(exprefix + "SELECT * WHERE {?car a eg:Car . LET(?car := (eg:MiniCooper))}");

                //Sub-query
                TestQueries.Add(vdsprefixes + "SELECT * WHERE {?academic vds:supervises ecs:11471 . { SELECT * WHERE {?academic vds:collaborates ?collaborator}}}");
                TestQueries.Add("SELECT * WHERE {?s a ?type . {SELECT ?s COUNT(?s) AS ?Triples WHERE {?s ?p ?o} GROUP BY ?s} FILTER(?Triples >= 3)}");
                TestQueries.Add("SELECT ?s ?Triples ?Type2 WHERE {?s a ?type . {SELECT ?s COUNT(?s) AS ?Triples WHERE {?s ?p ?o} GROUP BY ?s} {SELECT ?s ?Type2 WHERE {?s a ?Type2}} FILTER(?Triples >= 3)}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?car ?speed WHERE {?car a eg:Car {SELECT ?car ?speed WHERE {?car eg:Speed ?speed}}}");
                TestQueries.Add(stdprefixes + exprefix + "SELECT ?car ?speed WHERE {?car a eg:Car OPTIONAL {SELECT ?car ?speed WHERE {?car eg:Speed ?speed}}}");

                //XPath String Functions
                TestQueries.Add(fnprefix + "SELECT ?o WHERE {?s ?p ?o FILTER(fn:contains(?o, \"a\"))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:encode-for-uri(?o)) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o WHERE {?s ?p ?o FILTER(fn:ends-with(?o, \"s\"))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:false()) AS ?funcResult WHERE {?s ?p ?o}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:lower-case(?o)) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o WHERE {?s ?p ?o FILTER(fn:matches(?o, \"a\"))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:normalize-space(?o)) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:normalize-unicode(?o)) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:normalize-unicode(?o, \"NFD\")) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:not(?o)) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:replace(?o, \"a\", \"b\")) AS ?funcResult WHERE {?s ?p ?o . FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o WHERE {?s ?p ?o FILTER(fn:starts-with(?o, \"a\"))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:string-length(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring(?o, 5)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring(?o, 50)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring(?o, 5, 5)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring(?o, 5, 0)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring-after(?o, \"a\")) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:substring-before(?o, \"a\")) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:true()) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:upper-case(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(stdprefixes + fnprefix + "SELECT ?class ?class1 (fn:compare(STR(?class),STR(?class1))) AS ?funcResult {?subclass rdfs:subClassOf ?class . ?subclass1 rdfs:subClassOf ?class1 } ORDER BY ?class ?class1");

                //XPath Numeric Functions
                TestQueries.Add(fnprefix + "SELECT ?o (fn:abs(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:ceiling(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:floor(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:round(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:round-half-to-even(?o)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");
                TestQueries.Add(fnprefix + "SELECT ?o (fn:round-half-to-even(?o, 4)) AS ?funcResult WHERE {?s ?p ?o FILTER(ISLITERAL(?o))}");

                //ARQ Functions
                TestQueries.Add(afnprefix + "SELECT ?s (afn:localname(?s)) AS ?LocalName (afn:namespace(?s)) AS ?Namespace WHERE {?s ?p ?o . FILTER(ISURI(?s))}");
                TestQueries.Add(afnprefix + "SELECT ?p (afn:localname(?p)) AS ?LocalName (afn:namespace(?p)) AS ?Namespace WHERE {?s ?p ?o . FILTER(ISURI(?p))}");
                TestQueries.Add(afnprefix + "SELECT ?o (afn:localname(?o)) AS ?LocalName (afn:namespace(?o)) AS ?Namespace WHERE {?s ?p ?o . FILTER(ISURI(?o))}");
                TestQueries.Add(afnprefix + "SELECT ?s (afn:now()) AS ?Now WHERE {?s ?p ?o}");
                TestQueries.Add(afnprefix + "SELECT (afn:sha1sum(<http://example.org/book/book5>)) AS ?SHA1 WHERE {}");
                TestQueries.Add(afnprefix + "SELECT (afn:sha1sum(\"Harry Potter and the Order of the Phoenix\")) AS ?SHA1 WHERE {}");
                TestQueries.Add(xsdprefix + afnprefix + "SELECT (afn:sha1sum(1)) AS ?SHA1 WHERE {}");
                TestQueries.Add(afnprefix + "SELECT (afn:sha1sum(\"Some Text\")) AS ?SHA1 WHERE {}");
                TestQueries.Add(afnprefix + "SELECT (afn:sha1sum(\"Some Text\"@en)) AS ?SHA1 WHERE {}");
                TestQueries.Add(afnprefix + "SELECT (afn:sha1sum(\"Some Text\"@fr)) AS ?SHA1 WHERE {}");
                TestQueries.Add(xsdprefix + afnprefix + "SELECT (afn:sha1sum(\"Some Text\"^^xsd:unknownType)) AS ?SHA1 WHERE {}");
                TestQueries.Add(xsdprefix + afnprefix + "SELECT (afn:sha1sum(\"2010-04-01T15:36:00+00:00\"^^xsd:dateTime)) AS ?SHA1 WHERE { }");
        
                //Options.QueryOptimisation = false;
                //Options.QueryDefaultSyntax = SparqlQuerySyntax.Sparql_1_0;
                //Options.QueryDefaultSyntax = SparqlQuerySyntax.Sparql_1_1;
                Options.QueryOptimisation = true;
                Options.QueryDefaultSyntax = SparqlQuerySyntax.Extended;

                //Register the ARQ Function Library
                SparqlExpressionFactory.AddCustomFactory(new ArqFunctionFactory());

                //Get the Sparql Parser
                SparqlQueryParser sparqlparser;
                try
                {
                    sparqlparser = new SparqlQueryParser();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.InnerException.Message);
                        Console.WriteLine(ex.InnerException.StackTrace);
                    }
                    output.Close();
                    return;
                }
                //sparqlparser.TraceTokeniser = true;

                //Run the Test Queries
                int i = 0;
                Stopwatch parseTimer = new Stopwatch();
                foreach (String q in TestQueries)
                {
                    Console.WriteLine();
                    Console.WriteLine("# Raw Query Input");
                    Console.WriteLine(q);
                    Console.WriteLine();
                    Debug.WriteLine(q);
                    try
                    {
                        parseTimer.Start();
                        SparqlQuery query = sparqlparser.ParseFromString(q);
                        parseTimer.Stop();
                        Console.Write("# Parsed Query Input");
                        Console.WriteLine(" - Took " + parseTimer.ElapsedMilliseconds + "ms (" + parseTimer.ElapsedTicks + " ticks) to parse");
                        Console.WriteLine(query.ToString());
                        Console.WriteLine();
                        TestQuery(store, query, "localsparql_test/" + i);
                    }
                    catch (Exception ex)
                    {
                        parseTimer.Stop();
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine();
                            Console.WriteLine(ex.InnerException.Message);
                            Console.WriteLine(ex.InnerException.StackTrace);
                        }

                        Console.WriteLine();
                        Console.WriteLine(new String('-', 150));
                    }
                    parseTimer.Reset();

                    i++;
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            output.Close();
        }

        public static void TestQuery(IInMemoryQueryableStore store, SparqlQuery query, String outputFilename)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                query.Timeout = QueryTimeout;
                query.PartialResultsOnTimeout = false;
                Object temp = store.ExecuteQuery(query);
                watch.Stop();

                if (temp is SparqlResultSet)
                {
                    SparqlResultSet results = (SparqlResultSet)temp;
                    Console.WriteLine("Result = " + results.Result);
                    foreach (SparqlResult result in results)
                    {
                        Console.WriteLine(result.ToString());
                    }

                    ISparqlResultsWriter resultWriter;
                    for (int i = 0; i < ResultWriters.Length; i++) 
                    {
                        resultWriter = ResultWriters[i];
                        resultWriter.Save(results, outputFilename + ResultExtensions[i]);
                    }
                }
                else if (temp is IGraph)
                {
                    IGraph g = (IGraph)temp;
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                watch.Stop();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
            Console.WriteLine();
            Console.WriteLine("Query reports execution time of " + query.QueryExecutionTime);
            Console.WriteLine("Total Query Execution took " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine();
            Console.WriteLine(new String('-', 150));


        }
    }
}
