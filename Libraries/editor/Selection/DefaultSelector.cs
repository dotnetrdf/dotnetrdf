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

namespace VDS.RDF.Utilities.Editor.Selection
{
    /// <summary>
    /// Selector which selects the symbol deliminated by a starting &lt; " or new line and by an ending &gt; " or new line.
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class DefaultSelector<T> 
        : BaseSelector<T>
    {
        /// <summary>
        /// Gets whether a Character is a Starting Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if a character is a starting deliminator</returns>
        protected override bool IsStartingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '<':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a Character is an Ending Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if a character is an ending deliminator, false otherwise</returns>
        protected override bool IsEndingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '>':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a specific Starting Deliminator should be matched with a specific ending deliminator
        /// </summary>
        /// <param name="c">Starting Deliminator</param>
        /// <returns>True if a matching deliminator is required, false otherwise</returns>
        protected override char? RequireMatchingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                    return c;
                case '<':
                    return '>';
                default:
                    return null;
            }
        }
    }
}
