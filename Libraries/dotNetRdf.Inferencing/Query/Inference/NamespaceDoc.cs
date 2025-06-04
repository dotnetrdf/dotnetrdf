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

using System.Runtime.CompilerServices;

namespace VDS.RDF.Query.Inference;

/// <summary>
/// <para>
/// Namespace for Inference Classes which provide Inferencing capabilities on RDF - these features are currently experimental and may not work as expected.
/// </para>
/// <para>
/// Classes which implement reasoning must implement the <see cref="IInferenceEngine">IInferenceEngine</see> interface, these can then be attached to classes which implement the <see cref="IInferencingTripleStore">IInferencingTripleStore</see> interface or they can be used to apply inference to any <see cref="IGraph">IGraph</see> implementation with the inferred Triples optionally output to a separate Graph.
/// </para>
/// <para>
/// OWL reasoning currently has extremely limited support.
/// </para>
/// </summary>
[CompilerGenerated]
class NamespaceDoc
{

}