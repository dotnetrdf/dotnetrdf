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

using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators;

/// <summary>
/// Interface which represents an operator in SPARQL e.g. +.
/// </summary>
public interface ISparqlOperator
{
    /// <summary>
    /// Gets the Operator this is an implementation of.
    /// </summary>
    SparqlOperatorType Operator
    {
        get;
    }

    /// <summary>
    /// Get the flag that indicates if this operator is an extension to the set of operators defined in the SPARQL specification.
    /// </summary>
    bool IsExtension { get; }

    /// <summary>
    /// Gets whether the operator can be applied to the given inputs.
    /// </summary>
    /// <param name="ns">Inputs.</param>
    /// <returns>True if applicable to the given inputs.</returns>
    bool IsApplicable(params IValuedNode[] ns);

    /// <summary>
    /// Applies the operator to the given inputs.
    /// </summary>
    /// <param name="ns">Inputs.</param>
    /// <returns></returns>
    /// <exception cref="RdfQueryException">Thrown if an error occurs in applying the operator.</exception>
    IValuedNode Apply(params IValuedNode[] ns);
}
