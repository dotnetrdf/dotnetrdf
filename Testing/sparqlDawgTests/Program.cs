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
