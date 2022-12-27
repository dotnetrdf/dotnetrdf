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
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class VirtuosoTest
    {
        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("VirtuosoTest.txt");
            Console.SetOut(output);
            try
            {
                Console.WriteLine("##Virtuoso Test Suite");
                Console.WriteLine();

                //Do some basic operations
                Console.WriteLine("# Basic Read and Write of normal Graphs");

                //Read in a Test Graph from a Turtle File
                Graph g = new Graph();
                g.BaseURI = new Uri("http://www.dotnetrdf.org/Tests/SQLStore/");
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");

                Console.WriteLine("Loaded the InferenceTest.ttl file as the Test Graph");
                Console.WriteLine("Attempting to save into the SQL Store");

                //Get the Non Native Virtuoso Manager
                NonNativeVirtuosoManager manager = new NonNativeVirtuosoManager("localhost", 1111, "dotnetrdf_experimental", "dba", "20sQl09");

                //Save to a Store using SqlWriter
                SqlWriter sqlwriter = new SqlWriter(manager);
                sqlwriter.Save(g, false);

                Console.WriteLine("Saved to the SQL Store");

                //Read back from the Store using SqlReader
                Graph h = new Graph();
                Console.WriteLine("Trying to read the Graph back from the SQL Store");
                SqlReader sqlreader = new SqlReader(manager);
                h = sqlreader.Load("http://www.dotnetrdf.org/Tests/SQLStore/");

                Console.WriteLine("Read from SQL Store OK");

                foreach (String prefix in h.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + h.NamespaceMap.GetNamespaceURI(prefix).ToString() + ">");
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

                SqlGraph s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/SQLStore"), manager);
                Console.WriteLine("Opened the SQL Graph OK");

                INode type = s.CreateURINode("rdf:type");

                s.Assert(new Triple(type, type, type));

                Console.WriteLine("Asserted something");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/"));
                Console.WriteLine("Added a Namespace");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/changedNamespace/"));
                Console.WriteLine("Changed a Namespace");

                Console.WriteLine("Reopening to see if stuff gets loaded correctly");
                s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/SQLStore"), "dotnetrdf_experimental", "sa", "20sQl08");

                foreach (String prefix in s.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + s.NamespaceMap.GetNamespaceURI(prefix).ToString() + ">");
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
            }
            catch (OpenLink.Data.Virtuoso.VirtuosoException virtEx)
            {
                reportError(output, "Virtuoso Exception", virtEx);
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                reportError(output, "SQL Exception", sqlEx);
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RDFParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RDFException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }
            finally
            {
                output.Close();
            }
        }

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }
    }
}
