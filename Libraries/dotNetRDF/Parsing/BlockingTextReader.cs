/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.IO;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// An extended <see cref="TextReader"/> for use in parsing
    /// </summary>
    public abstract class ParsingTextReader
        : TextReader
    {
        /// <summary>
        /// Gets whether the end of the stream has been reached
        /// </summary>
        public abstract bool EndOfStream
        {
            get;
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Text Reader to wrap</param>
        /// <param name="bufferSize">Buffer Size</param>
        /// <remarks>
        /// If the given <see cref="TextReader">TextReader</see> is already a Blocking Text Reader this is a no-op
        /// </remarks>
        public static ParsingTextReader Create(TextReader input, int bufferSize)
        {
            if (input is ParsingTextReader) return (ParsingTextReader)input;
            if (input is StreamReader)
            {
                Stream s = ((StreamReader)input).BaseStream;
                if (!Options.ForceBlockingIO && (s is FileStream || s is MemoryStream))
                {
                    return new NonBlockingTextReader(input, bufferSize);
                }
                else
                {
                    return new BlockingTextReader(input, bufferSize);
                }
            }
            else
            {
                return new BlockingTextReader(input, bufferSize);
            }
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Text Reader to wrap</param>
        /// <remarks>
        /// If the given <see cref="TextReader">TextReader</see> is already a Blocking Text Reader this is a no-op
        /// </remarks>
        public static ParsingTextReader Create(TextReader input)
        {
            return Create(input, BufferedTextReader.DefaultBufferSize);
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <param name="bufferSize">Buffer Size</param>
        public static ParsingTextReader Create(Stream input, int bufferSize)
        {
            if (!Options.ForceBlockingIO && (input is FileStream || input is MemoryStream))
            {
                return CreateNonBlocking(new StreamReader(input), bufferSize);
            }
            else
            {
                return CreateBlocking(new StreamReader(input), bufferSize);
            }
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input Stream</param>
        public static ParsingTextReader Create(Stream input)
        {
            return Create(input, BufferedTextReader.DefaultBufferSize);
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input reader</param>
        /// <returns></returns>
        public static BlockingTextReader CreateBlocking(TextReader input)
        {
            return CreateBlocking(input, BufferedTextReader.DefaultBufferSize);
        }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input reader</param>
        /// <param name="bufferSize">Buffer Size</param>
        /// <returns></returns>
        public static BlockingTextReader CreateBlocking(TextReader input, int bufferSize)
        {
            if (input is BlockingTextReader) return (BlockingTextReader)input;
            return new BlockingTextReader(input, bufferSize);
        }

        /// <summary>
        /// Creates a new non-blocking Text Reader
        /// </summary>
        /// <param name="input">Input reader</param>
        /// <returns></returns>
        public static NonBlockingTextReader CreateNonBlocking(TextReader input)
        {
            if (input is NonBlockingTextReader) return (NonBlockingTextReader)input;
            return new NonBlockingTextReader(input);
        }

        /// <summary>
        /// Creates a new non-blocking Text Reader
        /// </summary>
        /// <param name="input">Input reader</param>
        /// <param name="bufferSize">Buffer Size</param>
        /// <returns></returns>
        public static NonBlockingTextReader CreateNonBlocking(TextReader input, int bufferSize)
        {
            if (input is NonBlockingTextReader) return (NonBlockingTextReader)input;
            return new NonBlockingTextReader(input, bufferSize);
        }
    }

    /// <summary>
    /// Abstract class representing a text reader that provides buffering on top of another text reader
    /// </summary>
    public abstract class BufferedTextReader
        : ParsingTextReader
    {
        /// <summary>
        /// Default Buffer Size
        /// </summary>
        public const int DefaultBufferSize = 1024;

        /// <summary>
        /// Buffer array
        /// </summary>
        protected char[] _buffer;
        /// <summary>
        /// Current buffer position
        /// </summary>
        protected int _pos = -1;
        /// <summary>
        /// Current buffer size (may be less than length of buffer array)
        /// </summary>
        protected int _bufferAmount = -1;
        /// <summary>
        /// Whether underlying reader has been exhausted
        /// </summary>
        protected bool _finished = false;
        /// <summary>
        /// Underlying reader
        /// </summary>
        protected readonly TextReader _reader;

        /// <summary>
        /// Creates a buffered reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="bufferSize"></param>
        /// <exception cref="ArgumentException">raised if <paramref name="bufferSize"/> is less than 1</exception>
        /// <exception cref="ArgumentNullException">raised if <paramref name="reader"/> is null</exception>
        protected BufferedTextReader(TextReader reader, int bufferSize)
        {
            if (bufferSize < 1) throw new ArgumentException("bufferSize must be >= 1", nameof(bufferSize));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader), "Cannot read from a null TextReader");
            _buffer = new char[bufferSize];
        }

        /// <summary>
        /// Requests that the buffer be filled
        /// </summary>
        protected abstract void FillBuffer();

        /// <summary>
        /// Reads a sequence of characters from the buffer in a blocking way
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index at which to start writing to the Buffer</param>
        /// <param name="count">Number of characters to read</param>
        /// <returns>Number of characters read</returns>
        public override int ReadBlock(char[] buffer, int index, int count)
        {
            if (count == 0) return 0;
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (index < 0) throw new ArgumentException("index", "Index must be >= 0");
            if (count < 0) throw new ArgumentException("count", "Count must be >= 0");
            if ((buffer.Length - index) < count) throw new ArgumentException("Buffer too small");

            if (_bufferAmount == -1 || _pos >= _bufferAmount)
            {
                if (!_finished)
                {
                    FillBuffer();
                    if (EndOfStream) return 0;
                }
                else
                {
                    return 0;
                }
            }

            _pos = Math.Max(0, _pos);
            if (count <= _bufferAmount - _pos)
            {
                // If we have sufficient things buffered to fufill the request just copy the relevant stuff across
                Array.Copy(_buffer, _pos, buffer, index, count);
                _pos += count;
                return count;
            }
            else
            {
                int copied = 0;
                while (copied < count)
                {
                    int available = _bufferAmount - _pos;
                    if (count < copied + available)
                    {
                        // We can finish fufilling this request this round
                        int toCopy = Math.Min(available, count - copied);
                        Array.Copy(_buffer, _pos, buffer, index + copied, toCopy);
                        copied += toCopy;
                        _pos += toCopy;
                        return copied;
                    }
                    else
                    {
                        // Copy everything we currently have available
                        Array.Copy(_buffer, _pos, buffer, index + copied, available);
                        copied += available;
                        _pos = _bufferAmount;

                        if (!_finished)
                        {
                            // If we haven't reached the end of the input refill our buffer and continue
                            FillBuffer();
                            if (EndOfStream) return copied;
                            _pos = 0;
                        }
                        else
                        {
                            // Otherwise we have reached the end of the input so just return what we've managed to copy
                            return copied;
                        }
                    }
                }
                return copied;
            }
        }

        /// <summary>
        /// Reads a sequence of characters from the buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index at which to start writing to the Buffer</param>
        /// <param name="count">Number of characters to read</param>
        /// <returns>Number of characters read</returns>
        public override int Read(char[] buffer, int index, int count)
        {
            return ReadBlock(buffer, index, count);
        }

        /// <summary>
        /// Reads a single character from the underlying Text Reader
        /// </summary>
        /// <returns>Character read or -1 if at end of input</returns>
        public override int Read()
        {
            if (_bufferAmount == -1 || _pos >= _bufferAmount - 1)
            {
                if (!_finished)
                {
                    FillBuffer();
                    if (EndOfStream) return -1;
                }
                else
                {
                    return -1;
                }
            }

            _pos++;
            return (int)_buffer[_pos];
        }

        /// <summary>
        /// Peeks at the next character from the underlying Text Reader
        /// </summary>
        /// <returns>Character peeked or -1 if at end of input</returns>
        public override int Peek()
        {
            if (_bufferAmount == -1 || _pos >= _bufferAmount - 1)
            {
                if (!_finished)
                {
                    FillBuffer();
                    if (EndOfStream) return -1;
                }
                else
                {
                    return -1;
                }
            }

            return (int)_buffer[_pos + 1];
        }

        /// <summary>
        /// Gets whether the end of the input has been reached
        /// </summary>
        public override bool EndOfStream
        {
            get
            {
                return _finished && (_pos >= _bufferAmount - 1);
            }
        }

#if NETCORE
        /// <summary>
        /// Closes the reader and the underlying reader
        /// </summary>
        public void Close()
        {
            // No-op as .NET Standard library version of TextReader has no Close() method
        }
#else
        /// <summary>
        /// Closes the reader and the underlying reader
        /// </summary>
        public override void Close()
        {
            _reader.Close();
        }
#endif

        /// <summary>
        /// Disposes of the reader and the underlying reader
        /// </summary>
        /// <param name="disposing">Whether this was called from the Dispose() method</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            Close();
            _reader.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// The BlockingTextReader is an implementation of a <see cref="BufferedTextReader" /> designed to wrap other readers which may or may not have high latency and thus ensures that premature end of input bug is not experienced.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is designed to avoid premature detection of end of input when the input has high latency and the consumer tries to read from the input faster than it can return data.  This derives from <see cref="BufferedTextReader"/> and ensures the buffer is filled by calling the <see cref="TextReader.ReadBlock">ReadBlock()</see> method of the underlying <see cref="TextReader">TextReader</see> thus avoiding the scenario where input appears to end prematurely.
    /// </para>
    /// </remarks>
    public sealed class BlockingTextReader 
        : BufferedTextReader
    {
        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="reader">Text Reader to wrap</param>
        /// <param name="bufferSize">Buffer Size</param>
        internal BlockingTextReader(TextReader reader, int bufferSize)
            : base(reader, bufferSize) { }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="reader">Text Reader to wrap</param>
        internal BlockingTextReader(TextReader reader)
            : this(reader, DefaultBufferSize) { }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <param name="bufferSize">Buffer Size</param>
        internal BlockingTextReader(Stream input, int bufferSize)
            : this(new StreamReader(input), bufferSize) { }

        /// <summary>
        /// Creates a new Blocking Text Reader
        /// </summary>
        /// <param name="input">Input Stream</param>
        internal BlockingTextReader(Stream input)
            : this(new StreamReader(input)) { }

        /// <summary>
        /// Fills the Buffer
        /// </summary>
        protected override void FillBuffer()
        {
            _pos = -1;
            if (_finished)
            {
                _bufferAmount = 0;
            }
            else
            {
                _bufferAmount = _reader.ReadBlock(_buffer, 0, _buffer.Length);
                if (_bufferAmount == 0 || _bufferAmount < _buffer.Length) _finished = true;
            }
        }
    }

    /// <summary>
    /// The NonBlockingTextReader is an implementation of a <see cref="BufferedTextReader"/> designed to wrap other readers where latency is known not to be a problem and we don't expect to ever have an empty read occur before the actual end of the stream
    /// </summary>
    /// <remarks>
    /// Currently we only use this for file and network streams, you can force this to never be used with the global static <see cref="Options.ForceBlockingIO"/> option
    /// </remarks>
    public sealed class NonBlockingTextReader
        : BufferedTextReader
    {
        internal NonBlockingTextReader(TextReader input, int bufferSize)
            : base(input, bufferSize) { }

        internal NonBlockingTextReader(TextReader input)
            : this(input, DefaultBufferSize) { }

        internal NonBlockingTextReader(Stream input, int bufferSize)
            : this(new StreamReader(input), bufferSize) { }

        internal NonBlockingTextReader(Stream input)
            : this(new StreamReader(input)) { }

        /// <summary>
        /// Fills the buffer in a non-blocking manner
        /// </summary>
        protected override void FillBuffer()
        {
            _pos = -1;
            if (_finished)
            {
                _bufferAmount = 0;
            }
            else
            {
                _bufferAmount = _reader.Read(_buffer, 0, _buffer.Length);
                if (_bufferAmount == 0) _finished = true;
            }
        }
    }
}
