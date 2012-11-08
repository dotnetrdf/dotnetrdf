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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class StringEscapingTest
    {
        private char[] _ntripleEscapes = new char[] { '\\', '"', 'n', 'r', 't' };
        private char[] _turtleEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\' };
        private char[] _n3Escapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', '\'' };
        private char[] _sparqlEscapes = new char[] { '"', 'n', 't', 'u', 'U', 'r', '\\', '0', 'f', 'b', '\'' };

        private List<String> _values = new List<string>()
        {
            "Not escapes - nrt",
            @"An example with a \ in it should get escaped but this later \\ should not",
            @"An example with a trailing \",
            @"",
            @"\",
            @"\\",
            @"An example with an explicit \n newline escape",
            @"An example with an explicit \t tab escape",
            "An example ending in a unescaped quote \"",
            "An example with explicit \\\"quote\\\" escapes",
            "An example with partial \\\"quote\" escapes",
            "An example where the \\\\\"quote is not escaped",
            @"An example with a
new line",
            @"An example with an escaped \' single quote, SPARQL/N3 formatters should permit it while others will escape it",
            @"An example with unicode escapes like \uABCD and \U1234ABCD which NTriples should escape the backslash for while others should permit it",
            @"An example with a \b backspace escape which only SPARQL permits",
            @"An example with a \f form feed escape which only SPARQL permits"
        };

        private void TestEscaping<T>(T formatter, IRdfReader parser, char[] cs) where T : INodeFormatter, ITripleFormatter
        {
            Graph g = new Graph();
            IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
            IUriNode pred2 = g.CreateUriNode(new Uri("http://example.org/predicate2"));

            foreach (String value in this._values)
            {
                //Do Escaping and Checking
                Console.WriteLine("Original Value - " + value);
                String temp = value.EscapeBackslashes(cs);
                Console.WriteLine("Backslash Escaped Value - " + temp);
                INode obj = g.CreateLiteralNode(temp);
                Console.WriteLine("Formatted Value - " + formatter.Format(obj));
                this.CheckEscapes(temp, cs);

                //Can only do round-trip checking if an RDF format
                if (parser != null)
                {
                    //Now generate a Triple, save in a String and then re-parse to check for round trip value equality
                    Triple tOrig = new Triple(subj, pred, obj);
                    System.IO.StringWriter writer = new System.IO.StringWriter();
                    writer.WriteLine(formatter.Format(tOrig));
                    Triple t2Orig = new Triple(subj, pred2, g.CreateLiteralNode(value));
                    writer.WriteLine(formatter.Format(t2Orig));

                    Console.WriteLine("Serialized Output");
                    Console.WriteLine(writer.ToString());

                    StringParser.Parse(g, writer.ToString(), parser);
                    Triple t = g.Triples.FirstOrDefault();
                    if (t == null) Assert.Fail("Failed to parse a Triple");

                    Triple t2 = g.Triples.LastOrDefault();
                    if (t2 == null) Assert.Fail("Failed to parse a Triple");
                    if (ReferenceEquals(t, t2)) Assert.Fail("Failed to generate two Triples");

                    Assert.AreEqual(t2.Object, t.Object);
                }

                Console.WriteLine();

                g.Clear();
            }
        }

        [TestMethod]
        public void WritingStringBackslashEscapingNTriples()
        {
            this.TestEscaping<NTriplesFormatter>(new NTriplesFormatter(), new NTriplesParser(), this._ntripleEscapes);
        }

        [TestMethod]
        public void WritingStringBackslashEscapingTurtle()
        {
            this.TestEscaping<TurtleFormatter>(new TurtleFormatter(), new TurtleParser(), this._turtleEscapes);
        }

        [TestMethod]
        public void WritingStringBackslashEscapingNotation3()
        {
            this.TestEscaping<Notation3Formatter>(new Notation3Formatter(), new Notation3Parser(), this._n3Escapes);
        }

        [TestMethod]
        public void WritingStringBackslashEscapingSparql()
        {
            this.TestEscaping<SparqlFormatter>(new SparqlFormatter(), null, this._sparqlEscapes);
        }

        [TestMethod]
        public void WritingStringIsFullyEscaped()
        {
            String test = "Ends with a \"";
            Console.WriteLine("Original Value - " + test);
            Assert.IsFalse(test.IsFullyEscaped(this._ntripleEscapes, new char[] { '"', '\n', '\r', '\t' }));

            NodeFactory factory = new NodeFactory();
            ILiteralNode n = factory.CreateLiteralNode(test);

            NTriplesFormatter formatter = new NTriplesFormatter();
            String result = formatter.Format(n);
            Console.WriteLine("Formatted Value - " + result);
            result = result.Substring(1, result.Length - 2);
            Console.WriteLine("Formatted Value with surrounding quotes removed - " + result);

            Assert.IsTrue(result.IsFullyEscaped(this._ntripleEscapes, new char[] { '"', '\n', '\r', '\t' }));
        }

        [TestMethod]
        public void WritingStringEscapeCharacters()
        {
            String test = "Ends with a \"";
            Console.WriteLine("Original Value - " + test);
            Assert.IsFalse(test.IsFullyEscaped(this._ntripleEscapes, new char[] { '"', '\n', '\r', '\t' }));

            String result = test.Escape('"');
            Console.WriteLine("Escaped Value - " + result);

            String test2 = "Ends with a  \"";
            Console.WriteLine("Original Value - " + test2);
            Assert.IsFalse(test2.IsFullyEscaped(this._ntripleEscapes, new char[] { '"', '\n', '\r', '\t' }));

            String result2 = test2.Escape('"');
            Console.WriteLine("Escaped Value - " + result2);
        }

        [TestMethod]
        public void WritingStringEscapeNewLines()
        {
            String test = "Has a \n new line";
            Console.WriteLine("Original Value - " + test);

            String result = test.Escape('\n', 'n');
            Console.WriteLine("Escaped Value - " + result);

            String test2 = "Has a \\n new line escape";
            Console.WriteLine("Original Value - " + test2);

            String result2 = test2.Escape('\n', 'n');
            Console.WriteLine("Escaped Value - " + result2);
        }

        public void CheckEscapes(String value, char[] cs)
        {
            if (value.Length == 0) return;
            if (value.Length == 1)
            {
                if (value[0] == '\\') Assert.Fail("Single Backslash was not escaped");
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\\')
                    {
                        if (i < value.Length - 1)
                        {
                            char next = value[i + 1];
                            if (!cs.Contains(next))
                            {
                                Assert.Fail("Backslash at Position " + i + " was not followed by a valid Escape Character");
                            }
                            i++;
                        }
                        else
                        {
                            Assert.Fail("Trailing Backslash was not escaped");
                        }
                    }
                }
            }
        }
    }
}
