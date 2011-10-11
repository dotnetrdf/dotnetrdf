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
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for parsing SPARQL Update commands into <see cref="SparqlUpdateCommandSet">SparqlUpdateCommandSet</see> objects that can be used to modify a Triple Store
    /// </summary>
    public class SparqlUpdateParser
        : ITraceableTokeniser, IObjectParser<SparqlUpdateCommandSet>
    {
        private bool _traceTokeniser = false;
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
        private Uri _baseUri;
        private IQueryOptimiser _optimiser;

        //OPT: Add support to the SPARQL Update Parser for selectable syntax in the future

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped custom expression factories
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get
            {
                return this._factories;
            }
            set
            {
                if (value != null)
                {
                    this._factories = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Default Base URI used for Updated Commands parsed by this parser instance
        /// </summary>
        public Uri DefaultBaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped Query Optimiser applied to graph patterns in update commands at the end of the parsing process
        /// </summary>
        /// <remarks>
        /// <para>
        /// May be null if no locally scoped optimiser is set in which case the globally scoped optimiser will be used
        /// </para>
        /// </remarks>
        public IQueryOptimiser QueryOptimiser
        {
            get
            {
                return this._optimiser;
            }
            set
            {
                this._optimiser = value;
            }
        }

        #region Events

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Update Commands being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Update Commands being parsed is detected
        /// </summary>
        public event SparqlWarning Warning;

        #endregion

        #region Public Parsing Methods

        /// <summary>
        /// Parses a SPARQL Update Command Set from the input stream
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <returns></returns>
        public SparqlUpdateCommandSet Parse(StreamReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Update Commands from a null Stream");

            //Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
#if !SILVERLIGHT
                this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
#else
                this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.GetType().Name + " - Please be aware that parsing errors may occur as a result");
#endif            
            }
            return this.Parse((TextReader)input);
        }

        /// <summary>
        /// Parses a SPARQL Update Command Set from the input
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns></returns>
        public SparqlUpdateCommandSet Parse(TextReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parse SPARQL Update Commands from a null TextReader");

            try
            {
                //Start the actual parsing
                SparqlUpdateParserContext context = new SparqlUpdateParserContext(new SparqlTokeniser(input, SparqlQuerySyntax.Sparql_1_1));
                return this.ParseInternal(context);
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
                    //No catch actions just trying to clean up the stream
                }
            }
        }

        /// <summary>
        /// Parses a SPARQL Update Command Set from the given file
        /// </summary>
        /// <param name="file">File</param>
        /// <returns></returns>
        public SparqlUpdateCommandSet ParseFromFile(String file)
        {
            if (file == null) throw new RdfParseException("Cannot parse SPARQL Update Commands from a null File");
            return this.Parse(new StreamReader(file, Encoding.UTF8));
        }

        /// <summary>
        /// Parses a SPARQL Update Command Set from the given String
        /// </summary>
        /// <param name="updates">SPARQL Update Commands</param>
        /// <returns></returns>
        public SparqlUpdateCommandSet ParseFromString(String updates)
        {
            if (updates == null) throw new RdfParseException("Cannot parse SPARQL Update Commands from a null String");
            return this.Parse(new StringReader(updates));
        }

        /// <summary>
        /// Parses a SPARQL Update Command Set from the given String
        /// </summary>
        /// <param name="updates">SPARQL Update Commands</param>
        /// <returns></returns>
        public SparqlUpdateCommandSet ParseFromString(SparqlParameterizedString updates)
        {
            if (updates == null) throw new RdfParseException("Cannot parse SPARQL Update Commands from a null String");
            return this.ParseFromString(updates.ToString());
        }

        #endregion

        #region Internal Parsing Logic

        private SparqlUpdateCommandSet ParseInternal(SparqlUpdateParserContext context)
        {
            //Set up the Context appropriately
            context.BaseUri = this.DefaultBaseUri;
            context.QueryParser.ExpressionFactories = context.ExpressionFactories;
            context.QueryParser.DefaultBaseUri = this.DefaultBaseUri;
            context.ExpressionParser.BaseUri = this.DefaultBaseUri;
            context.ExpressionParser.NamespaceMap = context.NamespaceMap;
            context.ExpressionParser.QueryParser = context.QueryParser;
            context.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            context.Tokens.InitialiseBuffer();

            IToken next;
            bool commandParsed = false;
            do
            {
                next = context.Tokens.Dequeue();
                switch (next.TokenType)
                {
                    case Token.BOF:
                    case Token.COMMENT:
                        //Discardable Tokens
                        break;
                    case Token.EOF:
                        if (next.StartLine == 1 && next.StartPosition == 1)
                        {
                            throw new RdfParseException("Empty SPARQL Updates are not permitted");
                        }
                        break;

                    case Token.BASEDIRECTIVE:
                        this.TryParseBaseDeclaration(context);
                        break;
                    case Token.PREFIXDIRECTIVE:
                        this.TryParsePrefixDeclaration(context);
                        break;

                    case Token.ADD:
                        this.TryParseAddCommand(context);
                        commandParsed = true;
                        break;

                    case Token.CLEAR:
                        this.TryParseClearCommand(context);
                        commandParsed = true;
                        break;

                    case Token.COPY:
                        this.TryParseCopyCommand(context);
                        commandParsed = true;
                        break;

                    case Token.CREATE:
                        this.TryParseCreateCommand(context);
                        commandParsed = true;
                        break;

                    case Token.DROP:
                        this.TryParseDropCommand(context);
                        commandParsed = true;
                        break;

                    case Token.DELETE:
                        context.CommandSet.AddCommand(this.TryParseDeleteCommand(context, true));
                        commandParsed = true;
                        break;

                    case Token.INSERT:
                        context.CommandSet.AddCommand(this.TryParseInsertCommand(context, true));
                        commandParsed = true;
                        break;

                    case Token.LOAD:
                        this.TryParseLoadCommand(context);
                        commandParsed = true;
                        break;

                    case Token.MOVE:
                        this.TryParseMoveCommand(context);
                        commandParsed = true;
                        break;

                    case Token.WITH:
                        this.TryParseModifyCommand(context);
                        commandParsed = true;
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token encountered", next);
                }

                if (commandParsed)
                {
                    //After a Command we expect to see either a separator or EOF
                    next = context.Tokens.Dequeue();
                    if (next.TokenType != Token.SEMICOLON && next.TokenType != Token.EOF)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a semicolon separator or EOF after a command", next);
                    }

                    commandParsed = false;
                }

            } while (next.TokenType != Token.EOF);

            //Optimise the Command Set before returning it
            context.CommandSet.Optimise();
            return context.CommandSet;
        }

        private void TryParseBaseDeclaration(SparqlUpdateParserContext context)
        {
            //Get the next Token which should be a Uri Token
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.URI)
            {
                context.BaseUri = new Uri(next.Value);
                context.CommandSet.BaseUri = context.BaseUri;
                context.QueryParser.DefaultBaseUri = context.BaseUri;
                context.ExpressionParser.BaseUri = context.BaseUri;
            }
            else
            {
                throw ParserHelper.Error("Expected a URI Token to follow the BASE Verb in an Update", next);
            }
        }

        private void TryParsePrefixDeclaration(SparqlUpdateParserContext context)
        {
            //Get the next Two Tokens which should be a Prefix and a Uri
            IToken prefix = context.Tokens.Dequeue();
            IToken uri = context.Tokens.Dequeue();

            if (prefix.TokenType == Token.PREFIX)
            {
                if (uri.TokenType == Token.URI)
                {
                    String baseUri = (context.BaseUri != null) ? context.BaseUri.ToString() : String.Empty;
                    Uri u = new Uri(Tools.ResolveUri(uri.Value, baseUri));
                    if (prefix.Value.Length == 1)
                    {
                        //Defining prefix for Default Namespace
                        context.NamespaceMap.AddNamespace(String.Empty, u);
                        context.CommandSet.NamespaceMap.AddNamespace(String.Empty, u);
                    }
                    else
                    {
                        //Defining prefix for some other Namespace
                        context.NamespaceMap.AddNamespace(prefix.Value.Substring(0, prefix.Value.Length - 1), u);
                        context.CommandSet.NamespaceMap.AddNamespace(prefix.Value.Substring(0, prefix.Value.Length - 1), u);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Expected a URI Token to follow a Prefix Token to follow the PREFIX Verb in an Update", uri);
                }
            }
            else
            {
                throw ParserHelper.Error("Expected a Prefix Token to follow the PREFIX Verb in an Update", prefix);
            }
        }

        private void TryParseAddCommand(SparqlUpdateParserContext context)
        {
            //First an Optional SILENT keyword
            bool silent = false;
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.SILENT)
            {
                context.Tokens.Dequeue();
                silent = true;
            }

            //Then get the Source and Destination URIs
            Uri sourceUri, destUri;
            this.TryParseTransferUris(context, out sourceUri, out destUri);

            context.CommandSet.AddCommand(new AddCommand(sourceUri, destUri, silent));
        }

        private void TryParseClearCommand(SparqlUpdateParserContext context)
        {
            bool silent = false;

            //May possibly have a SILENT Keyword
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.SILENT)
            {
                silent = true;
                next = context.Tokens.Dequeue();
            }

            //Then expect a GRAPH followed by a URI or one of the DEFAULT/NAMED/ALL keywords
            if (next.TokenType == Token.GRAPH)
            {
                Uri u = this.TryParseGraphRef(context);
                ClearCommand cmd = new ClearCommand(u, ClearMode.Graph, silent);
                context.CommandSet.AddCommand(cmd);
            }
            else if (next.TokenType == Token.DEFAULT)
            {
                context.CommandSet.AddCommand(new ClearCommand(ClearMode.Default, silent));
            }
            else if (next.TokenType == Token.NAMED)
            {
                context.CommandSet.AddCommand(new ClearCommand(ClearMode.Named, silent));
            }
            else if (next.TokenType == Token.ALLWORD)
            {
                context.CommandSet.AddCommand(new ClearCommand(ClearMode.All, silent));
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a GRAPH <URI> to specify the Graph to CLEAR or one of the DEFAULT/NAMED/ALL keywords", next);
            }
        }

        private void TryParseCopyCommand(SparqlUpdateParserContext context)
        {
            //First an Optional SILENT keyword
            bool silent = false;
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.SILENT)
            {
                context.Tokens.Dequeue();
                silent = true;
            }

            //Then get the Source and Destination URIs
            Uri sourceUri, destUri;
            this.TryParseTransferUris(context, out sourceUri, out destUri);

            context.CommandSet.AddCommand(new CopyCommand(sourceUri, destUri, silent));
        }

        private void TryParseCreateCommand(SparqlUpdateParserContext context)
        {
            bool silent = false;

            //May possibly have a SILENT Keyword
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.SILENT)
            {
                silent = true;
                next = context.Tokens.Dequeue();
            }

            //Followed by a mandatory GRAPH Keyword
            if (next.TokenType != Token.GRAPH)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a GRAPH Keyword as part of the CREATE command", next);
            }

            //Then MUST have a URI
            Uri u = this.TryParseGraphRef(context);
            CreateCommand cmd = new CreateCommand(u, silent);
            context.CommandSet.AddCommand(cmd);
        }

        private SparqlUpdateCommand TryParseDeleteCommand(SparqlUpdateParserContext context, bool allowData)
        {
            IToken next = context.Tokens.Dequeue();
            List<Uri> usings = new List<Uri>();
            List<Uri> usingNamed = new List<Uri>();
            if (allowData)
            {
                //We are allowed to have an DELETE DATA command here so check for it
                if (next.TokenType == Token.DATA) return this.TryParseDeleteDataCommand(context);
            }
            else
            {
                if (next.TokenType == Token.DATA) throw ParserHelper.Error("The DATA keyword is not permitted here as this INSERT command forms part of a modification command", next);
            }

            if (next.TokenType == Token.WHERE)
            {
                //Parse the WHERE pattern which serves as both the selection and deletion pattern in this case
                context.Tokens.Dequeue();
                GraphPattern where = this.TryParseModifyTemplate(context);

                //Then return the command
                return new DeleteCommand(where, where);
            }
            else
            {
                //Get the Modification Template
                GraphPattern deletions = this.TryParseModifyTemplate(context);

                //Then we expect a WHERE keyword
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.USING)
                {
                    foreach (KeyValuePair<Uri, bool> kvp in this.TryParseUsingStatements(context))
                    {
                        if (kvp.Value)
                        {
                            usingNamed.Add(kvp.Key);
                        }
                        else
                        {
                            usings.Add(kvp.Key);
                        }
                    }
                    next = context.Tokens.Dequeue();
                }
                if (next.TokenType == Token.WHERE)
                {
                    //Now parse the WHERE pattern
                    SparqlQueryParserContext subContext = new SparqlQueryParserContext(context.Tokens);
                    subContext.Query.BaseUri = context.BaseUri;
                    subContext.Query.NamespaceMap = context.NamespaceMap;
                    subContext.ExpressionParser.NamespaceMap = context.NamespaceMap;
                    subContext.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
                    subContext.ExpressionFactories = context.ExpressionFactories;
                    GraphPattern where = context.QueryParser.TryParseGraphPattern(subContext, context.Tokens.LastTokenType != Token.LEFTCURLYBRACKET);

                    //And finally return the command
                    DeleteCommand cmd = new DeleteCommand(deletions, where);
                    usings.ForEach(u => cmd.AddUsingUri(u));
                    usingNamed.ForEach(u => cmd.AddUsingNamedUri(u));
                    return cmd;
                }
                else if (next.TokenType == Token.INSERT)
                {
                    InsertCommand insertCmd = (InsertCommand)this.TryParseInsertCommand(context, false);
                    ModifyCommand cmd = new ModifyCommand(deletions, insertCmd.InsertPattern, insertCmd.WherePattern);
                    insertCmd.UsingUris.ToList().ForEach(u => cmd.AddUsingUri(u));
                    insertCmd.UsingNamedUris.ToList().ForEach(u => cmd.AddUsingNamedUri(u));
                    return cmd;
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a WHERE keyword as part of a DELETE command", next);
                }
            }
        }

        private SparqlUpdateCommand TryParseDeleteDataCommand(SparqlUpdateParserContext context)
        {
            DeleteDataCommand cmd;

            SparqlQueryParserContext subContext = new SparqlQueryParserContext(context.Tokens);
            subContext.Query.BaseUri = context.BaseUri;
            subContext.Query.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            subContext.ExpressionFactories = context.ExpressionFactories;
            GraphPattern gp = context.QueryParser.TryParseGraphPattern(subContext, context.Tokens.LastTokenType != Token.LEFTCURLYBRACKET);

            //Validate that the Graph Pattern is simple
            //Check it doesn't contain anything other than Triple Patterns or if it does it just contains a single GRAPH Pattern
            if (gp.IsFiltered)
            {
                throw new RdfParseException("A FILTER Clause cannot occur in a DELETE DATA Command");
            }
            else if (gp.IsOptional)
            {
                throw new RdfParseException("An OPTIONAL Clause cannot occur in a DELETE DATA Command");
            }
            else if (gp.IsUnion)
            {
                throw new RdfParseException("A UNION Clause cannot occur in a DELETE DATA Command");
            }
            else if (gp.HasChildGraphPatterns)
            {
                if (!gp.ChildGraphPatterns.All(p => p.IsGraph || (!p.IsExists && !p.IsMinus && !p.IsNotExists && !p.IsOptional && !p.IsOptional && !p.IsService && !p.IsSubQuery && !p.IsUnion)))
                {
                    throw new RdfParseException("A DELETE DATA Command may only contain a combination of Triple Patterns and GRAPH clauses");
                }
                else if (gp.ChildGraphPatterns.Any(p => p.HasChildGraphPatterns))
                {
                    throw new RdfParseException("A DELETE DATA Command may not contain nested Graph Patterns");
                }
                else if (gp.HasChildGraphPatterns && gp.TriplePatterns.Count > 0)
                {
                    cmd = new DeleteDataCommand(gp);
                }
                else if (gp.ChildGraphPatterns.Count == 1 && gp.ChildGraphPatterns[0].IsGraph)
                {
                    cmd = new DeleteDataCommand(gp.ChildGraphPatterns[0]);
                }
                else
                {
                    throw new RdfParseException("Nested Graph Patterns cannot occur in a DELETE DATA Command");
                }
            }
            else
            {
                //OK
                cmd = new DeleteDataCommand(gp);
            }

            return cmd;
        }

        private void TryParseDropCommand(SparqlUpdateParserContext context)
        {
            bool silent = false;

            //May possibly have a SILENT Keyword
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.SILENT)
            {
                silent = true;
                next = context.Tokens.Dequeue();
            }

            //Then expect a GRAPH followed by a URI or one of the DEFAULT/NAMED/ALL keywords
            if (next.TokenType == Token.GRAPH)
            {
                Uri u = this.TryParseGraphRef(context);
                DropCommand cmd = new DropCommand(u, ClearMode.Graph, silent);
                context.CommandSet.AddCommand(cmd);
            }
            else if (next.TokenType == Token.DEFAULT)
            {
                context.CommandSet.AddCommand(new DropCommand(ClearMode.Default, silent));
            } 
            else if (next.TokenType == Token.NAMED)
            {
                context.CommandSet.AddCommand(new DropCommand(ClearMode.Named, silent));
            }
            else if (next.TokenType == Token.ALLWORD)
            {
                context.CommandSet.AddCommand(new DropCommand(ClearMode.All, silent));
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a GRAPH <URI> to specify the Graph to DROP or one of the DEFAULT/NAMED/ALL keywords", next);
            }
        }
        
        private SparqlUpdateCommand TryParseInsertCommand(SparqlUpdateParserContext context, bool allowData)
        {
            List<Uri> usings = new List<Uri>();
            List<Uri> usingNamed = new List<Uri>();
            IToken next = context.Tokens.Dequeue();
            if (allowData)
            {
                //We are allowed to have an INSERT DATA command here so check for it
                if (next.TokenType == Token.DATA) return this.TryParseInsertDataCommand(context);
            }
            else
            {
                if (next.TokenType == Token.DATA) throw ParserHelper.Error("The DATA keyword is not permitted here as this INSERT command forms part of a modification command", next);
            }

            //Get the Modification Template
            GraphPattern insertions = this.TryParseModifyTemplate(context);

            //Then we expect a WHERE keyword
            next = context.Tokens.Dequeue();
            if (next.TokenType == Token.USING)
            {
                foreach (KeyValuePair<Uri, bool> kvp in this.TryParseUsingStatements(context))
                {
                    if (kvp.Value)
                    {
                        usingNamed.Add(kvp.Key);
                    }
                    else
                    {
                        usings.Add(kvp.Key);
                    }
                }
                next = context.Tokens.Dequeue();
            }
            if (next.TokenType != Token.WHERE) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a WHERE keyword as part of a INSERT command", next);
            
            //Now parse the WHERE pattern
            SparqlQueryParserContext subContext = new SparqlQueryParserContext(context.Tokens);
            subContext.Query.BaseUri = context.BaseUri;
            subContext.Query.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            subContext.ExpressionFactories = context.ExpressionFactories;
            GraphPattern where = context.QueryParser.TryParseGraphPattern(subContext, context.Tokens.LastTokenType != Token.LEFTCURLYBRACKET);

            //And finally return the command
            InsertCommand cmd = new InsertCommand(insertions, where);
            usings.ForEach(u => cmd.AddUsingUri(u));
            usingNamed.ForEach(u => cmd.AddUsingNamedUri(u));
            return cmd;
        }

        private SparqlUpdateCommand TryParseInsertDataCommand(SparqlUpdateParserContext context)
        {
            InsertDataCommand cmd;

            SparqlQueryParserContext subContext = new SparqlQueryParserContext(context.Tokens);
            subContext.Query.BaseUri = context.BaseUri;
            subContext.Query.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            subContext.ExpressionFactories = context.ExpressionFactories;
            GraphPattern gp = context.QueryParser.TryParseGraphPattern(subContext, context.Tokens.LastTokenType != Token.LEFTCURLYBRACKET);

            //Validate that the Graph Pattern is simple
            //Check it doesn't contain anything other than Triple Patterns or if it does it just contains a single GRAPH Pattern
            if (gp.IsFiltered)
            {
                throw new RdfParseException("A FILTER Clause cannot occur in a INSERT DATA Command");
            }
            else if (gp.IsOptional)
            {
                throw new RdfParseException("An OPTIONAL Clause cannot occur in a INSERT DATA Command");
            }
            else if (gp.IsUnion)
            {
                throw new RdfParseException("A UNION Clause cannot occur in a INSERT DATA Command");
            }
            else if (gp.HasChildGraphPatterns)
            {
                if (!gp.ChildGraphPatterns.All(p => p.IsGraph || (!p.IsExists && !p.IsMinus && !p.IsNotExists && !p.IsOptional && !p.IsOptional && !p.IsService && !p.IsSubQuery && !p.IsUnion)))
                {
                    throw new RdfParseException("An INSERT DATA Command may only contain a combination of Triple Patterns and GRAPH clauses");
                }
                else if (gp.ChildGraphPatterns.Any(p => p.HasChildGraphPatterns))
                {
                    throw new RdfParseException("An INSERT DATA Command may not contain nested Graph Patterns");
                }
                else if (gp.HasChildGraphPatterns && gp.TriplePatterns.Count > 0)
                {
                    cmd = new InsertDataCommand(gp);
                }
                else if (gp.ChildGraphPatterns.Count == 1 && gp.ChildGraphPatterns[0].IsGraph)
                {
                    cmd = new InsertDataCommand(gp.ChildGraphPatterns[0]);
                }
                else
                {
                    throw new RdfParseException("Nested Graph Patterns cannot occur in a INSERT DATA Command");
                }
            }
            else
            {
                //OK
                cmd = new InsertDataCommand(gp);
            }

            return cmd;
        }

        private void TryParseLoadCommand(SparqlUpdateParserContext context)
        {
            LoadCommand cmd;
            String baseUri = context.BaseUri.ToSafeString();

            //May optionally have a SILENT keyword
            bool silent = false;
            if (context.Tokens.Peek().TokenType == Token.SILENT)
            {
                silent = true;
                context.Tokens.Dequeue();
            }

            //Expect a URI which is the Source URI
            Uri sourceUri = this.TryParseIriRef(context, "to LOAD data from");

            //Then optionally an INTO GRAPH followed by a Graph URI to assign
            if (context.Tokens.Count > 0)
            {
                IToken next = context.Tokens.Peek();
                if (next.TokenType == Token.INTO)
                {
                    context.Tokens.Dequeue();
                    next = context.Tokens.Dequeue();
                    if (next.TokenType == Token.GRAPH)
                    {
                        Uri destUri = this.TryParseGraphRef(context);
                        cmd = new LoadCommand(sourceUri, destUri, silent);
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a GRAPH keyword after the INTO keyword for a LOAD command", next);
                    }
                }
                else
                {
                    cmd = new LoadCommand(sourceUri, silent);
                }
            }
            else
            {
                cmd = new LoadCommand(sourceUri, silent);
            }
            context.CommandSet.AddCommand(cmd);
        }

        private void TryParseModifyCommand(SparqlUpdateParserContext context)
        {
            //Firstly we expect the URI that the modifications apply to
            Uri u = this.TryParseIriRef(context, "after a WITH keyword");

            //Now parse the INSERT/DELETE as appropriate
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.INSERT)
            {
                InsertCommand insertCmd = (InsertCommand)this.TryParseInsertCommand(context, false);
                insertCmd.GraphUri = u;
                context.CommandSet.AddCommand(insertCmd);
            }
            else if (next.TokenType == Token.DELETE)
            {
                SparqlUpdateCommand deleteCmd = this.TryParseDeleteCommand(context, false);
                if (deleteCmd is BaseModificationCommand)
                {
                    ((BaseModificationCommand)deleteCmd).GraphUri = u;
                }
                else
                {
                    throw new RdfParseException("Unexpected Command returned by TryParseDeleteCommand()");
                }
                context.CommandSet.AddCommand(deleteCmd);
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an INSERT/DELETE keyword", next);
            }
        }

        private GraphPattern TryParseModifyTemplate(SparqlUpdateParserContext context)
        {
            SparqlQueryParserContext subContext = new SparqlQueryParserContext(context.Tokens);
            subContext.Query.BaseUri = context.BaseUri;
            subContext.Query.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.NamespaceMap = context.NamespaceMap;
            subContext.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            subContext.ExpressionFactories = context.ExpressionFactories;
            GraphPattern gp = context.QueryParser.TryParseGraphPattern(subContext, context.Tokens.LastTokenType != Token.LEFTCURLYBRACKET);

            //Validate that the Graph Pattern is simple
            //Check it doesn't contain anything other than Triple Patterns or if it does it just contains a single GRAPH Pattern
            if (gp.IsFiltered)
            {
                throw new RdfParseException("A FILTER Clause cannot occur in a Modify Template");
            }
            else if (gp.IsOptional)
            {
                throw new RdfParseException("An OPTIONAL Clause cannot occur in a Modify Template");
            }
            else if (gp.IsUnion)
            {
                throw new RdfParseException("A UNION Clause cannot occur in a Modify Template");
            }
            else if (gp.HasChildGraphPatterns)
            {
                if (gp.ChildGraphPatterns.All(p => p.IsGraph && !p.IsFiltered && !p.IsOptional && !p.IsUnion && !p.HasChildGraphPatterns))
                {
                    return gp;
                }
                else
                {
                    throw new RdfParseException("Nested Graph Patterns cannot occur in a Modify Template");
                }
            }
            else
            {
                return gp;
            }

        }

        private void TryParseMoveCommand(SparqlUpdateParserContext context)
        {
            //First an Optional SILENT keyword
            bool silent = false;
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.SILENT)
            {
                context.Tokens.Dequeue();
                silent = true;
            }

            //Then get the Source and Destination URIs
            Uri sourceUri, destUri;
            this.TryParseTransferUris(context, out sourceUri, out destUri);

            context.CommandSet.AddCommand(new MoveCommand(sourceUri, destUri, silent));
        }

        private void TryParseUsings(SparqlUpdateParserContext context, BaseModificationCommand cmd)
        {
            foreach (KeyValuePair<Uri,bool> u in this.TryParseUsingStatements(context))
            {
                //If the Boolean flag is true then this was a USING NAMED as opposed to a USING
                if (u.Value)
                {
                    cmd.AddUsingNamedUri(u.Key);
                }
                else
                {
                    cmd.AddUsingUri(u.Key);
                }
            }
        }

        private IEnumerable<KeyValuePair<Uri,bool>> TryParseUsingStatements(SparqlUpdateParserContext context)
        {
            if (context.Tokens.Count > 0)
            {
                String baseUri = (context.BaseUri == null) ? String.Empty : context.BaseUri.ToString();
                IToken next = context.Tokens.Peek();
                bool named = false;

                //While we can see USINGs we'll keep returning USING URIs
                do
                {
                    if (context.Tokens.LastTokenType != Token.USING) context.Tokens.Dequeue();
                    next = context.Tokens.Peek();

                    if (next.TokenType == Token.NAMED)
                    {
                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                        named = true;
                    }
                    if (next.TokenType == Token.URI || next.TokenType == Token.QNAME)
                    {
                        //Yield the URI
                        Uri u = this.TryParseIriRef(context, " as part of a USING clause");
                        yield return new KeyValuePair<Uri, bool>(u, named);
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI as part of a USING statement", next);
                    }

                    //Peek at the next thing in case it's a further USING
                    next = context.Tokens.Peek();
                } while (next.TokenType == Token.USING);
            }
            else
            {
                yield break;
            }
        }

        private void TryParseTransferUris(SparqlUpdateParserContext context, out Uri sourceUri, out Uri destUri)
        {
            IToken next = context.Tokens.Dequeue();
            sourceUri = destUri = null;

            //Parse the Source Graph URI
            if (next.TokenType == Token.GRAPH)
            {
                next = context.Tokens.Peek();
                if (next.TokenType == Token.URI || next.TokenType == Token.QNAME)
                {
                    sourceUri = this.TryParseIriRef(context, " to indicate the Source Graph for a Transfer (ADD/COPY/MOVE) command");
                }
                else
                {
                    ParserHelper.Error("Unexpected Token '" + next.GetType().Name + "' encountered, expected a URI/QName after a GRAPH keyword to specify the URI of the Source Graph for a Transfer (ADD/COPY/MOVE) Command", next);
                }
            }
            else if (next.TokenType == Token.DEFAULT)
            {
                sourceUri = null;
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().Name + "' encountered, expected a GRAPH/DEFAULT keyword to indicate the Source Graph for a Transfer (ADD/COPY/MOVE) Command", next);
            }

            //Then get the TO keyword
            next = context.Tokens.Dequeue();
            if (next.TokenType != Token.TO) throw ParserHelper.Error("Unexpected Token '" + next.GetType().Name + "' encountered, expected a TO Keyword after the Source Graph specifier", next);

            next = context.Tokens.Dequeue();

            //Parse the Destination Graph URI
            if (next.TokenType == Token.GRAPH)
            {
                next = context.Tokens.Peek();
                if (next.TokenType == Token.URI || next.TokenType == Token.QNAME)
                {
                    destUri = this.TryParseIriRef(context, " to indicate the Destination Graph for a Transfer (ADD/COPY/MOVE) command");
                }
                else
                {
                    ParserHelper.Error("Unexpected Token '" + next.GetType().Name + "' encountered, expected a URI/QName after a GRAPH keyword to specify the URI of the Destination Graph for a Transfer (ADD/COPY/MOVE) Command", next);
                }
            }
            else if (next.TokenType == Token.DEFAULT)
            {
                destUri = null;
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().Name + "' encountered, expected a GRAPH/DEFAULT keyword to indicate the Destination Graph for a Transfer (ADD/COPY/MOVE) Command", next);
            }
        }

        private Uri TryParseGraphRef(SparqlUpdateParserContext context)
        {
            return this.TryParseIriRef(context, "after a GRAPH keyword");
        }

        private Uri TryParseIriRef(SparqlUpdateParserContext context, String expected)
        {
            IToken next = context.Tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.URI:
                    return new Uri(Tools.ResolveUri(next.Value, context.BaseUri.ToSafeString()));
                case Token.QNAME:
                    return new Uri(Tools.ResolveQName(next.Value, context.NamespaceMap, context.BaseUri));
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName Token " + expected, next);
            }
        }

        #endregion
    }
}
