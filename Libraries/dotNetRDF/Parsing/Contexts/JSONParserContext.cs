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

using Newtonsoft.Json;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for RDF/JSON Parsers
    /// </summary>
    public class JsonParserContext : BaseParserContext
    {
        private JsonTextReader _input;

        /// <summary>
        /// Creates a new JSON Parser Context
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="input">JSON Text Reader to read from</param>
        public JsonParserContext(IGraph g, JsonTextReader input)
            : base(g)
        {
            _input = input;
        }

        /// <summary>
        /// Creates a new JSON Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">JSON Text Reader to read from</param>
        public JsonParserContext(IRdfHandler handler, JsonTextReader input)
            : base(handler)
        {
            _input = input;
        }

        /// <summary>
        /// Gets the JSON Text Reader which input is read from
        /// </summary>
        public JsonTextReader Input
        {
            get
            {
                return _input;
            }
        }

        /// <summary>
        /// Gets the Current Position of the JSON Text Reader
        /// </summary>
        public PositionInfo CurrentPosition
        {
            get
            {
                if (_input.LineNumber == 0 && _input.LinePosition == 0)
                {
                    return new PositionInfo(1, 1);
                }
                else
                {
                    return new PositionInfo(_input.LineNumber, _input.LinePosition);
                }
            }
        }

        /// <summary>
        /// Gets the Position range from the given Start Position to the current Position
        /// </summary>
        /// <param name="startPosition">Start Position</param>
        /// <returns></returns>
        public PositionInfo GetPositionRange(PositionInfo startPosition)
        {
            return new PositionInfo(startPosition.StartLine, _input.LineNumber, startPosition.StartPosition, _input.LinePosition);
        }
    }
}
