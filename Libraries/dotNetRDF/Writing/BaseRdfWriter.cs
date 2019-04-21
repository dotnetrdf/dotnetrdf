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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Base implementation of <see cref="IRdfWriter"/> that simply handles the logic of optionally closing a text writer stream.
    /// </summary>
    public abstract class BaseRdfWriter : IRdfWriter
    {
        /// <inheritdoc/>
        public abstract event RdfWriterWarning Warning;

        /// <inheritdoc/>
        public abstract void Save(IGraph g, string filename);

        /// <inheritdoc/>
        public void Save(IGraph g, TextWriter output)
        {
            Save(g, output, false);
        }

        /// <inheritdoc/>
        public void Save(IGraph g, TextWriter output, bool leaveOpen) {
            try
            {
                SaveInternal(g, output);
                if (!leaveOpen) output.Close();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                {
                    try
                    {
                        output.Close();
                    }
                    catch (Exception)
                    {
                        // No handling, just clean up
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Method to be implemented in derived classes to perform the actual writing to a TextWriter
        /// </summary>
        /// <param name="graph">The graph to be saved</param>
        /// <param name="output">The <see cref="TextWriter"/> to save the graph to.</param>
        protected abstract void SaveInternal(IGraph graph, TextWriter output);

        
    }
}
