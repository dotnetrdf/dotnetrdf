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

