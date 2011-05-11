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
            this._input = input;
        }

        /// <summary>
        /// Creates a new JSON Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">JSON Text Reader to read from</param>
        public JsonParserContext(IRdfHandler handler, JsonTextReader input)
            : base(handler)
        {
            this._input = input;
        }

        /// <summary>
        /// Gets the JSON Text Reader which input is read from
        /// </summary>
        public JsonTextReader Input
        {
            get
            {
                return this._input;
            }
        }

        /// <summary>
        /// Gets the Current Position of the JSON Text Reader
        /// </summary>
        public PositionInfo CurrentPosition
        {
            get
            {
                if (this._input.LineNumber == 0 && this._input.LinePosition == 0)
                {
                    return new PositionInfo(1, 1);
                }
                else
                {
                    return new PositionInfo(this._input.LineNumber, this._input.LinePosition);
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
            return new PositionInfo(startPosition.StartLine, this._input.LineNumber, startPosition.StartPosition, this._input.LinePosition);
        }
    }
}
