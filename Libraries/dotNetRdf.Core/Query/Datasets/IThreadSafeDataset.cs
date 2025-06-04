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

using System.Threading;

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// Interface for SPARQL Datasets which also provide a Lock by which threading can be controlled.
/// </summary>
/// <remarks>
/// Note that there is no guarantees that consuming code will respect the fact that a Dataset is Thread Safe and use the <see cref="IThreadSafeDataset.Lock">Lock</see> property appropriately.  Additionally some datasets may choose to implement thread safety in other ways which don't rely on this interface.
/// </remarks>
public interface IThreadSafeDataset : ISparqlDataset
{
    /// <summary>
    /// Gets the Lock used to ensure MRSW concurrency of the Dataset when used with the Leviathan SPARQL processors.
    /// </summary>
    ReaderWriterLockSlim Lock
    {
        get;
    }
}