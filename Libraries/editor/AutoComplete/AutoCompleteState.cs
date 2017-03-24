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

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Auto-Completion States
    /// </summary>
    public enum AutoCompleteState
    {
        /// <summary>
        /// Disabled
        /// </summary>
        Disabled,
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Inserted
        /// </summary>
        Inserted,

        /// <summary>
        /// Prefix
        /// </summary>
        Prefix,
        /// <summary>
        /// Base
        /// </summary>
        Base,
        /// <summary>
        /// Other Declaration
        /// </summary>
        Declaration,

        /// <summary>
        /// URI
        /// </summary>
        Uri,
        /// <summary>
        /// QName
        /// </summary>
        QName,
        /// <summary>
        /// Keyword
        /// </summary>
        Keyword,
        /// <summary>
        /// Keyword or QName
        /// </summary>
        KeywordOrQName,

        /// <summary>
        /// Blank Node
        /// </summary>
        BNode,

        /// <summary>
        /// Variable
        /// </summary>
        Variable,

        /// <summary>
        /// Literal
        /// </summary>
        Literal,
        /// <summary>
        /// Long Literal
        /// </summary>
        LongLiteral,
        /// <summary>
        /// Alternative Literal
        /// </summary>
        AlternateLiteral,
        /// <summary>
        /// Alternative Long Literal
        /// </summary>
        AlternateLongLiteral,
        /// <summary>
        /// Numeric Literal
        /// </summary>
        NumericLiteral,

        /// <summary>
        /// Comment
        /// </summary>
        Comment
    }
}
