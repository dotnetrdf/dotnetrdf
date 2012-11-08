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
            IUriNode a = g.CreateUriNode(new Uri("http://www.google.com"));
            IUriNode b = g.CreateUriNode(new Uri("http://www.GOOGLE.com/"));

            CompareNodes(a, b);

            Console.WriteLine("Creating two URIs with the same Fragment ID but differing in case and thus are different since Fragment IDs are case sensitive");
            IUriNode c = g.CreateUriNode(new Uri("http://www.google.com/#Test"));
            IUriNode d = g.CreateUriNode(new Uri("http://www.GOOGLE.com/#test"));

            CompareNodes(c, d);

            Console.WriteLine("Creating two identical URIs with unusual characters in them");
            IUriNode e = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));
            IUriNode f = g.CreateUriNode(new Uri("http://www.google.com/random,_@characters"));

            CompareNodes(e, f);

            Console.WriteLine("Creating two URIs with similar paths that differ in case");
            IUriNode h = g.CreateUriNode(new Uri("http://www.google.com/path/test/case"));
            IUriNode i = g.CreateUriNode(new Uri("http://www.google.com/path/Test/case"));

            CompareNodes(h, i);

            Console.WriteLine("Creating three URIs with equivalent relative paths");
            IUriNode j = g.CreateUriNode(new Uri("http://www.google.com/relative/test/../example.html"));
            IUriNode k = g.CreateUriNode(new Uri("http://www.google.com/relative/test/monkey/../../example.html"));
            IUriNode l = g.CreateUriNode(new Uri("http://www.google.com/relative/./example.html"));

            CompareNodes(j, k);
            CompareNodes(k, l);
        }

        private static void CompareNodes(IUriNode a, IUriNode b)
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
