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
using System.Text;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace VDS.RDF.Parsing;

[Collection("RdfServer")]
public class BlockingTextReaderTests : BaseTest
{
    private const String TestData = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const String TestData2 = "abcdefghijklmnopqrstuvwxyz\n0123456789";
    private const String TestData3 = "abcdefghijklmnopqrstuvwxyz\r0123456789";
    private const String TestData4 = "abcdefghijklmnopqrstuvwxyz\n\r0123456789";
    private const String TestData5 = "abcdefghijklmnopqrstuvwxyz\r\n0123456789";

    private readonly RdfServerFixture _serverFixture;

    public BlockingTextReaderTests(RdfServerFixture serverFixture, ITestOutputHelper output) : base(output)
    {
        _serverFixture = serverFixture;
    }


    [Fact]
    public void ParsingTextReaderCreation1()
    {
        File.WriteAllText("ParsingTextReaderCreation1.txt", "ParsingTextReaderCreation1");
        using (StreamReader stream = File.OpenText("ParsingTextReaderCreation1.txt"))
        {
            var reader = ParsingTextReader.Create(stream);
            Assert.IsType<NonBlockingTextReader>(reader);
            stream.Close();
        }
    }

    [Fact]
    public void ParsingTextReaderCreation2()
    {
        using (var stream = new MemoryStream())
        {
            var reader = ParsingTextReader.Create(stream);
            Assert.IsType<NonBlockingTextReader>(reader);
            stream.Close();
        }
    }

    [Fact]
    public void ParsingTextReaderCreation3()
    {
        using (var client = new WebClient())
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            using (Stream stream = client.OpenRead(new Uri("http://www.dotnetrdf.org")))
            {
                var reader = ParsingTextReader.Create(stream);
                Assert.IsType<BlockingTextReader>(reader);
                stream.Close();
            }
            client.Dispose();
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var output = new StringBuilder();
        while (!reader.EndOfStream)
        {
            output.Append((char)reader.Read());
        }
        Assert.True(reader.EndOfStream);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());
        reader.Close();

        Debug(output.ToString());

