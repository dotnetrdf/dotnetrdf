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
            this._startLine = this._endLine = line;
            this._startPos = this._endPos = position;
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
            this._endPos = endPosition;
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
            this._endLine = endLine;
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
                return this._startLine;
            }
        }

        /// <summary>
        /// Gets the End Line
        /// </summary>
        public int EndLine
        {
            get
            {
                return this._endLine;
            }
        }

        /// <summary>
        /// Gets the Start Column
        /// </summary>
        public int StartPosition
        {
            get
            {
                return this._startPos;
            }
        }

        /// <summary>
        /// Gets the End Column
        /// </summary>
        public int EndPosition
        {
            get
            {
                return this._endPos;
            }
        }
    }
}
