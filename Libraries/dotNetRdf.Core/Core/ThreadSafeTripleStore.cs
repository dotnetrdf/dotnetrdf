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
/// A thread safe variant of <see cref="TripleStore"/>, simply a <see cref="TripleStore"/> instance with a <see cref="ThreadSafeGraphCollection"/> decorator around it's underlying <see cref="BaseGraphCollection"/>.
/// </summary>
public class ThreadSafeTripleStore
    : TripleStore
{
    /// <summary>
    /// Creates a new Thread Safe triple store.
    /// </summary>
    public ThreadSafeTripleStore()
        : base(new ThreadSafeGraphCollection()) { }

    /// <summary>
    /// Creates a new Thread safe triple store using the given Thread safe graph collection.
    /// </summary>
    /// <param name="collection">Collection.</param>
    public ThreadSafeTripleStore(ThreadSafeGraphCollection collection)
        : base(collection) { }

    /// <summary>
    /// Creates a new Thread safe triple store using a thread safe decorator around the given graph collection.
    /// </summary>
    /// <param name="collection">Collection.</param>
    public ThreadSafeTripleStore(BaseGraphCollection collection)
        : this(new ThreadSafeGraphCollection(collection)) { }
}