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

using System.IO;
using System.Text;

namespace VDS.RDF;

/// <summary>
/// Interface to be implemented by Triple Store Writers.
/// </summary>
public interface IStoreWriter
{
    /// <summary>
    /// Writes the RDF content of the triple store to the specified file.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    /// <param name="filename">File to save to.</param>
    /// <remarks>The output file will use the UTF-8 text encoding with no byte-order mark.</remarks>
    void Save(ITripleStore store, string filename);

    /// <summary>
    /// Writes the content of the triple store to the specified file using the specified text encoding.
    /// </summary>
    /// <param name="store">The store whose content is to be written.</param>
    /// <param name="filename">The path to the file to write the store content to.</param>
    /// <param name="fileEncoding">The text encoding to use for the output file.</param>
    void Save(ITripleStore store, string filename, Encoding fileEncoding);

    /// <summary>
    /// Method for saving data to a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    /// <param name="output">Write to save to.</param>
    void Save(ITripleStore store, TextWriter output);

    /// <summary>
    /// Method for saving data to a Triple Store.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    /// <param name="output">Write to save to.</param>
    /// <param name="leaveOpen">Boolean flag indicating if the output writer should be left open by the writer when it completes.</param>
    void Save(ITripleStore store, TextWriter output, bool leaveOpen);

    /// <summary>
    /// Event which writers can raise to indicate possible ambiguities or issues in the syntax they are producing
    /// </summary>
    event StoreWriterWarning Warning;
}
