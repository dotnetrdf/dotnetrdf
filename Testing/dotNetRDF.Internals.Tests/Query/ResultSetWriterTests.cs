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
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Query
{

    public class ResultSetWriterTests
    {
        [Fact]
        public void SparqlXmlWriter()
        {
                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");

                Object results = g.ExecuteQuery("SELECT * WHERE {?s ?p ?o}");
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }

                StringBuilder output = new StringBuilder();
                System.IO.StringWriter writer = new System.IO.StringWriter(output);
                SparqlXmlWriter sparqlWriter = new SparqlXmlWriter();
                sparqlWriter.Save((SparqlResultSet)results, writer);

                Console.WriteLine();
                Console.WriteLine(output.ToString());
                Console.WriteLine();

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet results2 = new SparqlResultSet();
                StringParser.ParseResultSet(results2, output.ToString());

                Assert.Equal(((SparqlResultSet)results).Count, results2.Count);
                Assert.True(((SparqlResultSet)results).Equals(results2), "Result Sets should have been equal");
        }
    }
}
