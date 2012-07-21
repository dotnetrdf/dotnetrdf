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
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using System.IO;

namespace dotNetRDFTest
{
    class Program
    {
        public static void reportError(String header, Exception ex)
        {
            Console.WriteLine(header);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        static void Main(string[] args)
        {
            //Going to create a Graph and assert some stuff into it
            Graph g = new Graph();

            //Try to read from a file
            TurtleParser parser = new TurtleParser();
            parser.TraceTokeniser = true;
            parser.TraceParsing = true;
            try
            {
                StreamReader input = new StreamReader("test.n3");
                parser.Load(g, input);
            }
            catch (RDFException rdfEx)
            {
                reportError("RDF Exception", rdfEx);
            }
            catch (IOException ioEx)
            {
                reportError("IO Exception", ioEx);
            }
            catch (Exception ex)
            {
               reportError("Other Exception", ex);
            }

            Console.WriteLine();
            Console.WriteLine();

            //Show Namespaces
            Console.WriteLine("All Namespaces");
            foreach (String pre in g.NamespaceMap.Prefixes)
            {
                Console.WriteLine(pre + " = " + g.NamespaceMap.GetNamespaceURI(pre));
            }

            Console.WriteLine();

            //Now print all the Statements
            Console.WriteLine("All Statements");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }


            System.Threading.Thread.Sleep(60000);
            return;



            g.NamespaceMap.AddNamespace("vds", new Uri("http://www.vdesign-studios.com/dotNetRDF#"));
            g.NamespaceMap.AddNamespace("ecs", new Uri("http://id.ecs.soton.ac.uk/person/"));
            //g.BaseURI = g.NamespaceMap.GetNamespaceURI("vds");

            URINode rav08r, wh, lac, hcd;
            rav08r = g.CreateURINode("ecs:11471");
            wh = g.CreateURINode("ecs:1650");
            hcd = g.CreateURINode("ecs:46");
            lac = g.CreateURINode("ecs:60");

            BlankNode blank = g.CreateBlankNode();
            URINode a, b, c, d, has;
            a = g.CreateURINode("vds:someRel");
            b = g.CreateURINode("vds:someOtherRel");
            c = g.CreateURINode("vds:someObj");
            d = g.CreateURINode("vds:someOtherObj");
            has = g.CreateURINode("vds:has");

            URINode supervises, collaborates, advises;
            supervises = g.CreateURINode("vds:supervises");
            collaborates = g.CreateURINode("vds:collaborates");
            advises = g.CreateURINode("vds:advises");

            LiteralNode singleLine = g.CreateLiteralNode("Some string");
            LiteralNode multiLine = g.CreateLiteralNode("This goes over\n\nseveral\n\nlines");
            LiteralNode french = g.CreateLiteralNode("Bonjour", "fr");

            g.Assert(new Triple(wh, supervises, rav08r));
            g.Assert(new Triple(lac, supervises, rav08r));
            g.Assert(new Triple(hcd, advises, rav08r));
            g.Assert(new Triple(wh, collaborates, lac));
            g.Assert(new Triple(wh, collaborates, hcd));
            g.Assert(new Triple(lac, collaborates, hcd));
            //g.Assert(new Triple(rav08r, blank, c));
            //g.Assert(new Triple(rav08r, blank, d));
            g.Assert(new Triple(rav08r, has, singleLine));
            g.Assert(new Triple(rav08r, has, multiLine));
            g.Assert(new Triple(rav08r, has, french));


            //Now print all the Statements
            Console.WriteLine("All Statements");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            //Get statements about Rob Vesse
            Console.WriteLine();
            Console.WriteLine("Statements about Rob Vesse");
            foreach (Triple t in g.GetTriples(rav08r))
            {
                Console.WriteLine(t.ToString());
            }

            //Get Statements about Collaboration
            Console.WriteLine();
            Console.WriteLine("Statements about Collaboration");
            foreach (Triple t in g.GetTriples(collaborates))
            {
                Console.WriteLine(t.ToString());
            }

            //Show Namespaces for URINodes
            Console.WriteLine();
            Console.WriteLine("Namespaces for URI Nodes");
            foreach (URINode u in g.Nodes.URINodes)
            {
                Console.WriteLine(u.Namespace + " = " + u.URI);
            }

            //Attempt to output Notation 3 for this Graph
            try
            {
                TurtleWriter n3writer = new TurtleWriter();
                n3writer.Save(g, "test.n3");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            

            System.Threading.Thread.Sleep(30000);
        }
    }
}
