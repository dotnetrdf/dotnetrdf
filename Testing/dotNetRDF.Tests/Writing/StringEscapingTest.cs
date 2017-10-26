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
using System.Text.RegularExpressions;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{

    public class StringEscapingTest
    {
        private readonly List<String> _values = new List<string>()
        {
            "Not escapes - nrt" ,
            @"An example with a \ in it should get escaped", 
            @"An example with a trailing \", 
            @"" ,
            @"\",
            @"\\",
            "An example with an explicit \n newline escape", 
            "An example with an explicit \t tab escape",
            "An example ending in a unescaped quote \"",
            "An example with explicit \\\"quote\\\" escapes",
            "An example with partial \\\"quote\" escapes",
            "An example where the \\\\\"quote is not escaped",
            @"An example with a
new line",
            @"An example with an ' single quote",
            @"An example with unicode escapes like \uABCD and \U0034ABCD",
            @"An example with a \b backspace escape",
            @"An example with a \f form feed escape"
        };

        private void TestEscaping<T>(T formatter, IRdfReader parser) where T : INodeFormatter, ITripleFormatter
        {
            Graph g = new Graph();
            IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
            IUriNode pred2 = g.CreateUriNode(new Uri("http://example.org/predicate2"));

            foreach (String value in this._values)
            {
                //Do Escaping and Checking
                Console.WriteLine("Original Value - " + value);
                INode obj = g.CreateLiteralNode(value);
                Console.WriteLine("Formatted Value - " + formatter.Format(obj));

                //Can only do round-trip checking if an RDF format
                if (parser != null)
                {
                    //Now generate a Triple, save in a String and then re-parse to check for round trip value equality
                    Triple tOrig = new Triple(subj, pred, obj);
                    System.IO.StringWriter writer = new System.IO.StringWriter();
                    writer.WriteLine(formatter.Format(tOrig));

                    Console.WriteLine("Serialized Output");
                    Console.WriteLine(writer.ToString());

                    StringParser.Parse(g, writer.ToString(), parser);
                    Triple t = g.Triples.FirstOrDefault();
                    Assert.NotNull(t);

                    Assert.Equal(tOrig.Object, t.Object);
                }

                Console.WriteLine();

                g.Clear();
            }
        }

        [Fact]
        public void WritingStringBackslashEscapingNTriples1()
        {
            this.TestEscaping<NTriplesFormatter>(new NTriplesFormatter(NTriplesSyntax.Original), new NTriplesParser());
        }

        [Fact]
        public void WritingStringBackslashEscapingNTriples2()
        {
            this.TestEscaping<NTriplesFormatter>(new NTriplesFormatter(NTriplesSyntax.Rdf11), new NTriplesParser());
        }

        [Fact]
        public void WritingStringBackslashEscapingTurtle1()
        {
            this.TestEscaping<TurtleFormatter>(new TurtleFormatter(), new TurtleParser());
        }

        [Fact]
        public void WritingStringBackslashEscapingTurtle2()
        {
            this.TestEscaping<UncompressedTurtleFormatter>(new UncompressedTurtleFormatter(), new TurtleParser());
        }

        [Fact]
        public void WritingStringBackslashEscapingTurtle3()
        {
            this.TestEscaping<TurtleW3CFormatter>(new TurtleW3CFormatter(), new TurtleParser());
        }

        [Fact]
        public void WritingStringBackslashEscapingNotation3_1()
        {
            this.TestEscaping<Notation3Formatter>(new Notation3Formatter(), new Notation3Parser());
        }

        [Fact]
        public void WritingStringBackslashEscapingNotation3_2()
        {
            this.TestEscaping<UncompressedNotation3Formatter>(new UncompressedNotation3Formatter(), new Notation3Parser());
        }

        [Fact]
        public void WritingStringBackslashEscapingSparql()
        {
            this.TestEscaping<SparqlFormatter>(new SparqlFormatter(), null);
        }

        [Fact]
        public void WritingStringBackslashEscapingTsv()
        {
            this.TestEscaping<TsvFormatter>(new TsvFormatter(), null);
        }
    }
}
