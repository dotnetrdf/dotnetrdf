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

// unset

using System;
using System.Collections.Generic;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

/// <summary>
/// Interface representing a single row in a SPARQL results set.
/// </summary>
public interface ISparqlResult : IEnumerable<KeyValuePair<string, INode>>, IEquatable<ISparqlResult>
{
    /// <summary>
    /// Gets the Value that is bound to the given Variable.
    /// </summary>
    /// <param name="variable">Variable whose Value you wish to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="RdfException">Thrown if there is nothing bound to the given Variable Name for this Result.</exception>
    INode Value(string variable);

    /// <summary>
    /// Gets the Value that is bound to the given Variable.
    /// </summary>
    /// <param name="variable">Variable whose Value you wish to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="RdfException">Thrown if there is nothing bound to the given Variable Name for this Result.</exception>
    INode this[string variable] { get; }

    /// <summary>
    /// Gets the Value that is bound at the given Index.
    /// </summary>
    /// <param name="index">Index whose Value you wish to retrieve.</param>
    /// <returns></returns>
    /// <remarks>
    /// As of 1.0.0 the order of variables in a result may/may not vary depending on the original query.  If a specific variable list was declared dotNetRDF tries to preserve that order but this may not always happen depending on how results are received.
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">Thrown if there is nothing bound at the given Index.</exception>
    INode this[int index] { get; }

    /// <summary>
    /// Tries to get a value (which may be null) for the variable.
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <param name="value">Value.</param>
    /// <returns>True if the variable was present (even it was unbound) and false otherwise.</returns>
    bool TryGetValue(string variable, out INode value);

    /// <summary>
    /// Tries to get a non-null value for the variable.
    /// </summary>
    /// <param name="variable">Variable.</param>
    /// <param name="value">Value.</param>
    /// <returns>True if the variable was present and bound, false otherwise.</returns>
    bool TryGetBoundValue(string variable, out INode value);

    /// <summary>
    /// Gets the number of Variables for which this Result contains Bindings.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the set of Variables that are bound in this Result.
    /// </summary>
    IEnumerable<string> Variables { get; }

    /// <summary>
    /// Gets whether a Result is a Ground Result.
    /// </summary>
    /// <remarks>
    /// A <strong>Ground Result</strong> is a result which is considered to be a fixed fact.  In practise this means it contains no Blank Nodes.
    /// </remarks>
    bool IsGroundResult { get; }

    /// <summary>
    /// Checks whether a given Variable has a value (which may be null) for this result.
    /// </summary>
    /// <param name="variable">Variable Name.</param>
    /// <returns>True if the variable is present, false otherwise.</returns>
    /// <remarks>Returns true even if the value is null, use <see cref="HasBoundValue"/> instead to see whether a non-null value is present for a variable.</remarks>
    bool HasValue(string variable);

    /// <summary>
    /// Checks whether a given Variable has a non-null value for this result.
    /// </summary>
    /// <param name="variable">Variable Name.</param>
    /// <returns>True if the variable is present and has a non-null value, false otherwise.</returns>
    bool HasBoundValue(string variable);

    /// <summary>
    /// Removes all Variables Bindings where the Variable is Unbound.
    /// </summary>
    void Trim();

    /// <summary>
    /// Displays the Result as a comma separated string of paris of the form ?var = value where values are formatted using the given Node Formatter.
    /// </summary>
    /// <param name="formatter">Node Formatter.</param>
    /// <returns></returns>
    string ToString(INodeFormatter formatter);

    /// <summary>
    /// Sets the value bound to the specified variable, adding that variable to the result if necessary.
    /// </summary>
    /// <param name="variable">Variable name.</param>
    /// <param name="value">Value to bind to the variable.</param>
    void SetValue(string variable, INode value);
}