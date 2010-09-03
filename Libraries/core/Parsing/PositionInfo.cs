using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing
{
    public class PositionInfo
    {
        private int _startLine, _endLine, _startPos, _endPos;

        public PositionInfo(int line, int position)
        {
            this._startLine = this._endLine = line;
            this._startPos = this._endPos = position;
        }

        public PositionInfo(int line, int startPosition, int endPosition)
            : this(line, startPosition)
        {
            this._endPos = endPosition;
        }

        public PositionInfo(int startLine, int endLine, int startPosition, int endPosition)
            : this(startLine, startPosition, endPosition)
        {
            this._endLine = endLine;
        }

        public int StartLine
        {
            get
            {
                return this._startLine;
            }
        }

        public int EndLine
        {
            get
            {
                return this._endLine;
            }
        }

        public int StartPosition
        {
            get
            {
                return this._startPos;
            }
        }

        public int EndPosition
        {
            get
            {
                return this._endPos;
            }
        }
    }
}
