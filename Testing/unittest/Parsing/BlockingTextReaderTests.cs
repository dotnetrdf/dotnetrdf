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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class BlockingTextReaderTests
    {
        private const String TestData = "abcdefghijklmnopqrstuvwxyz0123456789";
        private const String TestData2 = "abcdefghijklmnopqrstuvwxyz\n0123456789";
        private const String TestData3 = "abcdefghijklmnopqrstuvwxyz\r0123456789";
        private const String TestData4 = "abcdefghijklmnopqrstuvwxyz\n\r0123456789";
        private const String TestData5 = "abcdefghijklmnopqrstuvwxyz\r\n0123456789";

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ParsingBlockingTextReaderBadInstantiation()
        {
            BlockingTextReader reader = ParsingTextReader.CreateBlocking((TextReader)null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParsingBlockingTextReaderBadInstantiation2()
        {
            BlockingTextReader reader = ParsingTextReader.CreateBlocking(new StringReader(String.Empty), 0);
        }

        [TestMethod]
        public void ParsingBlockingTextReaderSimpleRead()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            StringBuilder output = new StringBuilder();
            while (!reader.EndOfStream)
            {
                output.Append((char)reader.Read());
            }
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());
            reader.Close();

            Console.WriteLine(output.ToString());

            Assert.AreEqual(TestData, output.ToString());
        }

        [TestMethod]
        public void ParsingBlockingTextReaderSimpleRead2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[100];
            int read = reader.Read(cs, 0, cs.Length);
            Assert.AreEqual(TestData.Length, read);
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();

            String s = new string(cs, 0, read);
            Assert.AreEqual(TestData, s);
        }

        [TestMethod]
        public void ParsingBlockingTextReaderSimpleReadBlock()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[100];
            int read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.AreEqual(TestData.Length, read);
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();

            String s = new string(cs, 0, read);
            Assert.AreEqual(TestData, s);          
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadToEnd()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadToEnd();
            Assert.AreEqual(TestData, s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadLine()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.AreEqual(TestData, s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadLine2()
        {
            StringReader strReader = new StringReader(TestData2);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.AreEqual(TestData2.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.AreEqual(TestData2.Substring(27), s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadLine3()
        {
            StringReader strReader = new StringReader(TestData3);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.AreEqual(TestData3.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.AreEqual(TestData3.Substring(27), s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadLine4()
        {
            StringReader strReader = new StringReader(TestData4);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.AreEqual(TestData4.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.AreEqual(String.Empty, s);
            s = reader.ReadLine();
            Assert.AreEqual(TestData4.Substring(28), s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderReadLine5()
        {
            StringReader strReader = new StringReader(TestData5);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.AreEqual(TestData5.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.AreEqual(TestData5.Substring(28), s);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());

            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderSimplePeek()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            for (int i = 1; i <= 100; i++)
            {
                char c = (char)reader.Peek();
                Console.WriteLine("Peek #" + i + " = " + c);
                Assert.AreEqual(TestData[0], c);
            }
            Assert.IsFalse(reader.EndOfStream);
            Assert.AreNotEqual(-1, reader.Peek());
            Assert.AreNotEqual(-1, reader.Read());
            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderSimpleReadBlock2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.IsTrue(read <= 10);

                String s = new string(cs, 0, read);
                Assert.AreEqual(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());
            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderComplexReadBlock()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.IsTrue(read <= 10);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.AreEqual(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());
            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderComplexReadBlock2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

            char[] cs = new char[8];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.IsTrue(read <= 8);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.AreEqual(TestData.Substring(start, read), s);
                start += 8;
            }
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());
            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderComplexReadBlock3()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 2);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.IsTrue(read <= 10);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.AreEqual(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.IsTrue(reader.EndOfStream);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(-1, reader.Read());
            reader.Close();
        }

        [TestMethod]
        public void ParsingBlockingTextReaderNetworkStreamNotation3()
        {
            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                Options.UriLoaderCaching = false;
                Options.UriLoaderTimeout = 45000;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Southampton"), new Notation3Parser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                Options.UriLoaderCaching = true;
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }

        [TestMethod]
        public void ParsingBlockingTextReaderNetworkStreamNTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new NTriplesParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void ParsingBlockingTextReaderNetworkStreamTurtle()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new TurtleParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void ParsingBlockingTextReaderNetworkStreamRdfJson()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new RdfJsonParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        private void EnsureNIOData()
        {
            if (!File.Exists("nio.ttl"))
            {
                using (StreamWriter writer = new StreamWriter("nio.ttl"))
                {
                    using (StreamReader reader = new StreamReader("dataset_50.ttl.gz"))
                    {
                        String line = reader.ReadLine();
                        while (line != null)
                        {
                            writer.WriteLine(line);
                            line = reader.ReadLine();
                        }
                        reader.Close();
                    }
                    writer.Close();
                }
            }
        }

        [TestMethod]
        public void ParsingBlockingVsNonBlocking1()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Run #" + (i + 1));
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(new StreamReader("nio.ttl"));
                timer.Start();
                int totalBlocking = 0;
                int read;
                while (!blocking.EndOfStream)
                {
                    read = blocking.Read();
                    if (read >= 0) totalBlocking++;
                }
                timer.Stop();
                blocking.Close();
                blockingTime = blockingTime.Add(timer.Elapsed);

                Console.WriteLine("Blocking IO took " + timer.Elapsed + " and read " + totalBlocking + " characters");

                //Reset
                timer.Reset();
                int totalNonBlocking = 0;

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(new StreamReader("nio.ttl"));
                timer.Start();
                while (!nonBlocking.EndOfStream)
                {
                    read = nonBlocking.Read();
                    if (read >= 0) totalNonBlocking++;
                }
                timer.Stop();
                nonBlocking.Close();
                nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

                Console.WriteLine("Non-Blocking IO took " + timer.Elapsed + " and read " + totalNonBlocking + " characters");

                Assert.AreEqual(totalBlocking, totalNonBlocking);
            }

            Console.WriteLine();
            Console.WriteLine("Blocking Total Time = " + blockingTime);
            Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
        }
    }
}
