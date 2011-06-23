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
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for parsing TriG (Turtle with Named Graphs) RDF Syntax into a Triple Store
    /// </summary>
    /// <remarks>The Default Graph (if any) will be given the special Uri <strong>trig:default-graph</strong></remarks>
    public class TriGParser : IStoreReader, ITraceableTokeniser
    {
        private bool _tracetokeniser = false;

        /// <summary>
        /// Default Graph Uri for default graphs parsed from TriG input
        /// </summary>
        public const String DefaultGraphURI = "trig:default-graph";

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
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
        /// Loads the named Graphs from the TriG input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="parameters">Parameters indicating the input to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Store");
            this.Load(new StoreHandler(store), parameters);
        }

        /// <summary>
        /// Loads the named Graphs from the TriG input using the given RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="parameters">Parameters indicating the input to read from</param>
        public void Load(IRdfHandler handler, IStoreParams parameters)
        {
            if (handler == null) throw new ArgumentNullException("handler", "Cannot parse an RDF Dataset using a null RDF Handler");
            if (parameters == null) throw new ArgumentNullException("parameters", "Cannot parse an RDF Dataset using null Parameters");

            //Try and get the Input from the parameters
            TextReader input = null;
            if (parameters is StreamParams)
            {
                //Get Input Stream
                input = ((StreamParams)parameters).StreamReader;

                //Issue a Warning if the Encoding of the Stream is not UTF-8
                if (!((StreamReader)input).CurrentEncoding.Equals(Encoding.UTF8))
                {
#if !SILVERLIGHT
                    this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + ((StreamReader)input).CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
#else
                this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + ((StreamReader)input).CurrentEncoding.GetType().Name + " - Please be aware that parsing errors may occur as a result");
#endif
                }
            } 
            else if (parameters is TextReaderParams)
            {
                input = ((TextReaderParams)parameters).TextReader;
            }

            if (input != null)
            {
                try
                {
                    //Create the Parser Context and Invoke the Parser
                    TriGParserContext context = new TriGParserContext(handler, new TriGTokeniser(input), TokenQueueMode.SynchronousBufferDuringParsing, false, this._tracetokeniser);
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
                        //No catch actions just cleaning up
                    }
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriGParser must be of the type StreamParams/TextReaderParams");
            }
        }

        private void Parse(TriGParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                //Expect a BOF Token
                IToken first = context.Tokens.Dequeue();
                if (first.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + first.GetType().ToString() + "' encountered, expected a BOF Token", first);
                }

                //Expect either a Directive or a Graph
                IToken next;
                do
                {
                    next = context.Tokens.Peek();
                    switch (next.TokenType)
                    {
                        case Token.COMMENT:
                            //Discard
                            context.Tokens.Dequeue();
                            break;
                        case Token.EOF:
                            //End of File
                            context.Tokens.Dequeue();
                            break;

                        case Token.BASEDIRECTIVE:
                        case Token.PREFIXDIRECTIVE:
                            //Parse a Directive
                            this.TryParseDirective(context);
                            break;

                        case Token.QNAME:
                        case Token.URI:
                        case Token.LEFTCURLYBRACKET:
                            //Parse a Graph
                            this.TryParseGraph(context);
                            break;

                        default:
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                    }
                } while (next.TokenType != Token.EOF);
                
                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseDirective(TriGParserContext context)
        {
            //See what type of directive it is
            IToken directive = context.Tokens.Dequeue();

            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                //Base Directives Invalid in TriG
                throw ParserHelper.Error("The Base Directive is not a valid in TriG",directive);
            }
            else if (directive.TokenType == Token.PREFIXDIRECTIVE)
            {
                //Prefix Directive
                IToken prefix = context.Tokens.Dequeue();
                if (prefix.TokenType == Token.PREFIX)
                {
                    IToken uri = context.Tokens.Dequeue();
                    if (uri.TokenType == Token.URI)
                    {
                        //Ensure the Uri is absolute
                        try
                        {
                            Uri u = new Uri(uri.Value, UriKind.Absolute);
                            String pre = (prefix.Value.Equals(":")) ? String.Empty : prefix.Value.Substring(0, prefix.Value.Length-1);
                            context.Namespaces.AddNamespace(pre, u);
                            if (!context.Handler.HandleNamespace(pre, u)) ParserHelper.Stop();

                            //Expect a DOT to Terminate
                            IToken dot = context.Tokens.Dequeue();
                            if (dot.TokenType != Token.DOT)
                            {
                                throw ParserHelper.Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate a Prefix Directive", dot);
                            }
                        }
                        catch (UriFormatException)
                        {
                            throw ParserHelper.Error("The URI '" + uri.Value + "' given for the prefix '" + prefix.Value + "' is not a valid Absolute URI", uri);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a URI Token after a Prefix Token", uri);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a Prefix Token after a Prefix Directive Token", prefix);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix Directive Token", directive);
            }
        }

        private void TryParseGraph(TriGParserContext context)
        {
            //Is there a name for the Graph?
            IToken next = context.Tokens.Dequeue();
            Uri graphUri;
            if (next.TokenType == Token.QNAME)
            {
                //Try to resolve the QName
                graphUri = new Uri(Tools.ResolveQName(next.Value, context.Namespaces, null));

                //Get the Next Token
                next = context.Tokens.Dequeue();
            }
            else if (next.TokenType == Token.URI)
            {
                try
                {
                    //Ensure an absolute Uri
                    graphUri = new Uri(next.Value, UriKind.Absolute);
                }
                catch (UriFormatException)
                {
                    throw ParserHelper.Error("The URI '" + next.Value + "' given as a Graph Name is not a valid Absolute URI", next);
                }

                //Get the Next Token
                next = context.Tokens.Dequeue();
            }
            else
            {
                //No Name so is a Default Graph
                if (!context.DefaultGraphExists)
                {
                    graphUri = null;
                }
                else
                {
                    throw new RdfParseException("You cannot specify more than one Default (Unnamed) Graph in a TriG file", next);
                }
            }

            //Is there a discardable Equals token?
            if (next.TokenType == Token.EQUALS)
            {
                next = context.Tokens.Dequeue();
            }

            //Should the see a Left Curly Bracket
            if (next.TokenType == Token.LEFTCURLYBRACKET)
            {
                //Check that the Graph isn't empty i.e. the next token is not a } to close the Graph
                next = context.Tokens.Peek();
                if (next.TokenType == Token.RIGHTCURLYBRACKET)
                {
                    //Empty Graph so just discard the }
                    context.Tokens.Dequeue();
                }
                else
                {
                    //Parse Graph Contents
                    this.TryParseTriples(context, graphUri);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Curly Bracket to start a Graph", next);
            }

            //May optionally end with a Dot Token
            next = context.Tokens.Peek();
            if (next.TokenType == Token.DOT)
            {
                //Discard
                context.Tokens.Dequeue();
            }
        }

        private void TryParseTriples(TriGParserContext context, Uri graphUri)
        {
            do
            {
                //Try to get the Subject
                IToken subj = context.Tokens.Dequeue();
                INode subjNode;

                //Turn the Subject Token into a Node
                switch (subj.TokenType)
                {
                    case Token.COMMENT:
                        //Discard and continue
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        subjNode = ParserHelper.TryResolveUri(context, subj);
                        break;

                    case Token.BLANKNODEWITHID:
                        //Blank Node with ID
                        subjNode = context.Handler.CreateBlankNode(subj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        //Blank Node
                        IToken next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                            subjNode = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            //Blank Node Collection
                            subjNode = context.Handler.CreateBlankNode();

                            //Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            this.TryParsePredicateObjectList(context, graphUri, subjNode);
                        }
                        break;

                    case Token.LEFTBRACKET:
                        //Collection

                        //Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection
                            context.Tokens.Dequeue();
                            subjNode = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            subjNode = context.Handler.CreateBlankNode();
                            this.TryParseCollection(context, graphUri, subjNode);
                        }
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Triples", subj);

                    default:
                        //Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + subj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Subject of a Triple", subj);
                }

                //Parse the Predicate Object List
                this.TryParsePredicateObjectList(context, graphUri, subjNode);

                //Expect a Dot to Terminate
                if (context.Tokens.LastTokenType != Token.DOT && context.Tokens.LastTokenType != Token.RIGHTCURLYBRACKET)
                {
                    //We only do this if we haven't returned because we already hit the Dot Token/Right Curly Bracket
                    IToken dot = context.Tokens.Dequeue();
                    if (dot.TokenType != Token.DOT && dot.TokenType != Token.RIGHTCURLYBRACKET)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate Triples", dot);
                    }
                }

                //If we already hit the Right Curly Bracket return
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET) return;

            } while (context.Tokens.Peek().TokenType != Token.RIGHTCURLYBRACKET);

            //Discard the ending Right Curly Bracket
            context.Tokens.Dequeue();
        }

        private void TryParsePredicateObjectList(TriGParserContext context, Uri graphUri, INode subj)
        {
            bool ok = false;
            do
            {
                //After our first run through we'll need to discard semicolons here
                if (ok)
                {
                    context.Tokens.Dequeue();

                    //Watch out for Trailing Semicolons
                    if (context.Tokens.Peek().TokenType == Token.RIGHTSQBRACKET)
                    {
                        //Allow trailing semicolons to terminate Blank Node Collections
                        context.Tokens.Dequeue();
                        return;
                    }
                }

                //Try to get the Predicate
                IToken pred = context.Tokens.Dequeue();
                INode predNode;

                switch (pred.TokenType)
                {
                    case Token.COMMENT:
                        //Discard and continue
                        ok = false;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        predNode = ParserHelper.TryResolveUri(context, pred);
                        break;

                    case Token.KEYWORDA:
                        //'a' Keyword
                        predNode = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Predicate Object list", pred);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list was parsed", pred);
                        }
                        return;

                    case Token.RIGHTSQBRACKET:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list of a Blank Node Collection was parsed", pred);
                        }
                        return;

                    default:
                        //Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered, expected a URI/QName as the Predicate of a Triple", pred);
                }

                ok = true;

                //Parse the Object List
                this.TryParseObjectList(context, graphUri, subj, predNode);

                //Return if we hit the Dot Token/Right Curly Bracket/Right Square Bracket
                if (context.Tokens.LastTokenType == Token.DOT ||
                    context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET ||
                    context.Tokens.LastTokenType == Token.RIGHTSQBRACKET)
                {
                    return;
                }

                //Check for End of Blank Node Collections
                if (context.Tokens.Peek().TokenType == Token.RIGHTSQBRACKET)
                {
                    context.Tokens.Dequeue();
                    return;
                }

            } while (context.Tokens.Peek().TokenType == Token.SEMICOLON); //Expect a semicolon if we are to continue
        }

        private void TryParseObjectList(TriGParserContext context, Uri graphUri, INode subj, INode pred)
        {
            bool ok = false;

            do
            {
                //After the first run through we'll need to discard commas here
                if (ok)
                {
                    context.Tokens.Dequeue();
                }

                //Try to get the Object
                IToken obj = context.Tokens.Dequeue();
                IToken next;
                INode objNode;

                switch (obj.TokenType)
                {
                    case Token.COMMENT:
                        //Discard and Continue
                        ok = false;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        objNode = ParserHelper.TryResolveUri(context, obj);
                        break;

                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                        //Literals

                        //See whether we get a Language Specifier/Data Type next
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.LANGSPEC)
                        {
                            //Literal with Language Specifier
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateLiteralNode(obj.Value, next.Value);
                        }
                        else if (next.TokenType == Token.HATHAT)
                        {
                            //Literal with DataType
                            context.Tokens.Dequeue();
                            //Now expect a QName/Uri Token
                            next = context.Tokens.Dequeue();
                            if (next.TokenType == Token.QNAME || next.TokenType == Token.URI)
                            {
                                Uri dt = new Uri(Tools.ResolveUriOrQName(next, context.Namespaces, context.BaseUri));
                                objNode = context.Handler.CreateLiteralNode(obj.Value, dt);
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName Token to specify a Data Type after a ^^ Token", next);
                            }
                        }
                        else
                        {
                            //Just a string literal
                            objNode = context.Handler.CreateLiteralNode(obj.Value);
                        }
                        break;

                    case Token.PLAINLITERAL:
                        //Plain Literals
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)obj);
                        objNode = context.Handler.CreateLiteralNode(obj.Value, plt);
                        break;

                    case Token.BLANKNODEWITHID:
                        //Blank Node with ID
                        objNode = context.Handler.CreateBlankNode(obj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        //Blank Node
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            //Blank Node Collection
                            objNode = context.Handler.CreateBlankNode();

                            //Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            this.TryParsePredicateObjectList(context, graphUri, objNode);
                        }
                        break;

                    case Token.RIGHTSQBRACKET:
                        //End of Blank Node Collection
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list of a Blank Node Collection was parsed", obj);
                        }
                        return;

                    case Token.LEFTBRACKET:
                        //Collection

                        //Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            objNode = context.Handler.CreateBlankNode();
                            this.TryParseCollection(context, graphUri, objNode);
                        }
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Object List", obj);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                    case Token.SEMICOLON:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list was parsed", obj);
                        }
                        return;

                    default:
                        //Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Object of a Triple", obj);
                }

                ok = true;

                if (!context.Handler.HandleTriple(new Triple(subj, pred, objNode, graphUri))) ParserHelper.Stop();

            } while (context.Tokens.Peek().TokenType == Token.COMMA); //Expect a comma if we are to continue
        }

        private void TryParseCollection(TriGParserContext context, Uri graphUri, INode subj)
        {
            //Create the Nodes we need
            IUriNode rdfFirst, rdfRest, rdfNil;
            rdfFirst = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            rdfRest = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            rdfNil = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));

            IToken next;
            INode item, temp;
            item = null;
            do
            {
                next = context.Tokens.Dequeue();

                //Create a Node for this Token
                switch (next.TokenType)
                {
                    case Token.COMMENT:
                        //Discard and continue;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        item = ParserHelper.TryResolveUri(context, next);
                        break;

                    case Token.LITERAL:
                    case Token.LONGLITERAL:

                        break;
                    case Token.PLAINLITERAL:
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)next);
                        item = context.Handler.CreateLiteralNode(next.Value, plt);
                        break;

                    case Token.LEFTSQBRACKET:
                        //Check whether an anonymous Blank Node or a Blank Node Collection
                        item = context.Handler.CreateBlankNode();

                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                        }
                        else
                        {
                            //Blank Node Collection
                            this.TryParsePredicateObjectList(context, graphUri, item);
                        }
                        break;

                    case Token.LEFTBRACKET:
                        //Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection
                            context.Tokens.Dequeue();
                            item = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            item = context.Handler.CreateBlankNode();
                            this.TryParseCollection(context, graphUri, item);
                        }
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse a Collection", next);

                    default:
                        //Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName/Literal/Blank Node as an item in a Collection", next);
                }

                //Create the subj rdf:first item Triple
                if (!context.Handler.HandleTriple((new Triple(subj, rdfFirst, item, graphUri)))) ParserHelper.Stop();

                //Create the rdf:rest Triple
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    //End of Collection
                    context.Tokens.Dequeue();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil, graphUri))) ParserHelper.Stop();
                    return;
                }
                else
                {
                    //Continuing Collection
                    temp = context.Handler.CreateBlankNode();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, temp, graphUri))) ParserHelper.Stop();
                    subj = temp;
                }
            } while (true);

        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            StoreReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event StoreReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TriG";
        }
    }
}
