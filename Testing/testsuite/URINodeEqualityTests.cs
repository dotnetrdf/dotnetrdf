using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace dotNetRDFTest
{
    public class UriNodeEqualityTests
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("## URI Node Equality Code Tests");
            Console.WriteLine("Tests that URI Node Equality behaves as expected");
            Console.WriteLine();

            //Create the Nodes
            Graph g = new Graph();
            Console.WriteLine("Creating two URIs referring to google - one lowercase, one uppercase - which should be equivalent");
            UriNode a = g.CreateUriNode(new Uri("http://www.google.com"));
            UriNode b = g.CreateUriNode(new Uri("http://www.GOOGLE.com/"));

            CompareNodes(a, b);

            Console.WriteLine("Creating two URIs with the same Fragment ID but differing in case and thus are different since Fragment IDs are case sensitive");
            UriNode c = g.CreateUriNode(new Uri("http://www.google.com/#Test"));
            UriNode d = g.CreateUriNode(new Uri("http://www.GOOGLE.com/#test"));

            CompareNodes(c, d);

            Console.WriteLine("Creating two identical URIs with unusual characters in them");
            UriNode e = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));
            UriNode f = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));

            CompareNodes(e, f);

            Console.WriteLine("Creating two URIs with similar paths that differ in case");
            UriNode h = g.CreateUriNode(new Uri("http://www.google.com/path/test/case"));
            UriNode i = g.CreateUriNode(new Uri("http://www.google.com/path/Test/case"));

            CompareNodes(h, i);

            Console.WriteLine("Creating three URIs with equivalent relative paths");
            UriNode j = g.CreateUriNode(new Uri("http://www.google.com/relative/test/../example.html"));
            UriNode k = g.CreateUriNode(new Uri("http://www.google.com/relative/test/monkey/../../example.html"));
            UriNode l = g.CreateUriNode(new Uri("http://www.google.com/relative/./example.html"));

            CompareNodes(j, k);
            CompareNodes(k, l);
        }

        private static void CompareNodes(UriNode a, UriNode b)
        {
            Console.WriteLine("URI Node A has String form: " + a.ToString());
            Console.WriteLine("URI Node B has String form: " + b.ToString());
            Console.WriteLine();
            Console.WriteLine("URI Node A has Hash Code: " + a.GetHashCode());
            Console.WriteLine("URI Node B has Hash Code: " + b.GetHashCode());
            Console.WriteLine();
            Console.WriteLine("Nodes are Equal? " + a.Equals(b));
            Console.WriteLine("Hash Codes are Equal? " + a.GetHashCode().Equals(b.GetHashCode()));
            Console.WriteLine();
        }
    }
}
