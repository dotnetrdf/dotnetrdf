/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
