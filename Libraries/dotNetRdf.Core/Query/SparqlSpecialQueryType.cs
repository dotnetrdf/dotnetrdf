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
/// Types of Special SPARQL Query which may be optimised in special ways by the libraries SPARQL Engines.
/// </summary>
public enum SparqlSpecialQueryType
{
    /// <summary>
    /// The Query is of the form SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
    /// </summary>
    DistinctGraphs,
    /// <summary>
    /// The Query has no applicable special optimisation
    /// </summary>
    NotApplicable,
    /// <summary>
    /// The Query has not yet been tested to determine if special optimisations are applicable
    /// </summary>
    Unknown,
    /// <summary>
    /// The Query is of the form ASK WHERE {?s ?p ?o}
    /// </summary>
    AskAnyTriples,
}