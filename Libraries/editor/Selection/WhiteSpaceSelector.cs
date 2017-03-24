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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.Selection
{
    /// <summary>
    /// A selector which adds white space as a deliminator
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class WhiteSpaceSelector<T> 
        : DefaultSelector<T>
    {
        /// <summary>
        /// Gets whether the given character is a starting deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is whitespace of a standard deliminator, false otherwise</returns>
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsStartingDeliminator(c);
        }

        /// <summary>
        /// Gets whether the given character is an ending deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is whitespace or a standard deliminator, false otherwise</returns>
        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsEndingDeliminator(c);
        }
    }

    /// <summary>
    /// A selector which adds punctuation as a deliminator
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class PunctuationSelector<T> 
        : DefaultSelector<T>
    {
        /// <summary>
        /// Gets whether the given character is a starting deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is punctuation or a standard deliminator, false otherwise</returns>
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsPunctuation(c) || base.IsStartingDeliminator(c);
        }

        /// <summary>
        /// Gets whether the given character is an ending deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is punctuation or a standard deliminator, false otherwise</returns>
        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsPunctuation(c) || base.IsEndingDeliminator(c);
        }
    }

    /// <summary>
    /// A selecotr which adds punctuation and white space as a deliminator
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class WhiteSpaceOrPunctuationSelection<T> 
        : DefaultSelector<T>
    {
        /// <summary>
        /// Gets whether the given character is a starting deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is punctuation, white space or a standard deliminator, false otherwise</returns>
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || Char.IsPunctuation(c) || base.IsStartingDeliminator(c);
        }

        /// <summary>
        /// Gets whether the given character is an ending deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>True if the character is punctuation, white space or a standard deliminator, false otherwise</returns>
        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || Char.IsPunctuation(c) || base.IsEndingDeliminator(c);
        }
    }
}
