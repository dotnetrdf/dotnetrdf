/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Aggregation;

/// <summary>
/// An interface to be implemented by classes providing aggregation over an ISet enumeration
/// </summary>
public interface IAsyncAggregation
{
    /// <summary>
    /// Get the name of the variable that the <see cref="Value"/> node will be bound to
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Get the current value of the aggregation. 
    /// </summary>
    /// <remarks></remarks>
    public INode? Value { get; }

    /// <summary>
    /// Invoked before the first <see cref="ISet"/> is passed to <see cref="Accept"/>.
    /// </summary>
    public void Start();
    
    /// <summary>
    /// Invoked once for each solution binding to aggregate over.
    /// </summary>
    /// <param name="expressionContext">The context for evaluation of the aggregate expression, consisting of a set of variable bindings and an optional active graph name.</param>
    /// <returns>True if the solution binding was successfully processed by the aggregate, false if an error has occured.</returns>
    /// <remarks>If a call to <see cref="Accept"/> returns false, the caller may choose to end iteration of solutions as an error during aggregation will typically result in a null value being produced for the aggregation.</remarks>
    public bool Accept(ExpressionContext expressionContext);
    
    /// <summary>
    /// Invoked once after the last <see cref="ISet"/> is passed to <see cref="Accept"/>
    /// </summary>
    public void End();
}