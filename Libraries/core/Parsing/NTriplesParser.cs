/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for NTriples syntax which is designed specifically for NTriples
    /// </summary>
    /// <remarks>The <see cref="NTriplesParser">NTriplesParser</see> class can also be used which is a wrapper to a <see cref="TurtleParser">TurtleParser</see> which can be restricted to only parse NTriples syntax.  This Native Parser should be faster since it uses a NTriples only Tokeniser as opposed to the Turtle Tokeniser which will Tokenise syntax which is invalid in NTriples.  The NTriples specific Tokeniser is able to reject this syntax at the Tokeniser stage whereas the Turtle based parser has to reject at the Parser stage which is potentially slower.</remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class NTriplesParser : IRdfReader, ITraceableParser, ITraceableTokeniser
    {
        #region Initialisation, Variables and Properties

        private bool _tracetokeniser = false;
        private bool _traceparsing = false;
        private TokenQueueMode _queuemode = TokenQueueMode.QueueAllBeforeParsing;

        /// <summary>
        /// Creates a new Instance of the Parser
        /// </summary>
        public NTriplesParser()
        {
        }

        /// <summary>
        /// Creates a new Instance of the Parser using the given Token Queue Mode
        /// </summary>
        /// <param name="qmode">Token Queue Mode</param>
        public NTriplesParser(TokenQueueMode qmode)
        {
            this._queuemode = qmode;
        }

        /// <summary>
        /// Controls whether Tokeniser progress will be traced by writing output to the Console
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._tracetokeniser;
            }
            set
            {
                this._tracetokeniser = value;
            }
        }

        /// <summary>
        /// Controls whether Parser progress will be traced by writing output to the Console
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceparsing;
            }
            set
            {
                this._traceparsing = value;
            }
        }

        #endregion

        /// <summary>
        /// Parses NTriples Syntax from the given Input Stream into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Arbitrary Input Stream to read input from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Arbitrary Input to read input from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given File into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="filename">Name of the file containing Turtle Syntax</param>
        /// <remarks>Simply opens an StreamReader and uses the overloaded version of this function</remarks>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");

            //Can only open Streams as ASCII when not running under Silverlight as Silverlight has no ASCII support
#if !SILVERLIGHT
            StreamReader input = new StreamReader(filename, Encoding.ASCII);
#else
            StreamReader input = new StreamReader(filename);
            this.RaiseWarning("NTriples files are ASCII format but Silverlight does not support ASCII - will open as UTF-8 instead which may cause issues");
