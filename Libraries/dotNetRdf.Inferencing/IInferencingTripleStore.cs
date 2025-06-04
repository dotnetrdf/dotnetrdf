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

using VDS.RDF.Query.Inference;

namespace VDS.RDF;

/// <summary>
/// Interface for Triple Stores which can have a <see cref="IInferenceEngine">IInferenceEngine</see> attached to them.
/// </summary>
public interface IInferencingTripleStore
    : ITripleStore
{
    /// <summary>
    /// Adds an Inference Engine to the Triple Store.
    /// </summary>
    /// <param name="reasoner">Reasoner to add.</param>
    void AddInferenceEngine(IInferenceEngine reasoner);

    /// <summary>
    /// Removes an Inference Engine from the Triple Store.
    /// </summary>
    /// <param name="reasoner">Reasoner to remove.</param>
    void RemoveInferenceEngine(IInferenceEngine reasoner);

    /// <summary>
    /// Clears all Inference Engines from the Triple Store.
    /// </summary>
    void ClearInferenceEngines();

    /// <summary>
    /// Applies Inference to the given Graph.
    /// </summary>
    /// <param name="g">Graph to apply inference to.</param>
    /// <remarks>
    /// Allows you to apply Inference to a Graph even if you're not putting that Graph into the Store.
    /// </remarks>
    void ApplyInference(IGraph g);
}