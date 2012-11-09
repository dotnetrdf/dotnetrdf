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
    /// <summary>
    /// A Simple Example of building Graphs and creating some output from the Graph
    /// </summary>
    class GraphBuildingExample
    {
        public static void reportError(String header, Exception ex)
        {
            Console.WriteLine(header);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        
        public static void Main(string[] args)
        {
            //Create a new Empty Graph
            Graph g = new Graph();

            //Define Namespaces
            g.NamespaceMap.AddNamespace("vds", new Uri("http://www.vdesign-studios.com/dotNetRDF#"));
            g.NamespaceMap.AddNamespace("ecs", new Uri("http://id.ecs.soton.ac.uk/person/"));
            g.BaseUri = g.NamespaceMap.GetNamespaceUri("vds");

            //Create Uri Nodes
            IUriNode rav08r, wh, lac, hcd;
            rav08r = g.CreateUriNode("ecs:11471");
            wh = g.CreateUriNode("ecs:1650");
            hcd = g.CreateUriNode("ecs:46");
            lac = g.CreateUriNode("ecs:60");

            //Create Uri Nodes for some Predicates
            IUriNode supervises, collaborates, advises, has;
            supervises = g.CreateUriNode("vds:supervises");
            collaborates = g.CreateUriNode("vds:collaborates");
            advises = g.CreateUriNode("vds:advises");
            has = g.CreateUriNode("vds:has");

            //Create some Literal Nodes
            ILiteralNode singleLine = g.CreateLiteralNode("Some string");
            ILiteralNode multiLine = g.CreateLiteralNode("This goes over\n\nseveral\n\nlines");
            ILiteralNode french = g.CreateLiteralNode("Bonjour", "fr");
            ILiteralNode number = g.CreateLiteralNode("12", new Uri(g.NamespaceMap.GetNamespaceUri("xsd") + "integer"));

            g.Assert(new Triple(wh, supervises, rav08r));
            g.Assert(new Triple(lac, supervises, rav08r));
            g.Assert(new Triple(hcd, advises, rav08r));
            g.Assert(new Triple(wh, collaborates, lac));
            g.Assert(new Triple(wh, collaborates, hcd));
            g.Assert(new Triple(lac, collaborates, hcd));
            g.Assert(new Triple(rav08r, has, singleLine));
            g.Assert(new Triple(rav08r, has, multiLine));
            g.Assert(new Triple(rav08r, has, french));
            g.Assert(new Triple(rav08r, has, number));

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

            //Attempt to output Turtle for this Graph
            try
            {
                Console.WriteLine("Writing Turtle file graph_building_example.ttl");
                TurtleWriter ttlwriter = new TurtleWriter();
                ttlwriter.Save(g, "graph_building_example.ttl");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            //Attempt to output GraphViz
            try
            {
                Console.WriteLine("Writing GraphViz DOT file graph_building_example.dot");
                GraphVizWriter gvzwriter = new GraphVizWriter();
                gvzwriter.Save(g, "graph_building_example.dot");

                //Console.WriteLine("Attempting Live GraphViz Generation as SVG");
                //GraphVizGenerator gvzgen = new GraphVizGenerator("svg", "C:\\Program Files (x86)\\Graphviz2.20\\bin");
                //gvzgen.Generate(g, "graph_building_example.svg", true);

                //Console.WriteLine("Attempting Live GraphViz Generation as PNG");
                //gvzgen.Format = "png";
                //gvzgen.Generate(g, "graph_building_example.png", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
    }
}
