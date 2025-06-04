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
/// Interface for Triple Stores which support SPARQL Update as per the SPARQL 1.1 specifications.
/// </summary>
/// <remarks>
/// <para>
/// A Store which supports this may implement various access control mechanisms which limit what operations are actually permitted.
/// </para>
/// <para>
/// It is the responsibility of the Store class to ensure that commands are permissible before invoking them.
/// </para>
/// </remarks>
public interface IUpdateableTripleStore
    : ITripleStore
{
    /// <summary>
    /// Executes an Update against the Triple Store.
    /// </summary>
    /// <param name="update">SPARQL Update Command(s).</param>
    /// <remarks>
    /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands.
    /// </remarks>
    void ExecuteUpdate(string update);

    
}