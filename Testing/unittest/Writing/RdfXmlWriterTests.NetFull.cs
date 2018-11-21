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
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    public partial class RdfXmlWriterTests
    {
        [Fact]
        public void WritingRdfXmlSimpleCollection()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred ( 1 2 3 ).";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [Fact]
        [Trait("Coverage", "Skip")]
        public void WritingRdfXmlComplex()
        {
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(new PagingHandler(new GraphHandler(g), 1000), "resources\\chado-in-owl.ttl");

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlWithDtds()
        {
            String fragment = "@prefix xsd: <" + NamespaceMapper.XMLSCHEMA + ">. @prefix : <http://example.org/>. :subj a :obj ; :has \"string\"^^xsd:string ; :has 23 .";
            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }
    }
}
