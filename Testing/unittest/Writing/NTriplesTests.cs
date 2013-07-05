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
        [Test]
        public void WritingNTriplesAsciiChars()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= 127; i++)
            {
                builder.Append((char)i);
            }

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(builder.ToString()));

            NTriplesWriter writer = new NTriplesWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            NTriplesParser parser = new NTriplesParser();
            IGraph h = new Graph();
            parser.Load(h, new StringReader(strWriter.ToString()));

            Assert.AreEqual(g, h);
        }

        [Test]
        public void WritingNTriplesNonAsciiChars()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 128; i <= 255; i++)
            {
                builder.Append((char) i);
            }

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(builder.ToString()));

            NTriplesWriter writer = new NTriplesWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            NTriplesParser parser = new NTriplesParser();
            IGraph h = new Graph();
            parser.Load(h, new StringReader(strWriter.ToString()));

            Assert.AreEqual(g, h);
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

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(builder.ToString()));

            NTriplesWriter writer = new NTriplesWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            NTriplesParser parser = new NTriplesParser();
            IGraph h = new Graph();
            parser.Load(h, new StringReader(strWriter.ToString()));

            Assert.AreEqual(g, h);
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

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(builder.ToString()));

            NTriplesWriter writer = new NTriplesWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            NTriplesParser parser = new NTriplesParser();
            IGraph h = new Graph();
            parser.Load(h, new StringReader(strWriter.ToString()));

            Assert.AreEqual(g, h);
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
