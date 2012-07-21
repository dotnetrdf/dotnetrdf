/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
