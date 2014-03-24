/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2014 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.IO;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Interface to be implemented by RDF Readers which parse Concrete RDF Syntax
    /// </summary>
    public interface IRdfReader
    {
        /// <summary>
        /// Loads RDF from some RDF format (or RDFizable format) from some input source for which a <see cref="TextReader"/> is provided
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">The reader to read input from</param>
        /// <param name="profile">Parser profile to use</param>
        /// <exception cref="RdfException">Typically thrown if the parser tries to output something that is invalid RDF, should be rare since these should normally be treated as a <see cref="RdfParseException"/> instead</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the parser cannot parse the given input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the parser encounters an IO Error while trying to access the input</exception>
        void Load(IRdfHandler handler, TextReader input, IParserProfile profile);

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        event RdfReaderWarning Warning;
    }
}