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
        /// <param name="parameters">Parameters indicating the Stream to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is StreamParams)
            {
                //Get Input Stream
                StreamReader input = ((StreamParams)parameters).StreamReader;

                try
                {
                    //Create the Parser Context and Invoke the Parser
                    TriGParserContext context = new TriGParserContext(store, new TriGTokeniser(input), TokenQueueMode.SynchronousBufferDuringParsing, false, this._tracetokeniser);
                    this.Parse(context);

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
                        //No catch actions just cleaning up
                    }
                    throw;
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriGParser must be of type StreamParams");
            }
        }

        private void Parse(TriGParserContext context)
        {
            //Expect a BOF Token
            IToken first = context.Tokens.Dequeue();
            if (first.TokenType != Token.BOF)
            {
                throw Error("Unexpected Token '" + first.GetType().ToString() + "' encountered, expected a BOF Token", first);
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
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                }
            } while (next.TokenType != Token.EOF);
        }

        private void TryParseDirective(TriGParserContext context)
        {
            //See what type of directive it is
            IToken directive = context.Tokens.Dequeue();

            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                //Base Directives Invalid in TriG
                throw Error("The Base Directive is not a valid in TriG",directive);
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

                            if (prefix.Value.Equals(":"))
                            {
                                //Setting the Default Namespace
                                context.NamespaceMap.AddNamespace(String.Empty, u);
                            }
                            else
                            {
                                //Setting some other Namespace
                                context.NamespaceMap.AddNamespace(prefix.Value.Substring(0, prefix.Value.Length - 1), u);
                            }

                            //Expect a DOT to Terminate
                            IToken dot = context.Tokens.Dequeue();
                            if (dot.TokenType != Token.DOT)
                            {
                                throw Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate a Prefix Directive", dot);
                            }
                        }
                        catch (UriFormatException)
                        {
                            throw Error("The URI '" + uri.Value + "' given for the prefix '" + prefix.Value + "' is not a valid Absolute URI", uri);
                        }
                    }
                    else
                    {
                        throw Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a URI Token after a Prefix Token", uri);
                    }
                }
                else
                {
                    throw Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a Prefix Token after a Prefix Directive Token", prefix);
                }
            }
            else
            {
                throw Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix Directive Token", directive);
            }
        }

        private void TryParseGraph(TriGParserContext context)
        {
            //Create a new Graph
            Graph g = new Graph();

            //Is there a name for the Graph?
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.QNAME)
            {
                //Try to resolve the QName
                Uri u = new Uri(Tools.ResolveQName(next.Value, context.NamespaceMap, null));
                g.BaseUri = u;

                //Get the Next Token
                next = context.Tokens.Dequeue();
            }
            else if (next.TokenType == Token.URI)
            {
                try
                {
                    //Ensure an absolute Uri
                    Uri u = new Uri(next.Value, UriKind.Absolute);

                    //It's valid so set it as the Base Uri
                    g.BaseUri = u;
                }
                catch (UriFormatException)
                {
                    throw Error("The URI '" + next.Value + "' given as a Graph Name is not a valid Absolute URI", next);
                }

                //Get the Next Token
                next = context.Tokens.Dequeue();
            }
            else
            {
                //No Name so is a Default Graph
                if (!context.DefaultGraphExists)
                {
                    g.BaseUri = new Uri(TriGParser.DefaultGraphURI);
                }
                else
                {
                    throw new RdfParseException("You cannot specify more than one Default (Unnamed) Graph in a TriG file");
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
                //Import the Master Namespace Map
                g.NamespaceMap.Clear();
                g.NamespaceMap.Import(context.NamespaceMap);

                //Parse Graph Contents
                this.TryParseTriples(context, g);
            }
            else
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Curly Bracket to start a Graph", next);
            }

            //Add Graph to Store
            context.Store.Add(g);

            //May optionally end with a Dot Token
            next = context.Tokens.Peek();
            if (next.TokenType == Token.DOT)
            {
                //Discard
                context.Tokens.Dequeue();
            }
        }

        private void TryParseTriples(TriGParserContext context, Graph g)
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
                        //QName
                        subjNode = g.CreateUriNode(subj.Value);
                        break;

                    case Token.URI:
                        //Uri (Base Uri will never be null so safe to call ToString() directly)
                        subjNode = g.CreateUriNode(new Uri(Tools.ResolveUri(subj.Value, g.BaseUri.ToString())));
                        break;

                    case Token.BLANKNODEWITHID:
                        //Blank Node with ID
                        subjNode = g.CreateBlankNode(subj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        //Blank Node
                        IToken next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                            subjNode = g.CreateBlankNode();
                        }
                        else
                        {
                            //Blank Node Collection
                            subjNode = g.CreateBlankNode();

                            //Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            this.TryParsePredicateObjectList(context, g, subjNode);
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
                            subjNode = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            subjNode = g.CreateBlankNode();
                            this.TryParseCollection(context, g, subjNode);
                        }
                        break;

                    case Token.EOF:
                        throw Error("Unexpected End of File while trying to parse Triples", subj);

                    default:
                        //Unexpected Token
                        throw Error("Unexpected Token '" + subj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Subject of a Triple", subj);
                }

                //Parse the Predicate Object List
                this.TryParsePredicateObjectList(context, g, subjNode);

                //Expect a Dot to Terminate
                if (context.Tokens.LastTokenType != Token.DOT && context.Tokens.LastTokenType != Token.RIGHTCURLYBRACKET)
                {
                    //We only do this if we haven't returned because we already hit the Dot Token/Right Curly Bracket
                    IToken dot = context.Tokens.Dequeue();
                    if (dot.TokenType != Token.DOT && dot.TokenType != Token.RIGHTCURLYBRACKET)
                    {
                        throw Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate Triples", dot);
                    }
                }

                //If we already hit the Right Curly Bracket return
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET) return;

            } while (context.Tokens.Peek().TokenType != Token.RIGHTCURLYBRACKET);

            //Discard the ending Right Curly Bracket
            context.Tokens.Dequeue();
        }

        private void TryParsePredicateObjectList(TriGParserContext context, Graph g, INode subj)
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
                        //QName
                        predNode = g.CreateUriNode(pred.Value);
                        break;

                    case Token.URI:
                        //Uri (Base Uri will never be null so safe to call ToString() directly)
                        predNode = g.CreateUriNode(new Uri(Tools.ResolveUri(pred.Value, g.BaseUri.ToString())));
                        break;

                    case Token.KEYWORDA:
                        //'a' Keyword
                        predNode = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
                        break;

                    case Token.EOF:
                        throw Error("Unexpected End of File while trying to parse Predicate Object list", pred);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                        if (!ok)
                        {
                            throw Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list was parsed", pred);
                        }
                        return;

                    case Token.RIGHTSQBRACKET:
                        if (!ok)
                        {
                            throw Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list of a Blank Node Collection was parsed", pred);
                        }
                        return;

                    default:
                        //Unexpected Token
                        throw Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered, expected a URI/QName as the Predicate of a Triple", pred);
                }

                ok = true;

                //Parse the Object List
                this.TryParseObjectList(context, g, subj, predNode);

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

        private void TryParseObjectList(TriGParserContext context, Graph g, INode subj, INode pred)
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
                        //QName
                        objNode = g.CreateUriNode(obj.Value);
                        break;

                    case Token.URI:
                        //Uri (Base Uri will never be null so safe to call ToString() directly)
                        objNode = g.CreateUriNode(new Uri(Tools.ResolveUri(obj.Value, g.BaseUri.ToString())));
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
                            objNode = g.CreateLiteralNode(obj.Value, next.Value);
                        }
                        else if (next.TokenType == Token.HATHAT)
                        {
                            //Literal with DataType
                            context.Tokens.Dequeue();
                            //Now expect a QName/Uri Token
                            next = context.Tokens.Dequeue();
                            if (next.TokenType == Token.QNAME || next.TokenType == Token.URI)
                            {
                                Uri dt = new Uri(Tools.ResolveUriOrQName(next, g.NamespaceMap, g.BaseUri));
                                objNode = g.CreateLiteralNode(obj.Value, dt);
                            }
                            else
                            {
                                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName Token to specify a Data Type after a ^^ Token", next);
                            }
                        }
                        else
                        {
                            //Just a string literal
                            objNode = g.CreateLiteralNode(obj.Value);
                        }
                        break;

                    case Token.PLAINLITERAL:
                        //Plain Literals
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)obj);
                        objNode = g.CreateLiteralNode(obj.Value, plt);
                        break;

                    case Token.BLANKNODEWITHID:
                        //Blank Node with ID
                        objNode = g.CreateBlankNode(obj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        //Blank Node
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                            objNode = g.CreateBlankNode();
                        }
                        else
                        {
                            //Blank Node Collection
                            objNode = g.CreateBlankNode();

                            //Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            this.TryParsePredicateObjectList(context, g, objNode);
                        }
                        break;

                    case Token.RIGHTSQBRACKET:
                        //End of Blank Node Collection
                        if (!ok)
                        {
                            throw Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list of a Blank Node Collection was parsed", obj);
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
                            objNode = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            objNode = g.CreateBlankNode();
                            this.TryParseCollection(context, g, objNode);
                        }
                        break;

                    case Token.EOF:
                        throw Error("Unexpected End of File while trying to parse Object List", obj);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                    case Token.SEMICOLON:
                        if (!ok)
                        {
                            throw Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list was parsed", obj);
                        }
                        return;

                    default:
                        //Unexpected Token
                        throw Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Object of a Triple", obj);
                }

                ok = true;

                g.Assert(new Triple(subj, pred, objNode));

            } while (context.Tokens.Peek().TokenType == Token.COMMA); //Expect a comma if we are to continue
        }

        private void TryParseCollection(TriGParserContext context, Graph g, INode subj)
        {
            //Create the Nodes we need
            UriNode rdfFirst, rdfRest, rdfNil;
            rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));

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
                        item = g.CreateUriNode(next.Value);
                        break;
                    case Token.URI:
                        item = g.CreateUriNode(new Uri(Tools.ResolveUri(next.Value, g.BaseUri.ToString())));
                        break;

                    case Token.LITERAL:
                    case Token.LONGLITERAL:

                        break;
                    case Token.PLAINLITERAL:
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)next);
                        item = g.CreateLiteralNode(next.Value, plt);
                        break;

                    case Token.LEFTSQBRACKET:
                        //Check whether an anonymous Blank Node or a Blank Node Collection
                        item = g.CreateBlankNode();

                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                        }
                        else
                        {
                            //Blank Node Collection
                            this.TryParsePredicateObjectList(context, g, item);
                        }
                        break;

                    case Token.LEFTBRACKET:
                        //Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection
                            context.Tokens.Dequeue();
                            item = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            //Collection
                            item = g.CreateBlankNode();
                            this.TryParseCollection(context, g, item);
                        }
                        break;

                    case Token.EOF:
                        throw Error("Unexpected End of File while trying to parse a Collection", next);

                    default:
                        //Unexpected Token
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName/Literal/Blank Node as an item in a Collection", next);
                }

                //Create the subj rdf:first item Triple
                g.Assert(new Triple(subj, rdfFirst, item));

                //Create the rdf:rest Triple
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    //End of Collection
                    context.Tokens.Dequeue();
                    g.Assert(new Triple(subj, rdfRest, rdfNil));
                    return;
                }
                else
                {
                    //Continuing Collection
                    temp = g.CreateBlankNode();
                    g.Assert(new Triple(subj, rdfRest, temp));
                    subj = temp;
                }
            } while (true);

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
            output.Append(t.GetType().ToString());
            output.Append(" at Line ");
            output.Append(t.StartLine);
            output.Append(" Column ");
            output.Append(t.StartPosition);
            output.Append(" to Line ");
            output.Append(t.EndLine);
            output.Append(" Column ");
            output.Append(t.EndPosition);
            output.Append("]\n");
            output.Append(msg);

            return new RdfParseException(output.ToString());
        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void OnWarning(String message)
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
    }
}
