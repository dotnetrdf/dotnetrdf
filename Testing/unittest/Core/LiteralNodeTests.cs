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
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    [TestFixture]
    public class LiteralNodeTests
    {
        [Test]
        public void NodeToLiteralCultureInvariant1()
        {
            CultureInfo sysCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                // given
                INodeFactory nodeFactory = new NodeFactory();

                // when
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl");

                // then
                Assert.AreEqual("5.5", 5.5.ToLiteral(nodeFactory).Value);
                Assert.AreEqual("7.5", 7.5f.ToLiteral(nodeFactory).Value);
                Assert.AreEqual("15.5", 15.5m.ToLiteral(nodeFactory).Value);

                // when
                CultureInfo culture = Thread.CurrentThread.CurrentCulture;
                // Make a writable clone
                culture = (CultureInfo)culture.Clone();
                culture.NumberFormat.NegativeSign = "!";
                Thread.CurrentThread.CurrentCulture = culture;

                // then
                Assert.AreEqual("-1", (-1).ToLiteral(nodeFactory).Value);
                Assert.AreEqual("-1", ((short)-1).ToLiteral(nodeFactory).Value);
                Assert.AreEqual("-1", ((long)-1).ToLiteral(nodeFactory).Value);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = sysCulture;
            }
        }

        [Test]
        public void NodeToLiteralCultureInvariant2()
        {
            CultureInfo sysCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                INodeFactory factory = new NodeFactory();

                CultureInfo culture = Thread.CurrentThread.CurrentCulture;
                culture = (CultureInfo)culture.Clone();
                culture.NumberFormat.NegativeSign = "!";
                Thread.CurrentThread.CurrentCulture = culture;

                TurtleFormatter formatter = new TurtleFormatter();
                String fmtStr = formatter.Format((-1).ToLiteral(factory));
                Assert.AreEqual("-1 ", fmtStr);
                fmtStr = formatter.Format((-1.2m).ToLiteral(factory));
                Assert.AreEqual("-1.2", fmtStr);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = sysCulture;
            }
        }

        [Test]
        public void NodeToLiteralDateTimePrecision1()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            NodeFactory factory = new NodeFactory();
            ILiteralNode litNow = now.ToLiteral(factory);

            //Print out
            Console.WriteLine("Original: " + now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Node Form: " + formatter.Format(litNow));

            //Extract and check it round tripped
            DateTimeOffset now2 = litNow.AsValuedNode().AsDateTime();
            Console.WriteLine("Extracted: " + now2.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));

            TimeSpan diff = now - now2;
            Console.WriteLine("Difference: " + diff.ToString());
            Assert.IsTrue(diff < new TimeSpan(10), "Loss of precision should be at most 1 micro-second");
        }

        [Test]
        public void NodeToLiteralDateTimePrecision2()
        {
            DateTime now = DateTime.Now;
            NodeFactory factory = new NodeFactory();
            ILiteralNode litNow = now.ToLiteral(factory);

            //Print out
            Console.WriteLine("Original: " + now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Node Form: " + formatter.Format(litNow));

            //Extract and check it round tripped
            DateTimeOffset now2 = litNow.AsValuedNode().AsDateTime();
            Console.WriteLine("Extracted: " + now2.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));

            TimeSpan diff = now - now2;
            Console.WriteLine("Difference: " + diff.ToString());
            Assert.IsTrue(diff < new TimeSpan(10), "Loss of precision should be at most 1 micro-second");
        }

        [Test]
        public void NodeToLiteralDateTimePrecision3()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            NodeFactory factory = new NodeFactory();
            ILiteralNode litNow = now.ToLiteral(factory, false);

            //Print out
            Console.WriteLine("Original: " + now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Node Form: " + formatter.Format(litNow));

            //Extract and check it round tripped
            DateTimeOffset now2 = litNow.AsValuedNode().AsDateTime();
            Console.WriteLine("Extracted: " + now2.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));

            TimeSpan diff = now - now2;
            Console.WriteLine("Difference: " + diff.ToString());
            Assert.IsTrue(diff < new TimeSpan(0,0,1), "Loss of precision should be at most 1 second");
        }

        [Test]
        public void NodeToLiteralDateTimePrecision4()
        {
            DateTime now = DateTime.Now;
            NodeFactory factory = new NodeFactory();
            ILiteralNode litNow = now.ToLiteral(factory, false);

            //Print out
            Console.WriteLine("Original: " + now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Node Form: " + formatter.Format(litNow));

            //Extract and check it round tripped
            DateTimeOffset now2 = litNow.AsValuedNode().AsDateTime();
            Console.WriteLine("Extracted: " + now2.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));

            TimeSpan diff = now - now2;
            Console.WriteLine("Difference: " + diff.ToString());
            Assert.IsTrue(diff < new TimeSpan(0,0,1), "Loss of precision should be at most 1 second");
        }

        [Test]
        public void NodeLiteralLanguageSpecifierCase1()
        {
            NodeFactory factory = new NodeFactory();
            ILiteralNode lcase = factory.CreateLiteralNode("example", "en-gb");
            ILiteralNode ucase = factory.CreateLiteralNode("example", "en-GB");

            Assert.IsTrue(EqualityHelper.AreLiteralsEqual(lcase, ucase));
        }

        [Test]
        public void NodeLiteralLanguageSpecifierCase2()
        {
            NodeFactory factory = new NodeFactory();
            ILiteralNode lcase = factory.CreateLiteralNode("example", "en-gb");
            ILiteralNode ucase = factory.CreateLiteralNode("example", "en-GB");

            Assert.AreEqual(0, ComparisonHelper.CompareLiterals(lcase, ucase));
        }

        [Test]
        public void NodeLiteralLanguageSpecifierCase3()
        {
            IGraph g = new Graph();
            ILiteralNode lcase = g.CreateLiteralNode("example", "en-gb");
            ILiteralNode ucase = g.CreateLiteralNode("example", "en-GB");
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(s, p, lcase);
            g.Assert(s, p, ucase);

            Assert.AreEqual(2, g.GetTriplesWithObject(lcase).Count(), "Lower case search failed to find both solutions");
            Assert.AreEqual(2, g.GetTriplesWithObject(ucase).Count(), "Upper case search failed to find both solutions");
        }
    }
}