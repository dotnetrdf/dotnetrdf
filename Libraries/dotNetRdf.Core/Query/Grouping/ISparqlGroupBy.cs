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

using System.Collections.Generic;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Grouping;

/// <summary>
/// Interface for Classes that represent SPARQL GROUP BY clauses.
/// </summary>
public interface ISparqlGroupBy
{
    /// <summary>
    /// Gets/Sets the child GROUP BY Clause.
    /// </summary>
    ISparqlGroupBy Child
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the Variables used in the GROUP BY.
    /// </summary>
    IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables.
    /// </summary>
    IEnumerable<string> ProjectableVariables
    {
        get;
    }

    /// <summary>
    /// Gets the Expression used to GROUP BY.
    /// </summary>
    ISparqlExpression Expression
    {
        get;
    }

    /// <summary>
    /// Gets/Sets the Variable the value of the GROUP BY expression should be bound to (may be null if not bound to anything).
    /// </summary>
    string AssignVariable
    {
        get;
        set;
    }
}
