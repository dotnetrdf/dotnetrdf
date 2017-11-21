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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for reading SPARQL Results which have been encoded in the RDF schema for Result Sets and serialized as RDF
    /// </summary>
    public class SparqlRdfParser : ISparqlResultsReader
    {
        private IRdfReader _parser;

        /// <summary>
        /// Creates a new SPARQL RDF Parser which will use auto-detection for determining the syntax of input streams/files
        /// </summary>
        public SparqlRdfParser()
        { }

        /// <summary>
        /// Creates a new SPARQL RDF Parser which will use the given RDF Parser
        /// </summary>
        /// <param name="parser">RDF Parser</param>
        public SparqlRdfParser(IRdfReader parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Loads a SPARQL Result Set from RDF contained in the given Input
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="input">Input to read from</param>
        /// <remarks>
        /// Uses the <see cref="StringParser">StringParser</see> which will use simple heuristics to 'guess' the format of the RDF unless the parser was instaniated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(SparqlResultSet results, TextReader input)
        {
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a SPARQL Result Set from RDF contained in the given Stream
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="input">Stream to read from</param>
        /// <remarks>
        /// Uses the <see cref="StringParser">StringParser</see> which will use simple heuristics to 'guess' the format of the RDF unless the parser was instaniated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a SPARQL Result Set from RDF contained in the given File
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="filename">File to read from</param>
        /// <remarks>
        /// Uses the <see cref="FileLoader">FileLoader</see> to load the RDF from the file which will attempt to determine the format of the RDF based on the file extension unless the parser was instantiated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), filename);
        }

        /// <summary>
        /// Loads a SPARQL Results from RDF contained in the given Input using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to read from</param>
        /// <remarks>
        /// Uses the <see cref="StringParser">StringParser</see> which will use simple heuristics to 'guess' the format of the RDF unless the parser was instaniated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Stream");

            try
            {
                Graph g = new Graph();
                if (_parser == null)
                {
                    String data = input.ReadToEnd();
                    StringParser.Parse(g, data);
                }
                else
                {
                    _parser.Load(g, input);
                }
                Parse(new SparqlRdfParserContext(g, handler));
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
                    // No Catch actions here just trying to clean up
                }
            }
        }

        /// <summary>
        /// Loads a SPARQL Results from RDF contained in the given Stream using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Stream to read from</param>
        /// <remarks>
        /// Uses the <see cref="StringParser">StringParser</see> which will use simple heuristics to 'guess' the format of the RDF unless the parser was instaniated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads a SPARQL Results from RDF contained in the given file using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="filename">File to read from</param>
        /// <remarks>
        /// Uses the <see cref="FileLoader">FileLoader</see> to load the RDF from the file which will attempt to determine the format of the RDF based on the file extension unless the parser was instantiated with a specific <see cref="IRdfReader">IRdfReader</see> to use
        /// </remarks>
        public void Load(ISparqlResultsHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");

            Graph g = new Graph();
            if (_parser == null)
            {
                FileLoader.Load(g, filename);
            }
            else
            {
                _parser.Load(g, filename);
            }
            Parse(new SparqlRdfParserContext(g, handler));
        }

        /// <summary>
        /// Internal method which actually parses the Result Set by traversing the RDF Graph appropriately
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(SparqlRdfParserContext context)
        {
            try
            {
                context.Handler.StartResults();

                // Create relevant Nodes
                context.Graph.NamespaceMap.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));
                context.Graph.NamespaceMap.AddNamespace("rs", UriFactory.Create(SparqlSpecsHelper.SparqlRdfResultsNamespace));
                IUriNode rdfType = context.Graph.CreateUriNode("rdf:type");
                IUriNode resultSetClass = context.Graph.CreateUriNode("rs:ResultSet");
                IUriNode resultVariable = context.Graph.CreateUriNode("rs:resultVariable");
                IUriNode solution = context.Graph.CreateUriNode("rs:solution");
                IUriNode binding = context.Graph.CreateUriNode("rs:binding");
                IUriNode value = context.Graph.CreateUriNode("rs:value");
                IUriNode variable = context.Graph.CreateUriNode("rs:variable");
                IUriNode boolean = context.Graph.CreateUriNode("rs:boolean");

                // Try to get a ResultSet object
                Triple rset = context.Graph.Triples.WithPredicateObject(rdfType, resultSetClass).FirstOrDefault();
                if (rset != null)
                {
                    INode rsetID = rset.Subject;

                    // Find the Variables the Result Set contains or the Boolean Value
                    List<Triple> temp = context.Graph.Triples.WithSubjectPredicate(rsetID, boolean).ToList();
                    if (temp.Count > 0)
                    {
                        if (temp.Count > 1) throw new RdfParseException("Result Set has more than one boolean result defined for it");

                        Triple booleanResult = temp.First();
                        INode result = booleanResult.Object;
                        if (result.NodeType == NodeType.Literal)
                        {
                            ILiteralNode lit = (ILiteralNode)result;
                            if (lit.DataType != null)
                            {
                                if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                                {
                                    bool b;
                                    if (Boolean.TryParse(lit.Value, out b))
                                    {
                                        context.Handler.HandleBooleanResult(b);
                                        return;
                                    }
                                    else
                                    {
                                        throw new RdfParseException("Result Set has a boolean result which is a Literal typed as boolean but which does not contain a valid boolean value");
                                    }
                                }
                                else
                                {
                                    throw new RdfParseException("Result Set has a boolean result which is a Literal which is not boolean typed");
                                }
                            }
                            else
                            {
                                throw new RdfParseException("Result Set has a boolean result which is a Literal which is not typed as a boolean");
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Result Set has a boolean result which is not a Literal Node");
                        }
                    }
                    else
                    {
                        // We're expected one/more variables
                        temp = context.Graph.Triples.WithSubjectPredicate(rsetID, resultVariable).ToList();
                        if (temp.Count > 0)
                        {
                            foreach (Triple t in temp)
                            {
                                if (t.Object.NodeType == NodeType.Literal)
                                {
                                    if (!context.Handler.HandleVariable(((ILiteralNode)t.Object).Value)) ParserHelper.Stop();
                                    context.Variables.Add(((ILiteralNode)t.Object).Value);
                                }
                                else
                                {
                                    throw new RdfParseException("Result Set has a result variable definition which is not a Literal Node");
                                }
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Result Set does not define any result variables or a boolean result");
                        }

                        // Then we're expecting some Solutions
                        temp = context.Graph.Triples.WithSubjectPredicate(rsetID, solution).ToList();
                        foreach (Triple slnTriple in temp)
                        {
                            // Each Solution has some Bindings
                            INode slnID = slnTriple.Object;
                            bool ok = false;
                            SparqlResult r = new SparqlResult();

                            foreach (Triple bindingTriple in context.Graph.Triples.WithSubjectPredicate(slnID, binding))
                            {
                                // Each Binding has a Variable and a Value
                                ok = true;
                                INode bindingID = bindingTriple.Object;
                                String var = String.Empty;
                                INode val = null;

                                // Retrieve the Variable and the Bound Value
                                foreach (Triple valueTriple in context.Graph.Triples.WithSubject(bindingID))
                                {
                                    if (valueTriple.Predicate.Equals(variable))
                                    {
                                        if (!var.Equals(String.Empty)) throw new RdfParseException("Result Set contains a Binding which refers to more than one Variable");
                                        if (valueTriple.Object.NodeType != NodeType.Literal) throw new RdfParseException("Result Set contains a Binding which refers to a Variable but not by a Literal Node as required");
                                        var = ((ILiteralNode)valueTriple.Object).Value;
                                    }
                                    else if (valueTriple.Predicate.Equals(value))
                                    {
                                        if (val != null) throw new RdfParseException("Result Set contains a Binding which has more than one Value");
                                        val = valueTriple.Object;
                                    }
                                }
                                if (var.Equals(String.Empty) || val == null) throw new RdfParseException("Result Set contains a Binding which doesn't contain both a Variable and a Value");

                                // Check that the Variable was defined in the Header
                                if (!context.Variables.Contains(var))
                                {
                                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element attempts to bind a value to the variable '" + var + "' which is not defined in the <head> by a <variable> element!");
                                }

                                r.SetValue(var, val);
                            }
                            if (!ok) throw new RdfParseException("Result Set contains a Solution which has no Bindings");

                            // Check that all Variables are bound for a given result binding nulls where appropriate
                            foreach (String v in context.Variables)
                            {
                                if (!r.HasValue(v))
                                {
                                    r.SetValue(v, null);
                                }
                            }

                            if (!context.Handler.HandleResult(r)) ParserHelper.Stop();
                        }
                    }
                }
                else
                {
                    throw new RdfParseException("No Result Set object is defined in the Graph");
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
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
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
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_parser != null)
            {
                return "SPARQL Results in RDF (" + _parser.ToString() + ")";
            }
            else
            {
                return "SPARQL Results in RDF (Auto-detect)";
            }
        }
    }
}
