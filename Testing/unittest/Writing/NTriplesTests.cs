using System;
using System.Diagnostics;
using System.IO;
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
        public void WritingNTriplesAscii()
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
        public void WritingNTriplesNonAscii()
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
        public void WritingNTriplesVsTurtleNonAsciiSpeed()
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
