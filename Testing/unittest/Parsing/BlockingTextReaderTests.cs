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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Parsing
{
    public partial class BlockingTextReaderTests
    {
        private const String TestData = "abcdefghijklmnopqrstuvwxyz0123456789";
        private const String TestData2 = "abcdefghijklmnopqrstuvwxyz\n0123456789";
        private const String TestData3 = "abcdefghijklmnopqrstuvwxyz\r0123456789";
        private const String TestData4 = "abcdefghijklmnopqrstuvwxyz\n\r0123456789";
        private const String TestData5 = "abcdefghijklmnopqrstuvwxyz\r\n0123456789";

        [Fact]
        public void ParsingTextReaderCreation1()
        {
            File.WriteAllText("ParsingTextReaderCreation1.txt", "ParsingTextReaderCreation1");
            using (StreamReader stream = File.OpenText("ParsingTextReaderCreation1.txt"))
            {
                ParsingTextReader reader = ParsingTextReader.Create(stream);
#if PORTABLE
                // Portable class library uses blocking IO for file streams
                Assert.IsType<BlockingTextReader>(reader);
#else
                Assert.IsType<NonBlockingTextReader>(reader);
#endif
                stream.Close();
            }
        }

        [Fact]
        public void ParsingTextReaderCreation2()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ParsingTextReader reader = ParsingTextReader.Create(stream);
                Assert.IsType<NonBlockingTextReader>(reader);
                stream.Close();
            }
        }

        [Fact]
        public void ParsingTextReaderCreation4()
        {
            try
            {
                Options.ForceBlockingIO = true;

                File.WriteAllText("ParsingTextReaderCreation4.txt", "ParsingTextReaderCreation1");
                using (StreamReader stream = File.OpenText("ParsingTextReaderCreation4.txt"))
                {
                    ParsingTextReader reader = ParsingTextReader.Create(stream);
                    Assert.IsType<BlockingTextReader>(reader);
                    stream.Close();
                }
            }
            finally
            {
                Options.ForceBlockingIO = false;
            }
        }

        [Fact]
        public void ParsingTextReaderCreation5()
        {
            try
            {
                Options.ForceBlockingIO = true;

                using (MemoryStream stream = new MemoryStream())
                {
                    ParsingTextReader reader = ParsingTextReader.Create(stream);
                    Assert.IsType<BlockingTextReader>(reader);
                    stream.Close();
                }
            }
            finally
            {
                Options.ForceBlockingIO = false;
            }
        }

        [Fact]
        public void ParsingTextReaderBlockingBadInstantiation()
        {
            Assert.Throws<ArgumentNullException>(() => ParsingTextReader.CreateBlocking((TextReader)null));
        }

        [Fact]
        public void ParsingTextReaderBlockingBadInstantiation2()
        {
            Assert.Throws<ArgumentException>(() => ParsingTextReader.CreateBlocking(new StringReader(String.Empty), 0));
        }

        [Fact]
        public void ParsingTextReaderBlockingSimpleRead()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            StringBuilder output = new StringBuilder();
            while (!reader.EndOfStream)
            {
                output.Append((char)reader.Read());
            }
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());
            reader.Close();

            Console.WriteLine(output.ToString());

            Assert.Equal(TestData, output.ToString());
        }

        [Fact]
        public void ParsingTextReaderBlockingSimpleRead2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[100];
            int read = reader.Read(cs, 0, cs.Length);
            Assert.Equal(TestData.Length, read);
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();

            String s = new string(cs, 0, read);
            Assert.Equal(TestData, s);
        }

        [Fact]
        public void ParsingTextReaderBlockingSimpleReadBlock()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[100];
            int read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.Equal(TestData.Length, read);
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();

            String s = new string(cs, 0, read);
            Assert.Equal(TestData, s);          
        }

        [Fact]
        public void ParsingTextReaderBlockingReadToEnd()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadToEnd();
            Assert.Equal(TestData, s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingReadLine()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.Equal(TestData, s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingReadLine2()
        {
            StringReader strReader = new StringReader(TestData2);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.Equal(TestData2.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.Equal(TestData2.Substring(27), s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingReadLine3()
        {
            StringReader strReader = new StringReader(TestData3);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.Equal(TestData3.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.Equal(TestData3.Substring(27), s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingReadLine4()
        {
            StringReader strReader = new StringReader(TestData4);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.Equal(TestData4.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.Equal(String.Empty, s);
            s = reader.ReadLine();
            Assert.Equal(TestData4.Substring(28), s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingReadLine5()
        {
            StringReader strReader = new StringReader(TestData5);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            String s = reader.ReadLine();
            Assert.Equal(TestData5.Substring(0, 26), s);
            s = reader.ReadLine();
            Assert.Equal(TestData5.Substring(28), s);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());

            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingSimplePeek()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            for (int i = 1; i <= 100; i++)
            {
                char c = (char)reader.Peek();
                Console.WriteLine("Peek #" + i + " = " + c);
                Assert.Equal(TestData[0], c);
            }
            Assert.False(reader.EndOfStream);
            Assert.NotEqual(-1, reader.Peek());
            Assert.NotEqual(-1, reader.Read());
            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingSimpleReadBlock2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.True(read <= 10);

                String s = new string(cs, 0, read);
                Assert.Equal(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());
            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingComplexReadBlock()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.True(read <= 10);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.Equal(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());
            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingComplexReadBlock2()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

            char[] cs = new char[8];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.True(read <= 8);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.Equal(TestData.Substring(start, read), s);
                start += 8;
            }
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());
            reader.Close();
        }

        [Fact]
        public void ParsingTextReaderBlockingComplexReadBlock3()
        {
            StringReader strReader = new StringReader(TestData);

            BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 2);

            char[] cs = new char[10];
            int start = 0;
            while (!reader.EndOfStream)
            {
                int read = reader.ReadBlock(cs, 0, cs.Length);
                Assert.True(read <= 10);

                if (read == 0) break;

                String s = new string(cs, 0, read);
                Assert.Equal(TestData.Substring(start, read), s);
                start += 10;
            }
            Assert.True(reader.EndOfStream);
            Assert.Equal(-1, reader.Peek());
            Assert.Equal(-1, reader.Read());
            reader.Close();
        }

        private void SetUriLoaderCaching(bool cachingEnabled)
        {
#if !NO_URICACHE
            Options.UriLoaderCaching = cachingEnabled;
#endif
        }

#if !PORTABLE

        [Fact(Skip="Remote configuration is not currently available")]
        public void ParsingTextReaderBlockingNetworkStreamNotation3()
        {
            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                SetUriLoaderCaching(false);
                Options.UriLoaderTimeout = 45000;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new Notation3Parser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                SetUriLoaderCaching(true);
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingTextReaderBlockingNetworkStreamNTriples()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new NTriplesParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingTextReaderBlockingNetworkStreamTurtle()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new TurtleParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void ParsingTextReaderBlockingNetworkStreamRdfJson()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"), new RdfJsonParser());

                TestTools.ShowGraph(g);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

#endif

        private void EnsureNIOData()
        {
            if (!File.Exists("resources\\nio.ttl"))
            {
                using (StreamWriter writer = File.CreateText("resources\\nio.ttl"))
                {
                    using (StreamReader reader = File.OpenText("resources\\dataset_50.ttl.gz"))
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

        [Fact]
        public void ParsingBlockingVsNonBlocking1()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine("Run #" + (i + 1));
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText("resources\\nio.ttl"));
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

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText("resources\\nio.ttl"));
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

                Assert.Equal(totalBlocking, totalNonBlocking);
            }

            Console.WriteLine();
            Console.WriteLine("Blocking Total Time = " + blockingTime);
            Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
        }

        [Fact]
        public void ParsingBlockingVsNonBlocking2()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine("Run #" + (i + 1));
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText("resources\\nio.ttl"), 4096);
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

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText("resources\\nio.ttl"), 4096);
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

                Assert.Equal(totalBlocking, totalNonBlocking);
            }

            Console.WriteLine();
            Console.WriteLine("Blocking Total Time = " + blockingTime);
            Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
        }

        [Fact]
        public void ParsingBlockingVsNonBlocking3()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            int maxSize = 1024 * 32;
            for (int size = 1024; size < maxSize; size += 1024)
            {
                Console.WriteLine("Buffer Size " + size);
                for (int i = 0; i < 25; i++)
                {
                    timer.Reset();

                    //Test Blocking
                    BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText("resources\\nio.ttl"), 4096);
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

                    //Reset
                    timer.Reset();
                    int totalNonBlocking = 0;

                    NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText("resources\\nio.ttl"), 4096);
                    timer.Start();
                    while (!nonBlocking.EndOfStream)
                    {
                        read = nonBlocking.Read();
                        if (read >= 0) totalNonBlocking++;
                    }
                    timer.Stop();
                    nonBlocking.Close();
                    nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

                    Assert.Equal(totalBlocking, totalNonBlocking);
                }

                Console.WriteLine();
                Console.WriteLine("Blocking Total Time = " + blockingTime);
                Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
                Console.WriteLine();
                blockingTime = new TimeSpan();
                nonBlockingTime = new TimeSpan();
            }
        }

        [Fact]
        public void ParsingBlockingVsNonBlocking4()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine("Run #" + (i + 1));
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(new JitterReader(File.OpenText("resources\\nio.ttl")));
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

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(new JitterReader(File.OpenText("resources\\nio.ttl")));
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

                Assert.Equal(totalBlocking, totalNonBlocking);
            }

            Console.WriteLine();
            Console.WriteLine("Blocking Total Time = " + blockingTime);
            Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
        }

        [Fact]
        public void ParsingBlockingVsNonBlocking5()
        {
            this.EnsureNIOData();

            Stopwatch timer = new Stopwatch();
            TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine("Run #" + (i + 1));
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(new JitterReader(File.OpenText("resources\\nio.ttl"), 50));
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

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(new JitterReader(File.OpenText("resources\\nio.ttl"), 50));
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

                Assert.Equal(totalBlocking, totalNonBlocking);
            }

            Console.WriteLine();
            Console.WriteLine("Blocking Total Time = " + blockingTime);
            Console.WriteLine("Non-Blocking Total Time = " + nonBlockingTime);
        }
    }

    /// <summary>
    /// A Text Reader which introduces random jitter into block reads i.e. it may randomly read less than asked for a non-blocking read and may have random delays when doing blocking reads.  By default jitter chance is set at 10%
    /// </summary>
    class JitterReader
        : TextReader
    {
        /// <summary>
        /// Default Jitter Chance
        /// </summary>
        public const int DefaultJitter = 10;

        private TextReader _reader;
        private Random _rnd = new Random();
        private int _jitter = 10;

        public JitterReader(TextReader reader)
            : this(reader, DefaultJitter) { }

        public JitterReader(TextReader reader, int chance)
        {
            this._reader = reader;
            this._jitter = chance;
            if (this._jitter < 0) this._jitter = 0;
            if (this._jitter > 100) this._jitter = 100;
        }

#if NET40
        public override void Close()
        {
            this._reader.Close();
        }
#endif

        protected override void Dispose(bool disposing)
        {
            this._reader.Dispose();
        }

        public override int Peek()
        {
            return this._reader.Peek();
        }

        public override int Read()
        {
            return this._reader.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int r = this._rnd.Next(100);
            if (r < this._jitter)  count = 1 + this._rnd.Next(count);
            return this._reader.Read(buffer, index, count);
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            int r = this._rnd.Next(100);
            if (r < this._jitter) Thread.Sleep(1);
            return this._reader.ReadBlock(buffer, index, count);
        }

        public override string ReadLine()
        {
            return this._reader.ReadLine();
        }

        public override string ReadToEnd()
        {
            return this._reader.ReadToEnd();
        }
    }
}
