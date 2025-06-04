/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query;

/// <summary>
/// Types of SPARQL Query.
/// </summary>
public enum SparqlQueryType : int
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown= 0,
    /// <summary>
    /// Ask
    /// </summary>
    Ask = 1,
    /// <summary>
    /// Construct
    /// </summary>
    Construct = 2,
    /// <summary>
    /// Describe
    /// </summary>
    Describe = 3,
    /// <summary>
    /// Describe All
    /// </summary>
    DescribeAll = 4,
    /// <summary>
    /// Select
    /// </summary>
    Select = 5,
    /// <summary>
    /// Select Distinct
    /// </summary>
    SelectDistinct = 6,
    /// <summary>
    /// Select Reduced
    /// </summary>
    SelectReduced = 7,
    /// <summary>
    /// Select All
    /// </summary>
    SelectAll = 8,
    /// <summary>
    /// Select All Distinct
    /// </summary>
    SelectAllDistinct = 9,
    /// <summary>
    /// Select All Reduced
    /// </summary>
    SelectAllReduced = 10,
}