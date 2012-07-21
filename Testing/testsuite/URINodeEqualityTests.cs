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