        Assert.Equal(TestData, output.ToString());
    }

    [Fact]
    public void ParsingTextReaderBlockingSimpleRead2()
    {
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var cs = new char[100];
        var read = reader.Read(cs, 0, cs.Length);
        Assert.Equal(TestData.Length, read);
        Assert.True(reader.EndOfStream);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());

        reader.Close();

        var s = new string(cs, 0, read);
        Assert.Equal(TestData, s);
    }


    [Fact]
    public void ParsingTextReaderBlockingSimpleReadBlock()
    {
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var cs = new char[100];
        var read = reader.ReadBlock(cs, 0, cs.Length);
        Assert.Equal(TestData.Length, read);
        Assert.True(reader.EndOfStream);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());

        reader.Close();

        var s = new string(cs, 0, read);
        Assert.Equal(TestData, s);          
    }

    [Fact]
    public void ParsingTextReaderBlockingReadToEnd()
    {
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadToEnd();
        Assert.Equal(TestData, s);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());

        reader.Close();
    }

    [Fact]
    public void ParsingTextReaderBlockingReadLine()
    {
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadLine();
        Assert.Equal(TestData, s);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());

        reader.Close();
    }

    [Fact]
    public void ParsingTextReaderBlockingReadLine2()
    {
        var strReader = new StringReader(TestData2);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadLine();
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
        var strReader = new StringReader(TestData3);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadLine();
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
        var strReader = new StringReader(TestData4);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadLine();
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
        var strReader = new StringReader(TestData5);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var s = reader.ReadLine();
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        for (var i = 1; i <= 100; i++)
        {
            var c = (char)reader.Peek();
            Debug("Peek #" + i + " = " + c);
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader);

        var cs = new char[10];
        var start = 0;
        while (!reader.EndOfStream)
        {
            var read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.True(read <= 10);

            var s = new string(cs, 0, read);
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

        var cs = new char[10];
        var start = 0;
        while (!reader.EndOfStream)
        {
            var read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.True(read <= 10);

            if (read == 0) break;

            var s = new string(cs, 0, read);
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 5);

        var cs = new char[8];
        var start = 0;
        while (!reader.EndOfStream)
        {
            var read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.True(read <= 8);

            if (read == 0) break;

            var s = new string(cs, 0, read);
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
        var strReader = new StringReader(TestData);

        BlockingTextReader reader = ParsingTextReader.CreateBlocking(strReader, 2);

        var cs = new char[10];
        var start = 0;
        while (!reader.EndOfStream)
        {
            var read = reader.ReadBlock(cs, 0, cs.Length);
            Assert.True(read <= 10);

            if (read == 0) break;

            var s = new string(cs, 0, read);
            Assert.Equal(TestData.Substring(start, read), s);
            start += 10;
        }
        Assert.True(reader.EndOfStream);
        Assert.Equal(-1, reader.Peek());
        Assert.Equal(-1, reader.Read());
        reader.Close();
    }

    [Theory]
    [InlineData(typeof(Notation3Parser))]
    [InlineData(typeof(NTriplesParser))]
    [InlineData(typeof(RdfXmlParser))]
    [InlineData(typeof(RdfJsonParser))]
    [InlineData(typeof(JsonLdParser))]
    [InlineData(typeof(TurtleParser))]
    public void ParsingTextReaderBlockingNetwork(Type parserType)
    {
        var loader = new Loader(_serverFixture.Client);
        var g = new Graph();
        var parser = parserType.GetConstructor(new Type []{}).Invoke(new object []{}) as IRdfReader;
        Uri uri = _serverFixture.UriFor("/doap#");
        loader.LoadGraph(g, uri, parser);
        g.Triples.Count.Should().BeGreaterThan(0);
    }


    private void EnsureNIOData()
    {
        if (!File.Exists(Path.Combine("resources", "nio.ttl")))
        {
            using (StreamWriter writer = File.CreateText(Path.Combine("resources", "nio.ttl")))
            {
                using (StreamReader reader = File.OpenText(Path.Combine("resources", "dataset_50.ttl.gz")))
                {
                    var line = reader.ReadLine();
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
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void ParsingBlockingVsNonBlocking1()
    {
        EnsureNIOData();

        var timer = new Stopwatch();
        TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

        for (var i = 0; i < 25; i++)
        {
            Debug("Run #" + (i + 1));
            timer.Reset();

            //Test Blocking
            BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")));
            timer.Start();
            var totalBlocking = 0;
            int read;
            while (!blocking.EndOfStream)
            {
                read = blocking.Read();
                if (read >= 0) totalBlocking++;
            }
            timer.Stop();
            blocking.Close();
            blockingTime = blockingTime.Add(timer.Elapsed);

            Debug("Blocking IO took " + timer.Elapsed + " and read " + totalBlocking + " characters");

            //Reset
            timer.Reset();
            var totalNonBlocking = 0;

            NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")));
            timer.Start();
            while (!nonBlocking.EndOfStream)
            {
                read = nonBlocking.Read();
                if (read >= 0) totalNonBlocking++;
            }
            timer.Stop();
            nonBlocking.Close();
            nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

            Debug("Non-Blocking IO took " + timer.Elapsed + " and read " + totalNonBlocking + " characters");

            Assert.Equal(totalBlocking, totalNonBlocking);
        }

        Debug();
        Debug("Blocking Total Time = " + blockingTime);
        Debug("Non-Blocking Total Time = " + nonBlockingTime);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void ParsingBlockingVsNonBlocking2()
    {
        EnsureNIOData();

        var timer = new Stopwatch();
        TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

        for (var i = 0; i < 25; i++)
        {
            Debug("Run #" + (i + 1));
            timer.Reset();

            //Test Blocking
            BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")), 4096);
            timer.Start();
            var totalBlocking = 0;
            int read;
            while (!blocking.EndOfStream)
            {
                read = blocking.Read();
                if (read >= 0) totalBlocking++;
            }
            timer.Stop();
            blocking.Close();
            blockingTime = blockingTime.Add(timer.Elapsed);

            Debug("Blocking IO took " + timer.Elapsed + " and read " + totalBlocking + " characters");

            //Reset
            timer.Reset();
            var totalNonBlocking = 0;

            NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")), 4096);
            timer.Start();
            while (!nonBlocking.EndOfStream)
            {
                read = nonBlocking.Read();
                if (read >= 0) totalNonBlocking++;
            }
            timer.Stop();
            nonBlocking.Close();
            nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

            Debug("Non-Blocking IO took " + timer.Elapsed + " and read " + totalNonBlocking + " characters");

            Assert.Equal(totalBlocking, totalNonBlocking);
        }

        Debug();
        Debug("Blocking Total Time = " + blockingTime);
        Debug("Non-Blocking Total Time = " + nonBlockingTime);
    }

    [Fact]
    [Trait("Category", "performance")]
    public void ParsingBlockingVsNonBlocking3()
    {
        EnsureNIOData();

        var timer = new Stopwatch();
        TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

        var maxSize = 1024 * 32;
        for (var size = 1024; size < maxSize; size += 1024)
        {
            Debug("Buffer Size " + size);
            for (var i = 0; i < 25; i++)
            {
                timer.Reset();

                //Test Blocking
                BlockingTextReader blocking = ParsingTextReader.CreateBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")), 4096);
                timer.Start();
                var totalBlocking = 0;
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
                var totalNonBlocking = 0;

                NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(File.OpenText(Path.Combine("resources", "nio.ttl")), 4096);
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

            Debug();
            Debug("Blocking Total Time = " + blockingTime);
            Debug("Non-Blocking Total Time = " + nonBlockingTime);
            Debug();
            blockingTime = new TimeSpan();
            nonBlockingTime = new TimeSpan();
        }
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void ParsingBlockingVsNonBlocking4()
    {
        EnsureNIOData();

        var timer = new Stopwatch();
        TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

        for (var i = 0; i < 25; i++)
        {
            Debug("Run #" + (i + 1));
            timer.Reset();

            //Test Blocking
            BlockingTextReader blocking = ParsingTextReader.CreateBlocking(new JitterReader(File.OpenText(Path.Combine("resources", "nio.ttl"))));
            timer.Start();
            var totalBlocking = 0;
            int read;
            while (!blocking.EndOfStream)
            {
                read = blocking.Read();
                if (read >= 0) totalBlocking++;
            }
            timer.Stop();
            blocking.Close();
            blockingTime = blockingTime.Add(timer.Elapsed);

            Debug("Blocking IO took " + timer.Elapsed + " and read " + totalBlocking + " characters");

            //Reset
            timer.Reset();
            var totalNonBlocking = 0;

            NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(new JitterReader(File.OpenText(Path.Combine("resources", "nio.ttl"))));
            timer.Start();
            while (!nonBlocking.EndOfStream)
            {
                read = nonBlocking.Read();
                if (read >= 0) totalNonBlocking++;
            }
            timer.Stop();
            nonBlocking.Close();
            nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

            Debug("Non-Blocking IO took " + timer.Elapsed + " and read " + totalNonBlocking + " characters");

            Assert.Equal(totalBlocking, totalNonBlocking);
        }

        Debug();
        Debug("Blocking Total Time = " + blockingTime);
        Debug("Non-Blocking Total Time = " + nonBlockingTime);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void ParsingBlockingVsNonBlocking5()
    {
        EnsureNIOData();

        var timer = new Stopwatch();
        TimeSpan blockingTime = new TimeSpan(), nonBlockingTime = new TimeSpan();

        for (var i = 0; i < 25; i++)
        {
            Debug("Run #" + (i + 1));
            timer.Reset();

            //Test Blocking
            BlockingTextReader blocking = ParsingTextReader.CreateBlocking(new JitterReader(File.OpenText(Path.Combine("resources", "nio.ttl")), 50));
            timer.Start();
            var totalBlocking = 0;
            int read;
            while (!blocking.EndOfStream)
            {
                read = blocking.Read();
                if (read >= 0) totalBlocking++;
            }
            timer.Stop();
            blocking.Close();
            blockingTime = blockingTime.Add(timer.Elapsed);

            Debug("Blocking IO took " + timer.Elapsed + " and read " + totalBlocking + " characters");

            //Reset
            timer.Reset();
            var totalNonBlocking = 0;

            NonBlockingTextReader nonBlocking = ParsingTextReader.CreateNonBlocking(new JitterReader(File.OpenText(Path.Combine("resources", "nio.ttl")), 50));
            timer.Start();
            while (!nonBlocking.EndOfStream)
            {
                read = nonBlocking.Read();
                if (read >= 0) totalNonBlocking++;
            }
            timer.Stop();
            nonBlocking.Close();
            nonBlockingTime = nonBlockingTime.Add(timer.Elapsed);

            Debug("Non-Blocking IO took " + timer.Elapsed + " and read " + totalNonBlocking + " characters");

            Assert.Equal(totalBlocking, totalNonBlocking);
        }

        Debug();
        Debug("Blocking Total Time = " + blockingTime);
        Debug("Non-Blocking Total Time = " + nonBlockingTime);
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
        _reader = reader;
        _jitter = chance;
        if (_jitter < 0) _jitter = 0;
        if (_jitter > 100) _jitter = 100;
    }

    public override void Close()
    {
        _reader.Close();
    }

    protected override void Dispose(bool disposing)
    {
        _reader.Dispose();
    }

    public override int Peek()
    {
        return _reader.Peek();
    }

    public override int Read()
    {
        return _reader.Read();
    }

    public override int Read(char[] buffer, int index, int count)
    {
        var r = _rnd.Next(100);
        if (r < _jitter)  count = 1 + _rnd.Next(count);
        return _reader.Read(buffer, index, count);
    }

    public override int ReadBlock(char[] buffer, int index, int count)
    {
        var r = _rnd.Next(100);
        if (r < _jitter) Thread.Sleep(1);
        return _reader.ReadBlock(buffer, index, count);
    }

    public override string ReadLine()
    {
        return _reader.ReadLine();
    }

    public override string ReadToEnd()
    {
        return _reader.ReadToEnd();
    }
}
