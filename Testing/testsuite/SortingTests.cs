using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace dotNetRDFTest
{
    public class SortingTests
    {

        public static void Main(string[] args)
        {
            //Stream for Output
            StreamWriter output = new StreamWriter("SortingTests.txt");
            Console.SetOut(output);
            Console.WriteLine("## Sorting Test");
            Console.WriteLine("NULLs < Blank Nodes < URI Nodes < Untyped Literals < Typed Literals");
            Console.WriteLine();

            //Create a Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");
            g.NamespaceMap.AddNamespace("",new Uri("http://example.org/"));

            //Create a list of various Nodes
            List<INode> nodes = new List<INode>();
            nodes.Add(g.CreateUriNode(":someUri"));
            nodes.Add(g.CreateBlankNode());
            nodes.Add(null);
            nodes.Add(g.CreateBlankNode());
            nodes.Add(g.CreateLiteralNode("cheese"));
            nodes.Add(g.CreateLiteralNode("aardvark"));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.AddDays(-25).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("duck"));
            nodes.Add(g.CreateUriNode(":otherUri"));
            nodes.Add(g.CreateLiteralNode("1.5",new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateUriNode(new Uri("http://www.google.com")));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.AddYears(3).ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("23",new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode("M43d", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateUriNode(new Uri("http://www.dotnetrdf.org")));
            nodes.Add(g.CreateLiteralNode("12",new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateBlankNode("monkey"));
            nodes.Add(g.CreateBlankNode());
            nodes.Add(g.CreateLiteralNode("chaese"));
            nodes.Add(g.CreateLiteralNode("1.0456345",new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateLiteralNode("cheese"));
            nodes.Add(g.CreateLiteralNode(Convert.ToBase64String(new byte[] { Byte.Parse("32") }), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateLiteralNode("TA==", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            nodes.Add(g.CreateLiteralNode("-45454", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode(DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)));
            nodes.Add(g.CreateLiteralNode("-3",new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            nodes.Add(g.CreateLiteralNode("242344.3456435",new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
            nodes.Add(g.CreateLiteralNode("true",new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
            nodes.Add(g.CreateUriNode(":what"));
            nodes.Add(null);
            nodes.Add(g.CreateLiteralNode("false",new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));

            for (int i = 0; i < 32; i++)
            {
                nodes.Add(g.CreateLiteralNode(i.ToString("x"),new Uri(XmlSpecsHelper.XmlSchemaDataTypeHexBinary)));
            }

            for (byte b = 50; b < 77; b++)
            {
                nodes.Add(g.CreateLiteralNode(Convert.ToBase64String(new byte[] { b }), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
            }

            nodes.Sort();

            //Output the Results
            foreach (INode n in nodes) {
                if (n == null) {
                    Console.WriteLine("NULL");
                } else {
                    Console.WriteLine(n.ToString());
                }
            }

            Console.WriteLine();
            Console.WriteLine("Now in reverse...");
            Console.WriteLine();

            nodes.Reverse();

            //Output the Results
            foreach (INode n in nodes)
            {
                if (n == null)
                {
                    Console.WriteLine("NULL");
                }
                else
                {
                    Console.WriteLine(n.ToString());
                }
            }

            output.Close();
        }
    }
}
