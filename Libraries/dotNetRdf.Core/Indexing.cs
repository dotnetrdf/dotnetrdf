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

namespace VDS.RDF;

/// <summary>
/// Possible Triple Index types.
/// </summary>
/// <remarks>
/// <para>
/// Index types are given Integer values with the lowest being the least useful index and the highest being most useful index.  Non-Index based Patterns are given arbitrary high values since these will typically never be used as these items are usually inserted into a Graph Pattern after the ordering step.
/// </para>
/// <para>
/// When used to sort Patterns as part of query optimisation the patterns are partially ordered on the usefullness of their index since more useful indexes are considered more likely to return fewer results which will help restrict the query space earlier in the execution process.
/// </para>
/// </remarks>
public enum TripleIndexType : int
{
    /// <summary>
    /// No Index should be used as the Pattern does not use Variables
    /// </summary>
    NoVariables = -2,
    /// <summary>
    /// No Index should be used as the Pattern is three Variables
    /// </summary>
    None = -1,
    /// <summary>
    /// Subject Index should be used
    /// </summary>
    Subject = 2,
    /// <summary>
    /// Predicate Index should be used
    /// </summary>
    Predicate = 1,
    /// <summary>
    /// Object Index should be used
    /// </summary>
    Object = 0,
    /// <summary>
    /// Subject-Predicate Index should be used
    /// </summary>
    SubjectPredicate = 5,
    /// <summary>
    /// Predicate-Object Index should be used
    /// </summary>
    PredicateObject = 3,
    /// <summary>
    /// Subject-Object Index should be used
    /// </summary>
    SubjectObject = 4,
}