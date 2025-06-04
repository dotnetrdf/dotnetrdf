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

namespace VDS.RDF.Parsing;

/// <summary>
/// Available Query Syntaxes.
/// </summary>
public enum SparqlQuerySyntax
{
    /// <summary>
    /// Use SPARQL 1.0
    /// </summary>
    Sparql_1_0,

    /// <summary>
    /// Use SPARQL 1.1
    /// </summary>
    Sparql_1_1,

    /// <summary>
    /// Use the latest SPARQL specification supported by the library (currently SPARQL 1.1) with some extensions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extensions include the following:
    /// </para>
    /// <ul>
    /// <li><strong>LET</strong> assignments (we recommend using the SPARQL 1.1 standards BIND instead)</li>
    /// <li>Additional aggregates - <strong>NMAX</strong>, <strong>NMIN</strong>, <strong>MEDIAN</strong> and <strong>MODE</strong> (we recommend using the Leviathan Function Library URIs for these instead to make them usable in SPARQL 1.1 mode)</li>
    /// <li><strong>UNSAID</strong> alias for <strong>NOT EXISTS</strong> (we recommend using the SPARQL 1.1 standard NOT EXISTS instead</li>
    /// <li><strong>EXISTS</strong> and <strong>NOT EXISTS</strong> are permitted as Graph Patterns (only allowed in FILTERs in SPARQL 1.1)</li>
    /// </ul>
    /// </remarks>
    Extended,

    /// <summary>
    /// Use SPARQL 1.1 with the SPARQL-Star syntax extensions
    /// </summary>
    Sparql_Star_1_1,
}
