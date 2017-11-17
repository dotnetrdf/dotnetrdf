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
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for reading SPARQL Results that have been serialized in the SPARQL Results CSV format
    /// </summary>
    public class SparqlCsvParser
        : ISparqlResultsReader
    {
        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Results from a null input stream");
            
            // Check Encoding
            if (input.CurrentEncoding != Encoding.UTF8)
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            Load(results, (TextReader)input);
        }

        /// <summary>
        /// Loads a Result Set from a File
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse SPARQL Results from a null file");
            Load(results, new StreamReader(File.OpenRead(filename)));
        }

        /// <summary>
        /// Loads a Result Set from an Input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Set");
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Results from a null input stream");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parser SPARQL Results from a null input stream");

            // Check Encoding
            if (input.CurrentEncoding != Encoding.UTF8)
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads a Result Set from a File using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="filename">Filename to load from</param>
        public void Load(ISparqlResultsHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse SPARQL Results from a null file");
            Load(handler, new StreamReader(File.OpenRead(filename)));
        }

        /// <summary>
        /// Loads a Result Set from an Input using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse SPARQL Results into a null Result Handler");
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Results from a null input stream");

            try
            {
                TokenisingResultParserContext context = new TokenisingResultParserContext(handler, new CsvTokeniser(ParsingTextReader.Create(input)));
                TryParseResults(context);
                input.Close();
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // No catch actions just trying to clean up
                }
                throw;
            }
        }

        private void TryParseResults(TokenisingResultParserContext context)
        {
            try
            {
                context.Handler.StartResults();
                context.Tokens.InitialiseBuffer();

                // Thrown away the BOF if present
                if (context.Tokens.Peek().TokenType == Token.BOF) context.Tokens.Dequeue();

                // Firstly parse the Header Row
                TryParseHeaderRow(context);

                // Then while not EOF try parse result rows
                IToken next = context.Tokens.Peek();
                while (next.TokenType != Token.EOF)
                {
                    TryParseResultRow(context);
                    if (context.Tokens.LastTokenType == Token.EOF) break;
                }

                context.Handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndResults(true);
            }
            catch
            {
                // Some Other Error
                context.Handler.EndResults(false);
                throw;
            }
        }

        private void TryParseHeaderRow(TokenisingResultParserContext context)
        {
            IToken next = context.Tokens.Peek();
            bool allowEOL = true, expectComma = false;
            while (true)
            {
                next = context.Tokens.Dequeue();
                switch (next.TokenType)
                {
                    case Token.EOL:
                        if (allowEOL)
                        {
                            break;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected End of Line, expected a Variable", next);
                        }

                    case Token.PLAINLITERAL:
                    case Token.LITERAL:
                        if (expectComma) throw ParserHelper.Error("Unexpected Variable, expected a comma between each Variable", next);
                        context.Variables.Add(next.Value);
                        if (!context.Handler.HandleVariable(next.Value)) ParserHelper.Stop();
                        allowEOL = true;
                        expectComma = true;
                        break;

                    case Token.COMMA:
                        expectComma = false;
                        allowEOL = false;
                        break;

                    case Token.EOF:
                        if (!allowEOL) throw ParserHelper.Error("Unexpected EOF, expected another Variable for the Header Row", next);
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                }

                // Stop when we've hit the End of the Line/File
                if (next.TokenType == Token.EOL || next.TokenType == Token.EOF) break;
            }
        }

        private void TryParseResultRow(TokenisingResultParserContext context)
        {
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.EOF)
            {
                context.Tokens.Dequeue();
                return;
            }

            bool allowEOL = true, expectComma = false;
            int v = 0;
            SparqlResult result = new SparqlResult();
            while (true)
            {
                next = context.Tokens.Dequeue();
                switch (next.TokenType)
                {

                    case Token.BLANKNODEWITHID:
                        if (expectComma) throw ParserHelper.Error("Unexpected Blank Node, expected a comma between RDF Terms", next);
                        if (v >= context.Variables.Count) throw ParserHelper.Error("Too many RDF Terms, only expecting " + context.Variables.Count + " terms", next);
                        INode blank = context.Handler.CreateBlankNode(next.Value.Substring(2));
                        result.SetValue(context.Variables[v], blank);
                        v++;
                        allowEOL = true;
                        expectComma = true;
                        break;

                    case Token.LITERAL:
                    case Token.PLAINLITERAL:
                        if (expectComma) throw ParserHelper.Error("Unexpected Blank Node, expected a comma between RDF Terms", next);
                        if (v >= context.Variables.Count) throw ParserHelper.Error("Too many RDF Terms, only expecting " + context.Variables.Count + " terms", next);
                        
                        // Try and guess what kind of term this is
                        String lexicalForm = next.Value;
                        INode value;
                        if (lexicalForm.StartsWith("http://") || lexicalForm.StartsWith("https://") || lexicalForm.StartsWith("mailto:") || lexicalForm.StartsWith("ftp://"))
                        {
                            try
                            {
                                // Guessing a URI if starts with common URI prefix
                                value = ParserHelper.TryResolveUri(context, next);
                            }
                            catch
                            {
                                // If invalid URI fall back to treating as literal
                                value = context.Handler.CreateLiteralNode(lexicalForm);
                            }
                        }
                        else
                        {
                            value = context.Handler.CreateLiteralNode(lexicalForm);
                        }
                        
                        result.SetValue(context.Variables[v], value);
                        v++;
                        allowEOL = true;
                        expectComma = true;
                        break;

                    case Token.EOL:
                        if (allowEOL)
                        {
                            break;
                        }
                        else
                        {
                            if (v == context.Variables.Count - 1)
                            {
                                // If this is the last expected term then this must be an empty term
                                v++;
                                break;
                            }
                            throw ParserHelper.Error("Unexpected End of Line, expected a RDF Term Token", next);
                        }

                    case Token.COMMA:
                        if (!expectComma)
                        {
                            // This is an empty field
                            if (v >= context.Variables.Count) throw ParserHelper.Error("Too many RDF Terms, only expecting " + context.Variables.Count + " terms", next);
                            v++;
                        }
                        expectComma = false;
                        allowEOL = false;
                        break;

                    case Token.EOF:
                        if (!allowEOL) throw ParserHelper.Error("Unexpected EOF, expected another RDF Term for the Result Row", next);
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                 }

                // Stop when we've hit the End of the Line/File
                if (next.TokenType == Token.EOL || next.TokenType == Token.EOF) break;
            }

            result.SetVariableOrdering(context.Variables);
            if (!context.Handler.HandleResult(result)) ParserHelper.Stop();
        }

        /// <summary>
        /// Event which is raised when the parser encounters a non-fatal issue with the syntax being parsed
        /// </summary>
        public event SparqlWarning Warning;

        private void RaiseWarning(String message)
        {
            SparqlWarning d = Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Gets the String representation of the Parser
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL CSV Results";
        }
    }
}
