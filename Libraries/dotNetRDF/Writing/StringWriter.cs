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
using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Static Helper class for the writing of RDF Graphs and SPARQL Result Sets to Strings rather than Streams/Files
    /// </summary>
    public static class StringWriter
    {
        /// <summary>
        /// Writes the Graph to a String and returns the output in your chosen concrete RDF Syntax
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="writer">Writer to use to generate the concrete RDF Syntax</param>
        /// <returns></returns>
        /// <remarks>
        /// Since the API allows for any <see cref="TextWriter">TextWriter</see> to be passed to the <see cref="IRdfWriter.Save(IGraph, TextWriter)">Save()</see> method of a <see cref="IRdfWriter">IRdfWriter</see> you can just pass in a <see cref="StringWriter">StringWriter</see> to the Save() method to get the output as a String.  This method simply provides a wrapper to doing just that.
        /// </remarks>
        public static String Write(IGraph g, IRdfWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(g, sw);

            return sw.ToString();
        }

        /// <summary>
        /// Writes the given Triple Store to a String and returns the output in your chosen concrete RDF dataset syntax
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="writer">Writer to use to generate conrete RDF Syntax</param>
        /// <returns></returns>
        public static String Write(ITripleStore store, IStoreWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(store, sw);

            return sw.ToString();
        }

        /// <summary>
        /// Writes the SPARQL Result Set to a String and returns the Output in your chosen format
        /// </summary>
        /// <param name="results">SPARQL Result Set</param>
        /// <param name="writer">Writer to use to generate the SPARQL Results output</param>
        /// <returns></returns>
        public static String Write(SparqlResultSet results, ISparqlResultsWriter writer)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            writer.Save(results, sw);

            return sw.ToString();
        }
    }
}
