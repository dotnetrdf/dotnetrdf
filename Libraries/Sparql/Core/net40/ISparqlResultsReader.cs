/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using VDS.RDF.Query.Results;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Reader Classes which parser SPARQL Result Set serializations
    /// </summary>
    public interface ISparqlResultsReader
    {
        /// <summary>
        /// Reads SPARQL results passing them to the given results handler
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="input">Input to read from</param>
        /// <param name="profile">Parser profile to use</param>
        void Load(ISparqlResultsHandler handler, TextReader input, IParserProfile profile);

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        event SparqlWarning Warning;
    }

    /// <summary>
    /// Interface for readers which are capable of streaming the results
    /// </summary>
    public interface IStreamingSparqlResultsReader
        : ISparqlResultsReader
    {
        /// <summary>
        /// Streams a result set from the given input
        /// </summary>
        /// <param name="input">Input to stream from</param>
        /// <param name="profile">Parser profile to use</param>
        /// <returns>Results stream</returns>
        IResultStream Stream(TextReader input, IParserProfile profile);
    }
}
