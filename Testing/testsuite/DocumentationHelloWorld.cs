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
using VDS.RDF;
using VDS.RDF.Writing;

namespace dotNetRDFTest 
{

public class HelloWorld 
{

    public static void Main(String[] args) 
    {
	    //Fill in the code shown on this page here to build your hello world application
        Graph g = new Graph();

        IUriNode dotNetRDF = g.CreateUriNode(new Uri("http://www.dotnetrdf.org"));
        IUriNode says = g.CreateUriNode(new Uri("http://example.org/says"));
        ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
        ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

        g.Assert(new Triple(dotNetRDF, says, helloWorld));
        g.Assert(new Triple(dotNetRDF, says, bonjourMonde));

        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString());
        }

        NTriplesWriter ntwriter = new NTriplesWriter();
        ntwriter.Save(g, "HelloWorld.nt");

        RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
        rdfxmlwriter.Save(g, "HelloWorld.rdf");

    }
}
}

