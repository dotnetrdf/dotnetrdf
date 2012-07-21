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
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class MergeTest
    {

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("MergeTest.txt");
            try
            {
                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## Merge Test Suite");

                //Load the Test RDF
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                Graph h = new Graph();
                ttlparser.Load(g, "MergePart1.ttl");
                ttlparser.Load(h, "MergePart2.ttl");

                Console.WriteLine("Merge Test Data Loaded OK");
                Console.WriteLine();

                Console.WriteLine("Graph 1 Contains");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine();
                Console.WriteLine("Graph 2 Contains");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine();

                Console.WriteLine("Attempting Graph Merge");
                g.Merge(h);
                Console.WriteLine();

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                //Use a GraphViz Generator to picture this
                Console.WriteLine();
                Console.WriteLine("Visualizing Merged Graph as SVG with GraphViz");
                GraphVizGenerator gvzgen = new GraphVizGenerator("svg");
                gvzgen.Generate(g, "MergeTest.svg", false);
                Console.WriteLine("Visualisation created as MergeTest.svg");

                //Same merge into an Empty Graph
                Console.WriteLine();
                Console.WriteLine("Combining the two Graphs with two Merge operations into an Empty Graph");
                Graph i = new Graph();

                //Need to reload g from disk
                g = new Graph();
                ttlparser.Load(g, "MergePart1.ttl");

                //Do the actual merge
                i.Merge(g);
                i.Merge(h);
                Console.WriteLine();

                foreach (Triple t in i.Triples)
                {
                    Console.WriteLine(t.ToString());
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            output.Close();
        }
    }
}

