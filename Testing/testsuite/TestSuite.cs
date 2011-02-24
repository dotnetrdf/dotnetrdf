using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Parsing.Events;

namespace dotNetRDFTest
{
    /// <summary>
    /// Class which invokes the individual Test Suite Classes
    /// </summary>
    public class TestSuite
    {
        public const String TestSuiteTimeFormat = "HH:mm:ss.ffff";

        public static void Main(string[] args)
        {
            String optionlist = String.Empty;

            Console.Title = "dotNetRDF Test Suite";
            TextWriter stdout = Console.Out;
            Console.WriteLine("## dotNetRDF Test Suite");
            Console.WriteLine();

            Console.WriteLine("Redirect Debug Trace to Console (y/n)? ");
            char option = (char)Console.Read();
            if (option == 'y' || option == 'Y')
            {
                Debug.Listeners.Add(new TextWriterTraceListener(stdout));
                Console.WriteLine("Debug Trace redirected to Console");
            }
            Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("# Starting Running Tests");

            //Call Graph Building Example
            //GraphBuildingExample.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Graph Building Example 1 Done");

            //Call 2nd Graph Build Example
            //GraphBuildingExample2.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Graph Building Example 2 Done");

            //Call Uri Resolution Test
            //URIResolutionTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("URI Resolution Test Done");

            //Call Hash Code Test
            //HashCodeTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Hash Code Tests Done");

            //Call Uri Node Equality Tests
            //URINodeEqualityTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("URI Node Equality Test Done");

            //Call Thread Safe Graph Test
            //ThreadSafeGraphTest test = new ThreadSafeGraphTest(8,100);
            //test.RunTest();
            Console.SetOut(stdout);
            Console.WriteLine("Thread Safe Graph Test Done");

            //Call Query Speed Tests
            //while (!optionlist.Equals("n"))
            //{
            //    Console.WriteLine("Enter options for Query Speed Tests or  leave blank for defaults or enter 'skip' to skip");
            //    Console.WriteLine("Options format is 'Runs'");
            //    optionlist = Console.ReadLine();

            //    if (optionlist.Equals(String.Empty))
            //    {
            //        QuerySpeedTest.Main(new string[] { "1000" });
            //    }
            //    else if (optionlist.Equals("skip"))
            //    {
            //        //Exit while loop
            //        break;
            //    }
            //    else
            //    {
            //        String[] options = optionlist.Split(',');
            //        QuerySpeedTest.Main(options);
            //    }

            //    Console.WriteLine("Run again (y/n)? ");
            //    optionlist = Console.ReadLine();
            //}
            Console.SetOut(stdout);
            Console.WriteLine("Query Speed Tests Done");

            //Call Inference Tests
            //InferenceTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Inference Tests Done");

            //Call Merge Tests
            //MergeTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Graph Merging Tests Done");

            //Call XML/RDF Test Suite
            //RdfXmlTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("RDF/XML Parser Tests Done");

            //Call Notation3 Test Suite
            //Notation3TestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Notation 3 Parser Tests Done");

            //Call Turtle Test Suite
            //TurtleTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Turtle Parser Tests Done");

            //Call the NTriples Test Suite
            //NTriplesTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("NTriples Parser Tests Done");

            //Call the Json Test Suite
            //JsonTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("JSON Tests Done");

            //Call the RDFa Test Suite
            //RdfATestSuite.Main(args);
            Console.SetOut(stdout);
            Console.Write("RDFa Tests Done");

            //Call the TriG Test Suite
            //TriGTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("TriG Tests Done");

            //Call the NQuads Test Suite
            //NQuadsTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("NQuads Tests Done");

            //Call the TriX Test Suite
            //TriXTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("TriX Tests Done");

            //Call the Sparql Parser Test Suite
            //SparqlTestSuite.Main(args);
            //Console.SetOut(stdout);
            Console.WriteLine("SPARQL Query Parser Tests Done");

            //Call the SPARQL Evaluation Test Suite
            SparqlEvaluationTestSuite sparqlEvaluation = new SparqlEvaluationTestSuite();
            sparqlEvaluation.RunTests();
            Console.SetOut(stdout);
            Console.WriteLine("SPARQL Evaluation Tests Done");

            //Call the SPARQL 1.1 Evaluation Test Suite
            Sparql11EvaluationTestSuite sparql11Evaluation = new Sparql11EvaluationTestSuite();
            sparql11Evaluation.RunTests();
            Console.SetOut(stdout);
            Console.WriteLine("SPARQL 1.1 Evaluation Tests Done");

            //Call the Local Sparql Test Suite
            //LocalSparqlTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("SPARQL Local Query Tests Done");

            //Call the Remote Sparql Test Suite
            //RemoteSparqlTestSuite.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("SPARQL Remote Endpoint Tests Done");

            //Call the SPARQL Update Test Suite
            //SparqlUpdateParserTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("SPARQL Update Parser Tests Done");

            //Call Parser Speed Tests
            //ParserSpeedTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Parser Speed Comparison Tests Done");

            //Call Sorting Tests
            //SortingTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Node Sorting Tests Done");

            //Call SQL Store Test
            //Parameters are {TestMode,Runs,ReuseManager,UseThreading,NoThreads}
            //TestMode is read/write
            //Console.WriteLine("Enter options for SQL Store Test or leave blank for defaults or enter 'skip' to skip test");
            //Console.WriteLine("Options format is 'TestMode,Runs,ReuseManager,UseThreading,NumThreads'");
            //optionlist = Console.ReadLine();
            //if (optionlist.Equals(String.Empty))
            //{
            //    SqlStoreTest.Main(new string[] { "read", "30", "false", "true", "8" });
            //}
            //else if (optionlist.Equals("skip", StringComparison.OrdinalIgnoreCase))
            //{
            //    //Do Nothing
            //}
            //else
            //{
            //    String[] options = optionlist.Split(',');
            //    SqlStoreTest.Main(options);
            //}
            Console.SetOut(stdout);
            Console.WriteLine("SQL Store Tests Done");

            //Call MySQL Store Test
            //MySQLStoreTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("MySQL Store Tests Done");

            //Call Serialization Tests
            //WriterTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("RDF Serializers Tests Done");

            //Call Triple Store Tests
            //TripleStoreTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Triple Store Tests Done");

            //Call Folder Store Tests
            //FolderStoreTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Folder Store Tests Done");

            //Call Read/Write Speed Tests
            //Parameters are {TestMode,Runs,AllowHighSpeedWrites}
            //Test Mode is read/write/both
            //Console.WriteLine("Enter options for Parser & Serializer Speed Benchmarking Tests or leave blank for defaults or enter 'skip' to skip test");
            //Console.WriteLine("Options format is 'TestMode,Runs,AllowHighSpeedWrites'");
            //optionlist = Console.ReadLine();
            //if (optionlist.Equals(String.Empty))
            //{
            //    DiskIOSpeedTests.Main(new string[] { "write", "30", "false" });
            //}
            //else if (optionlist.Equals("skip", StringComparison.OrdinalIgnoreCase))
            //{
            //    //Do Nothing
            //}
            //else
            //{
            //    String[] options = optionlist.Split(',');
            //    DiskIOSpeedTests.Main(options);
            //}
            Console.SetOut(stdout);
            Console.WriteLine("Parser & Serializer Speed Benchmarking Tests Done");

            //Call Talis Tests
            //TalisTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Talis Platform Tests Done");

            //Call 4store Tests
            //FourStoreTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("4store Tests Done");

            //Call AllegroGraph Tests
            //AllegroGraphTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("AllegroGraph Tests Done");

            //Call Joseki Tests
            //JosekiTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Joseki Tests Done");

            //Call Hash Code Check
            //StoreHashCodeChecker.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Hash Code Check Done");

            //Background Persisted Graph Test
            //BackgroundPersistedGraphTest.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Background persisted Graph Test Done");

            //Call ConfigurationLoader Test
            //ConfigurationLoaderTests.Main(args);
            Console.SetOut(stdout);
            Console.WriteLine("Configuration Loader Tests Done");

            //Create the Figures for my 9 Month Report
            //NineMonthReportFigures.Main(args);

            //Documentation Hello World Example
            //HelloWorld.Main(args);

        }
    }
}
