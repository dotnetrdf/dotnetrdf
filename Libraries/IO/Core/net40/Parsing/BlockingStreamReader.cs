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
using System.Linq;
using System.Text;
using System.IO;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// A wrapper to a Stream which does all its Read() and Peek() calls using ReadBlock() to handle slow underlying streams (eg Network Streams)
    /// </summary>
    [Obsolete("Obsoleted in favour of the new BlockingTextReader which can wrap any TextReader and provides better buffering", true)]
    public sealed class BlockingStreamReader : StreamReader
    {
        private bool _peeked = false;
        private int _peekChar = -1;

        /// <summary>
        /// Creates a new Blocking Stream Reader from a StreamReader
        /// </summary>
        /// <param name="reader">Stream Reader to wrap in a Blocking Stream Reader</param>
        public BlockingStreamReader(StreamReader reader) : base(reader.BaseStream) { }

        /// <summary>
        /// Creates a new Blocking Stream Reader from a Stream
        /// </summary>
        /// <param name="stream">Stream to wrap in a Blocking Stream Reader</param>
        public BlockingStreamReader(Stream stream) : base(stream) { }

        /// <summary>
        /// Reads a single character from the input Stream and advances the position in the Stream
        /// </summary>
        /// <returns>Next character from the Stream or -1 if at the end of the Stream</returns>
        public override int Read()
        {
            if (this._peeked)
            {
                this._peeked = false;
                return this._peekChar;
            }
            else
            {
                if (this.EndOfStream) return -1;

                char[] cs = new char[1];
                base.ReadBlock(cs, 0, 1);

                return cs[0];
            }
        }

        /// <summary>
        /// Reads the next character in the input Stream but preserves the position in the Stream
        /// </summary>
        /// <returns>Next Character from the Stream or -1 if at the end of the Stream</returns>
        public override int Peek()
        {
            if (this._peeked)
            {
                return this._peekChar;
            }
            else
            {
                if (this.EndOfStream) return -1;

                this._peeked = true;

                char[] cs = new char[1];
                base.ReadBlock(cs, 0, 1);

                this._peekChar = cs[0];
                return this._peekChar;
            }
        }

        /// <summary>
        /// Gets whether the End of the Stream been reached
        /// </summary>
        /// <remarks>Since <see cref="StreamReader.EndOfStream">StreamReader.EndOfStream</see> cannot be overridden this class only shadows the property.  This means if you use a <see cref="BlockingStreamReader">BlockingStreamReader</see> in a variable typed <see cref="StreamReader">StreamReader</see> you will hit the end of the Stream one character early which may cause you issues.  If you need a <see cref="BlockingStreamReader">BlockingStreamReader</see> then you must ensure to strongly type it as such</remarks>
        public new bool EndOfStream
        {
            get
            {
                #if MONO
                    return (!this._peeked && base.Peek() < 0);
                #else
                    return (!this._peeked && base.EndOfStream);
                #endif
            }
        }
    }
}
