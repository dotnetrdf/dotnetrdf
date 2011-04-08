using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class MySqlStoreTest
    {
        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("MySQLStoreTest.txt");
            Console.SetOut(output);
            Console.WriteLine("## MySQL Store Test");

            try
            {

                #region Basic Tests

                //Do some basic operations
                Console.WriteLine("# Basic Read and Write of normal Graphs");

                //Read in a Test Graph from a Turtle File
                Graph g = new Graph();
                g.BaseUri = new Uri("http://www.dotnetrdf.org/Tests/MySQLStore/");
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");

                Console.WriteLine("Loaded the InferenceTest.ttl file as the Test Graph");
                Console.WriteLine("Attempting to save into the MySQL Store");

                //Save to a Store using SqlWriter
                SqlWriter sqlwriter = new SqlWriter(new MySqlStoreManager("localhost","dotnetrdf_test", "root", "20sQl08"));
                sqlwriter.Save(g, false);

                Console.WriteLine("Saved to the MySQL Store");

                //Read back from the Store using SqlReader
                IGraph h = new Graph();
                Console.WriteLine("Trying to read the Graph back from the SQL Store");
                SqlReader sqlreader = new SqlReader(new MySqlStoreManager("localhost","dotnetrdf_test", "root", "20sQl08"));
                h = sqlreader.Load("http://www.dotnetrdf.org/Tests/MySQLStore/");

                Console.WriteLine("Read from MySQL Store OK");

                foreach (String prefix in h.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + h.NamespaceMap.GetNamespaceUri(prefix).ToString() + ">");
                }
                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine("# Test Passed");
                Console.WriteLine();

                //Demonstrate that the SqlGraph persists stuff to the Store
                Console.WriteLine("# Advanced Read and Write with a SQLGraph");

                SqlGraph s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/SQLStore"), new MySqlStoreManager("localhost","dotnetrdf_test", "root", "20sQl08"));
                Console.WriteLine("Opened the MySQL Graph OK");

                INode type = s.CreateUriNode("rdf:type");

                s.Assert(new Triple(type, type, type));

                Console.WriteLine("Asserted something");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/"));
                Console.WriteLine("Added a Namespace");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/changedNamespace/"));
                Console.WriteLine("Changed a Namespace");

                Console.WriteLine("Reopening to see if stuff gets loaded correctly");
                s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/MySQLStore"), new MySqlStoreManager("localhost","dotnetrdf_test", "root", "20sQl08"));

                foreach (String prefix in s.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + s.NamespaceMap.GetNamespaceUri(prefix).ToString() + ">");
                }
                Console.WriteLine();
                foreach (Triple t in s.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                s.Retract(new Triple(type, type, type));
                Console.WriteLine("Retracted something");

                foreach (Triple t in s.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                #endregion

                Console.WriteLine("# Tests Passed");
            }
            catch (MySql.Data.MySqlClient.MySqlException sqlEx)
            {
                reportError(output, "MySQL Exception", sqlEx);
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            output.Close();
        }
    }
}
