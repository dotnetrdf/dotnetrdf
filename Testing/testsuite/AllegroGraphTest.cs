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
using System.Net;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class AllegroGraphTest
    {
        public static void Main(String[] args)
        {
            StreamWriter output  = new StreamWriter("AllegroGraphTest.txt");
            Console.SetOut(output);
            try
            {
                Console.WriteLine("## AllegroGraph Test");
                Console.WriteLine();

                //Load the Graph we want to use as a Test
                Graph g = new Graph();
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");
                Console.WriteLine("Test Graph contains the following Triples:");
                ShowGraph(g);
                Console.WriteLine();

                //Load another Graph
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/test");
                Notation3Parser n3parser = new Notation3Parser();
                n3parser.Load(h, "test.n3");
                Console.WriteLine("Second Test Graph contains the following Triples:");
                ShowGraph(h);
                Console.WriteLine();

                Console.WriteLine("Trying to create a test store in the test catalog");
                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875/", "test", "test");
                Console.WriteLine("Store Created OK");
                Console.WriteLine();

                Console.WriteLine("Trying to add data to the Store");
                agraph.SaveGraph(g);
                agraph.SaveGraph(h);
                Console.WriteLine("Saved OK");
                Console.WriteLine();

                Console.WriteLine("Trying to load data from the Store");
                Graph i = new Graph();
                agraph.LoadGraph(i, String.Empty);
                ShowGraph(i);
                Console.WriteLine();
                i = new Graph();
                agraph.LoadGraph(i, new Uri("http://example.org/test"));
                ShowGraph(i);
                Console.WriteLine("Loaded OK");
                Console.WriteLine();

                Console.WriteLine("Trying to update data in the Store");
                List<Triple> toRemove = g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")).ToList();
                Triple toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/")), g.CreateUriNode("rdf:type"), g.CreateLiteralNode("Added Triple Test"));
                agraph.UpdateGraph(String.Empty, new List<Triple>() { toAdd }, toRemove);
                Console.WriteLine("Updated OK");
                Console.WriteLine();

                Console.WriteLine("Trying a SPARQL ASK query against the store");
                Object results = agraph.Query("ASK WHERE {?s ?p ?o}");
                Console.WriteLine("Got results OK");
                ShowResults(results);
                Console.WriteLine();

                Console.WriteLine("Trying a SPARQL SELECT query against the store");
                results = agraph.Query("SELECT * WHERE {?s ?p ?o}");
                Console.WriteLine("Got results OK");
                ShowResults(results);
                Console.WriteLine();

            }
            catch (RdfStorageException storeEx)
            {
                reportError(output, "RDF Storage Error", storeEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "RDF Parsing Error", parseEx);
            }
            catch (RdfQueryException queryEx)
            {
                reportError(output, "RDF Query Error", queryEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Error", rdfEx);
            }
            catch (WebException webEx)
            {
                reportError(output, "HTTP Error", webEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Error", ex);
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

            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                output.WriteLine();
                output.WriteLine(innerEx.Message);
                output.WriteLine(innerEx.StackTrace);
                innerEx = innerEx.InnerException;
            }
        }


        public static void ShowResults(Object results)
        {
            if (results is Graph)
            {
                ShowGraph((Graph)results);
            }
            else if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                Console.WriteLine("Result: " + resultSet.Result);
                Console.WriteLine(resultSet.Results.Count + " Results");
                foreach (SparqlResult r in resultSet.Results)
                {
                    Console.WriteLine(r.ToString());
                }
            }
            else
            {
                throw new ArgumentException("Expected a Graph or a SPARQLResultSet");
            }
        }

        public static void ShowGraph(IGraph g)
        {
            Console.Write("Graph URI: ");
            if (g.BaseUri != null)
            {
                Console.WriteLine(g.BaseUri.ToString());
            }
            else
            {
                Console.WriteLine("NULL");
            }
            Console.WriteLine(g.Triples.Count + " Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
