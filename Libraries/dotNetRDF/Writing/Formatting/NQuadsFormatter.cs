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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter which formats Triples as NQuads adding an additional URI at the end of the Triple if there is a Graph URI associated with the Triple
    /// </summary>
    public class NQuadsFormatter
        : NTriplesFormatter
    {
        /// <summary>
        /// Creates a new NQuads Formatter
        /// </summary>
        public NQuadsFormatter()
            : this(NQuadsSyntax.Original, GetName()) { }

        /// <summary>
        /// Creates a new NQuads formatter
        /// </summary>
        /// <param name="syntax">NQuads syntax to output</param>
        public NQuadsFormatter(NQuadsSyntax syntax)
            : this(syntax, GetName(syntax)) { }

        /// <summary>
        /// Creates a new NQuads formatter
        /// </summary>
        /// <param name="syntax">NQuads syntax to output</param>
        /// <param name="formatName">Format Name</param>
        public NQuadsFormatter(NQuadsSyntax syntax, String formatName)
            : base(NQuadsParser.AsNTriplesSyntax(syntax), formatName) { }

        private static String GetName()
        {
            return GetName(NQuadsSyntax.Original);
        }

        private static string GetName(NQuadsSyntax syntax)
        {
            switch (syntax)
            {
                case NQuadsSyntax.Original:
                    return "NQuads";
                default:
                    return "NQuads (RDF 1.1)";
            }
        }

        /// <summary>
        /// Formats a Triple as a String
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override string Format(Triple t)
        {
            if (t.GraphUri == null)
            {
                return base.Format(t);
            }
            return Format(t.Subject, TripleSegment.Subject) + " " + Format(t.Predicate, TripleSegment.Predicate) + " " + Format(t.Object, TripleSegment.Object) + " <" + FormatUri(t.GraphUri) + "> .";
        }
    }

    /// <summary>
    /// Formatter which formats Triples as NQuads according to the RDF 1.1 NQuads specification
    /// </summary>
    public class NQuads11Formatter
        : NQuadsFormatter
    {
        /// <summary>
        /// Creates a new formatter
        /// </summary>
        public NQuads11Formatter()
            : base(NQuadsSyntax.Rdf11) { }
    }
}