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

using System;
using System.IO;
using System.Text;

namespace VDS.RDF.Writing;

/// <summary>
/// Abstract base class for store writers that provides the default logic for implementing the Save method overrides.
/// </summary>
public abstract class BaseStoreWriter : IStoreWriter
{
    /// <inheritdoc />
    public void Save(ITripleStore store, string filename)
    {
        Save(store, filename,
#pragma warning disable CS0618 // Type or member is obsolete
                new UTF8Encoding(Options.UseBomForUtf8) //new UTF8Encoding(false)
#pragma warning restore CS0618 // Type or member is obsolete
            );
    }

    /// <inheritdoc />
    public void Save(ITripleStore store, string filename, Encoding fileEncoding)
    {
        if (store == null) throw new ArgumentNullException(nameof(store), "Cannot output a null ITripleStore instance.");
        if (filename == null) throw new ArgumentNullException(nameof(filename), "Cannot output to a null file");
        using FileStream stream = File.Open(filename, FileMode.Create);
        Save(store, new StreamWriter(stream, fileEncoding), false);
    }

    /// <inheritdoc />
    public void Save(ITripleStore store, TextWriter output)
    {
        if (store == null) throw new ArgumentNullException(nameof(store), "Cannot output a null ITripleStore instance.");
        if (output == null) throw new ArgumentNullException(nameof(output), "The output TextWriter must not be null.");
        Save(store, output, false);
    }

    /// <inheritdoc />
    public abstract void Save(ITripleStore store, TextWriter writer, bool leaveOpen);

    /// <inheritdoc />
    public abstract event StoreWriterWarning Warning;
}