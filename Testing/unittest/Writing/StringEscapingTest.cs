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
