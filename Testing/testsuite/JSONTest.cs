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

namespace dotNetRDFTest
{
    public class JsonTest
    {
        public static void Main(String[] args)
        {
            try
            {
                Console.WriteLine("Reading a RDF/XML format Test Graph");
                RdfXmlParser parser = new RdfXmlParser();
                Graph g = new Graph();
                parser.Load(g, "JSONTest.rdf");

                Console.WriteLine("Serializing back to RDF/JSON");
                RdfJsonWriter writer = new RdfJsonWriter();
                writer.Save(g, "JSONTest.rdf.json");

                Console.WriteLine("Reading back from the RDF/JSON");
                RdfJsonParser jsonparser = new RdfJsonParser();
                Graph h = new Graph();
                jsonparser.Load(h, "JSONTest.rdf.json");

                Console.WriteLine("Outputting to NTriples");
                NTriplesWriter ntwriter = new NTriplesWriter();
                ntwriter.Save(h, "JSONTest.rdf.json.out");

                Console.WriteLine("Reading a RDF/JSON format Test Graph with tons of Comments in it");
                Graph i = new Graph();
                jsonparser.Load(i, "JSONTest.json");

                Console.WriteLine("Outputting to NTriples");
                ntwriter.Save(i, "JSONTest.json.out");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
    }
}
