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
using VDS.RDF.Update;

namespace dotNetRDFTest
{
    public class SparqlUpdateParserTests
    {
        public static void Main(string[] args)
        {
            String stdprefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\nPREFIX rdfs: <" + NamespaceMapper.RDFS + ">\nPREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n";

            List<String> testQueries = new List<string>()
            {
                "CLEAR DEFAULT",
                "CLEAR GRAPH <http://example.org>",
                "CREATE GRAPH <http://example.org>",
                "CREATE SILENT GRAPH <http://example.org>",
                "DELETE WHERE { ?s ?p ?o }",
                "DELETE { ?s ?p ?o } WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } USING <http://example.org/1> WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } USING NAMED <http://example.org/1> WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } USING <http://example.org/1> USING NAMED <http://example.org/2> WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } USING <http://example.org/1> USING NAMED <http://example.org/2> USING <http://example.org/3> WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } INSERT { ?s ?p ?o } WHERE { ?s ?p ?o }",
                "WITH <http://example.org> DELETE { ?s ?p ?o } INSERT { ?s ?p ?o } USING <http://example.org/1> USING NAMED <http://example.org/2> USING <http://example.org/3> WHERE { ?s ?p ?o }",
                "DELETE { ?s ?p ?o . GRAPH <http://example.org> { ?s a ?type } } WHERE { ?s ?p ?o }",
                "WITH <http://example.org> INSERT { ?s ?p ?o } WHERE { ?s ?p ?o }",
                "DROP GRAPH <http://example.org>",
                "DROP SILENT GRAPH <http://example.org>",
                "INSERT { ?s ?p ?o } WHERE { ?s ?p ?o }",
                "INSERT { ?s ?p ?o } WHERE { GRAPH <http://example.org> { ?s ?p ?o } }",
                "INSERT { ?s ?p ?o } USING <http://example.org/1> USING NAMED <http://example.org/2> USING <http://example.org/3> WHERE { ?s ?p ?o }",
                "LOAD <http://dbpedia.org/resource/Southampton>",
                "LOAD <http://dbpedia.org/resource/Southampton> INTO GRAPH <http://example.org>"
            };

            StreamWriter output = new StreamWriter("SparqlUpdateTests.txt");
            Console.SetOut(output);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            parser.TraceTokeniser = true;

            foreach (String testQuery in testQueries)
            {
                try
                {
                    Console.WriteLine("Trying to parse the following SPARQL Update Command");
                    Console.WriteLine(testQuery);

                    SparqlUpdateCommandSet commands = parser.ParseFromString(testQuery);

                    Console.WriteLine("Parsed OK");
                    foreach (SparqlUpdateCommand cmd in commands.Commands)
                    {
                        Console.WriteLine(cmd.GetType().ToString());
                    }
                    Console.WriteLine(commands.ToString());
                }
                catch (RdfParseException parseEx)
                {
                    reportError(output, "Parser Error", parseEx);
                }
                catch (RdfException rdfEx)
                {
                    reportError(output, "RDF Error", rdfEx);
                }
                catch (Exception ex)
                {
                    reportError(output, "Error", ex);
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            //Try and parse in bulk
            Console.WriteLine("Parsing in bulk");
            String allQueries = String.Join("\n;\n", testQueries.ToArray());
            try
            {
                SparqlUpdateCommandSet commands = parser.ParseFromString(allQueries);
                Console.WriteLine("Parsed OK");
                foreach (SparqlUpdateCommand cmd in commands.Commands)
                {
                    Console.WriteLine(cmd.GetType().ToString());
                }
                Console.WriteLine(commands.ToString());
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parser Error", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Error", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Error", ex);
            }

            output.Flush();
            output.Close();
        }

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);

            if (!(ex.InnerException == null))
            {
                output.WriteLine(ex.InnerException.Message);
                output.WriteLine(ex.InnerException.StackTrace);
            }
        }
    }
}
