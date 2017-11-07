/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Writing
{
    /// <summary>
    /// A convenience wrapper that allows a single graph to be written as the default
    /// graph using a store writer.
    /// </summary>
    public class SingleGraphWriter: IRdfWriter
    {
        private readonly IStoreWriter _storeWriter;

        /// <summary>
        /// Create a new writer instance that wraps the specified <see cref="IStoreWriter"/> instance.
        /// </summary>
        /// <param name="storeWriter">The <see cref="IStoreWriter"/> instance that will do the writing</param>
        public SingleGraphWriter(IStoreWriter storeWriter)
        {
            _storeWriter = storeWriter;
            _storeWriter.Warning += RaiseGraphWriterWarning;
        }

        private void RaiseGraphWriterWarning(string message)
        {
            Warning?.Invoke(message);
        }

        /// <inheritdoc />
        public void Save(IGraph g, string filename)
        {
            _storeWriter.Save(g.AsTripleStore(), filename);
        }

        /// <inheritdoc />
        public void Save(IGraph g, TextWriter output)
        {
            _storeWriter.Save(g.AsTripleStore(), output);
        }

        /// <inheritdoc />
        public void Save(IGraph g, TextWriter output, bool leaveOpen)
        {
            _storeWriter.Save(g.AsTripleStore(), output, leaveOpen);
        }

        /// <inheritdoc/>
        public event RdfWriterWarning Warning;
    }
}
