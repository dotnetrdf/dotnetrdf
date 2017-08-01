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

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Enumeration of the supported container mappings
    /// </summary>
    [Flags]
    public enum JsonLdContainer
    {
        /// <summary>
        /// No container mapping
        /// </summary>
        Null = 0x0,
        /// <summary>
        /// @list container mapping
        /// </summary>
        List = 0x1,
        /// <summary>
        /// @set container mapping
        /// </summary>
        Set = 0x2,
        /// <summary>
        /// @index container mapping
        /// </summary>
        Index = 0x4,
        /// <summary>
        /// @id container mapping
        /// </summary>
        Id = 0x8,
        /// <summary>
        /// @type container mapping
        /// </summary>
        Type = 0x10,
        /// <summary>
        /// @language container mapping
        /// </summary>
        Language = 0x20
    }
}