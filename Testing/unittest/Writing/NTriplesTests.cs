/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    [TestFixture]
    public class NTriplesTests
    {
        private void Test(String literal, IRdfWriter writer, IRdfReader parser)
        {
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(literal));

            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            IGraph h = new Graph();
            parser.Load(h, new StringReader(strWriter.ToString()));

            Assert.AreEqual(g, h);
        }

        [Test]
        public void WritingNTriplesAsciiChars1()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= 127; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test]
        public void WritingNTriplesAsciiChars2()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= 127; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test]
        public void WritingNTriplesAsciiChars3()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= 127; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesAsciiChars4()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= 127; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesNonAsciiChars1()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char) i);
            }

           this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test, ExpectedException(typeof(RdfParseException))]
        public void WritingNTriplesNonAsciiChars2()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test]
        public void WritingNTriplesNonAsciiChars3()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesNonAsciiChars4()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char)i);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesMixedChars1()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
            {
                builder.Append((char) i);
                builder.Append((char) j);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test]
        public void WritingNTriplesMixedChars2()
        {
            StringBuilder builder = new StringBuilder();
            Queue<char> ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char) c));
            Queue<char> nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char) c));

            int i = 1;
            while (ascii.Count > 0)
            {
                for (int x = 0; x < i && ascii.Count > 0; x++)
                {
                    builder.Append(ascii.Dequeue());
                }
                for (int x = 0; x < i && nonAscii.Count > 0; x++)
                {
                    builder.Append(nonAscii.Dequeue());
                }
                i++;
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test, ExpectedException(typeof(RdfParseException))]
        public void WritingNTriplesMixedChars3()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
            {
                builder.Append((char)i);
                builder.Append((char)j);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test, ExpectedException(typeof(RdfParseException))]
        public void WritingNTriplesMixedChars4()
        {
            StringBuilder builder = new StringBuilder();
            Queue<char> ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
            Queue<char> nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

            int i = 1;
            while (ascii.Count > 0)
            {
                for (int x = 0; x < i && ascii.Count > 0; x++)
                {
                    builder.Append(ascii.Dequeue());
                }
                for (int x = 0; x < i && nonAscii.Count > 0; x++)
                {
                    builder.Append(nonAscii.Dequeue());
                }
                i++;
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original));
        }

        [Test]
        public void WritingNTriplesMixedChars5()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
            {
                builder.Append((char)i);
                builder.Append((char)j);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesMixedChars6()
        {
            StringBuilder builder = new StringBuilder();
            Queue<char> ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
            Queue<char> nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

            int i = 1;
            while (ascii.Count > 0)
            {
                for (int x = 0; x < i && ascii.Count > 0; x++)
                {
                    builder.Append(ascii.Dequeue());
                }
                for (int x = 0; x < i && nonAscii.Count > 0; x++)
                {
                    builder.Append(nonAscii.Dequeue());
                }
                i++;
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesMixedChars7()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
            {
                builder.Append((char)i);
                builder.Append((char)j);
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesMixedChars8()
        {
            StringBuilder builder = new StringBuilder();
            Queue<char> ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
            Queue<char> nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

            int i = 1;
            while (ascii.Count > 0)
            {
                for (int x = 0; x < i && ascii.Count > 0; x++)
                {
                    builder.Append(ascii.Dequeue());
                }
                for (int x = 0; x < i && nonAscii.Count > 0; x++)
                {
                    builder.Append(nonAscii.Dequeue());
                }
                i++;
            }

            this.Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
        }

        [Test]
        public void WritingNTriplesVsTurtleNonAsciiCharsSpeed()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char)i);
            }
            INode n = new NodeFactory().CreateLiteralNode(builder.ToString());

            // Write 10,000 times with NTriplesFormatter
            Stopwatch timer = new Stopwatch();
            INodeFormatter formatter = new NTriplesFormatter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            timer.Start();
            strWriter.WriteLine(formatter.Format(n));
            timer.Stop();

            Console.WriteLine("NTriples Formatter Time Elapsed: " + timer.Elapsed);
            TimeSpan ntriplesTime = timer.Elapsed;
            timer.Reset();

            // Write 10,000 times with NTriplesFormatter in RDF 1.1 mode
            formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
            strWriter = new System.IO.StringWriter();
            timer.Start();
            strWriter.WriteLine(formatter.Format(n));
            timer.Stop();

            Console.WriteLine("NTriples Formatter (RDF 1.1) Time Elapsed: " + timer.Elapsed);
            //TimeSpan ntriples11Time = timer.Elapsed;
            timer.Reset();

            // Write and check round trip with Turtle and compare speed
            strWriter = new System.IO.StringWriter();
            formatter = new UncompressedTurtleFormatter();
            timer.Start();
            strWriter.WriteLine(formatter.Format(n));
            timer.Stop();

            Console.WriteLine("Turtle Write Time Elapsed: " + timer.Elapsed);
            TimeSpan turtleTime = timer.Elapsed;

            // Compare Speed
            Assert.IsTrue(turtleTime.CompareTo(ntriplesTime) == -1);
        }
    }
}
