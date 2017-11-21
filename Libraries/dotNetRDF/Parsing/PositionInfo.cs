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

using System.Xml;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Represents Position Information from Parsers
    /// </summary>
    public class PositionInfo
    {
        private int _startLine, _endLine, _startPos, _endPos;

        /// <summary>
        /// Creates a new set of Position Information
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="position">Column</param>
        public PositionInfo(int line, int position)
        {
            _startLine = _endLine = line;
            _startPos = _endPos = position;
        }

        /// <summary>
        /// Creates a new set of Position Information
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="startPosition">Start Column</param>
        /// <param name="endPosition">End Column</param>
        public PositionInfo(int line, int startPosition, int endPosition)
            : this(line, startPosition)
        {
            _endPos = endPosition;
        }

        /// <summary>
        /// Creates a new set of Position Information
        /// </summary>
        /// <param name="startLine">Start Line</param>
        /// <param name="endLine">End Line</param>
        /// <param name="startPosition">Start Column</param>
        /// <param name="endPosition">End Column</param>
        public PositionInfo(int startLine, int endLine, int startPosition, int endPosition)
            : this(startLine, startPosition, endPosition)
        {
            _endLine = endLine;
        }

        /// <summary>
        /// Creates a new set of Position Information form some XML Line Information
        /// </summary>
        /// <param name="info">XML Line Information</param>
        public PositionInfo(IXmlLineInfo info)
            : this(info.LineNumber, info.LinePosition) { }

        /// <summary>
        /// Gets the Start Line
        /// </summary>
        public int StartLine
        {
            get
            {
                return _startLine;
            }
        }

        /// <summary>
        /// Gets the End Line
        /// </summary>
        public int EndLine
        {
            get
            {
                return _endLine;
            }
        }

        /// <summary>
        /// Gets the Start Column
        /// </summary>
        public int StartPosition
        {
            get
            {
                return _startPos;
            }
        }

        /// <summary>
        /// Gets the End Column
        /// </summary>
        public int EndPosition
        {
            get
            {
                return _endPos;
            }
        }
    }
}
