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
