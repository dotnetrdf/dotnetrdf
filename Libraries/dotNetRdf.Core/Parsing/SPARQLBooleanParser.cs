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
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;

/// <summary>
/// Parser for SPARQL Boolean results as Plain Text.
/// </summary>
public class SparqlBooleanParser : BaseSparqlResultsReader
{
    /// <summary>
    /// Loads a Result Set from an Input Stream.
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="input">Input Stream to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, StreamReader input, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(new ResultSetHandler(results), input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from an Input Stream.
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="filename">File to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, string filename, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(results, new StreamReader(File.OpenRead(filename)), uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from an Input. 
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="input">Input to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, TextReader input, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(new ResultSetHandler(results), input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from an Input using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="input">Input to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, TextReader input, IUriFactory uriFactory)
    {
        if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
        if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Input");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

        try
        {
            Parse(new BaseResultsParserContext(handler, uriFactory), input);
        }
        finally
        {
            try
            {
                input.Close();
            }
            catch
            {
                // No Catch just cleaning up
            }
        }
    }

    /// <summary>
    /// Loads a Result Set from an Input Stream using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="input">Input Stream to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, StreamReader input, IUriFactory uriFactory)
    {
        Load(handler, (TextReader)input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from a file using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="filename">File to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, string filename, IUriFactory uriFactory)
    {
        if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(handler, new StreamReader(File.OpenRead(filename)), uriFactory);
    }

    private void Parse(IResultsParserContext context, TextReader input)
    {
        Parse(context, input.ReadToEnd());
    }

    private void Parse(IResultsParserContext context, string data)
    {
        try
        {
            context.Handler.StartResults();

            if (bool.TryParse(data.Trim(), out var result))
            {
                context.Handler.HandleBooleanResult(result);
            }
            else
            {
                throw new RdfParseException("The input was not a single boolean value as a String");
            }

            context.Handler.EndResults(true);
        }
        catch (RdfParsingTerminatedException)
        {
            context.Handler.EndResults(true);
        }
        catch
        {
            context.Handler.EndResults(false);
            throw;
        }
    }

    /// <summary>
    /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message)
    {
        SparqlWarning d = Warning;
        if (d != null)
        {
            d(message);
        }
    }

    /// <summary>
    /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
    /// </summary>
    public override event SparqlWarning Warning;

    /// <summary>
    /// Gets the String representation of the Parser which is a description of the syntax it parses.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "SPARQL Boolean Result";
    }
}