#endif
            this.Load(g, input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input Stream using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input Stream to read input from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

#if !SILVERLIGHT
            //Issue a Warning if the Encoding of the Stream is not ASCII
            if (!input.CurrentEncoding.Equals(Encoding.ASCII))
            {
                this.RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }
#endif

            this.Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read input from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                TokenisingParserContext context = new TokenisingParserContext(handler, new NTriplesTokeniser(input), this._queuemode, this._traceparsing, this._tracetokeniser);
                this.Parse(context);
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //Catch is just here in case something goes wrong with closing the stream
                    //This error can be ignored
                }
            }
        }

        /// <summary>
        /// Parses NTriples Syntax from the given file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            this.Load(handler, new StreamReader(filename, Encoding.UTF8));
        }

        private void Parse(TokenisingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                //Initialise the Buffer
                context.Tokens.InitialiseBuffer(10);

                //Expect a BOF
                IToken start = context.Tokens.Dequeue();
                if (start.TokenType != Token.BOF)
                {
                    throw Error("Unexpected Token '" + start.GetType().ToString() + "' encountered, expected a Beginning of File Token", start);
                }

                //Expect Triples
                IToken next = context.Tokens.Peek();
                while (next.TokenType != Token.EOF)
                {
                    //Discard Comments
                    while (next.TokenType == Token.COMMENT)
                    {
                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                    }
                    if (next.TokenType == Token.EOF) break;

                    this.TryParseTriple(context);

                    next = context.Tokens.Peek();
                }

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch (RdfParseException)
            {
                //We hit some Parsing error
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseTriple(TokenisingParserContext context)
        {
            //Get the Subject, Predicate and Object
            INode subj = this.TryParseSubject(context);
            INode pred = this.TryParsePredicate(context);
            INode obj = this.TryParseObject(context);

            //Ensure we're terminated by a DOT
            this.TryParseLineTerminator(context);

            //Assert the Triple
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
        }

        private INode TryParseSubject(TokenisingParserContext context)
        {
            IToken subjToken = context.Tokens.Dequeue();

            //Discard Comments
            while (subjToken.TokenType == Token.COMMENT)
            {
                subjToken = context.Tokens.Dequeue();
            }

            switch (subjToken.TokenType)
            {
                case Token.BLANKNODE:
                    return context.Handler.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return context.Handler.CreateBlankNode(subjToken.Value.Substring(2));
                case Token.URI:
                    return context.Handler.CreateUriNode(new Uri(subjToken.Value));
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw Error("Subject cannot be a Literal in NTriples", subjToken);
                default:
                    throw Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, expected a Blank Node or URI for the Subject of a Triple", subjToken);
            }
        }

        private INode TryParsePredicate(TokenisingParserContext context)
        {
            IToken predToken = context.Tokens.Dequeue();

            //Discard Comments
            while (predToken.TokenType == Token.COMMENT)
            {
                predToken = context.Tokens.Dequeue();
            }

            switch (predToken.TokenType)
            {
                case Token.BLANKNODE:
                case Token.BLANKNODEWITHID:
                    throw Error("Predicate cannot be a Blank Node in NTriples", predToken);
                case Token.URI:
                    return context.Handler.CreateUriNode(new Uri(predToken.Value));
                    //return this.ConvertToNode(g, predToken);
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw Error("Predicate cannot be a Literal in NTriples", predToken);
                default:
                    throw Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered, expected a URI for the Predicate of a Triple", predToken);
            }
        }

        private INode TryParseObject(TokenisingParserContext context)
        {
            IToken objToken = context.Tokens.Dequeue();
            String dt;

            //Discard Comments
            while (objToken.TokenType == Token.COMMENT)
            {
                objToken = context.Tokens.Dequeue();
            }

            switch (objToken.TokenType)
            {
                case Token.BLANKNODE:
                    return context.Handler.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return context.Handler.CreateBlankNode(objToken.Value.Substring(2));
                case Token.URI:
                    return context.Handler.CreateUriNode(new Uri(objToken.Value));
                case Token.LITERALWITHDT:
                    dt = ((LiteralWithDataTypeToken)objToken).DataType;
                    dt = dt.Substring(1,dt.Length-2);
                    return context.Handler.CreateLiteralNode(objToken.Value, new Uri(dt));
                case Token.LITERALWITHLANG:
                    return context.Handler.CreateLiteralNode(objToken.Value, ((LiteralWithLanguageSpecifierToken)objToken).Language);
                case Token.LITERAL:
                    IToken next = context.Tokens.Peek();
                    //Is there a Language Specifier or Data Type?
                    if (next.TokenType == Token.LANGSPEC)
                    {
                        context.Tokens.Dequeue();
                        return context.Handler.CreateLiteralNode(objToken.Value, next.Value);
                    }
                    else if (next.TokenType == Token.URI)
                    {
                        context.Tokens.Dequeue();
                        return context.Handler.CreateLiteralNode(objToken.Value, new Uri(Tools.ResolveUriOrQName(next, context.Namespaces, context.BaseUri)));
                    }
                    else
                    {
                        return context.Handler.CreateLiteralNode(objToken.Value);
                    }
                    
                default:
                    throw Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered, expected a Blank Node, Literal or URI for the Object of a Triple", objToken);
            }
        }

        private void TryParseLineTerminator(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();

            //Discard Comments
            while (next.TokenType == Token.COMMENT)
            {
                next = context.Tokens.Dequeue();
            }

            //Ensure we finish with a Dot terminator
            if (next.TokenType != Token.DOT)
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Triple", next);
            }
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="t">The Token that is the cause of the Error</param>
        /// <returns></returns>
        private RdfParseException Error(String msg, IToken t)
        {
            StringBuilder output = new StringBuilder();
            output.Append("[");
            output.Append(t.GetType().Name);
            output.Append(" at Line ");
            output.Append(t.StartLine);
            output.Append(" Column ");
            output.Append(t.StartPosition);
            output.Append(" to Line ");
            output.Append(t.EndLine);
            output.Append(" Column ");
            output.Append(t.EndPosition);
            output.Append("] ");
            output.Append(msg);

            return new RdfParseException(output.ToString(), t);
        }

        /// <summary>
        /// Internal Helper method which raises the Warning event if an event handler is registered to it
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (this.Warning == null)
            {
                //Do Nothing
            }
            else
            {
                //Raise Event
                this.Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the NTriples being parsed
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NTriples";
        }
    }
}
