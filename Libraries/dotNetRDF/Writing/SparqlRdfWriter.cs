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
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving SPARQL Result Sets in the RDF serialization in the RDF format of your choice (default Turtle)
    /// </summary>
    public class SparqlRdfWriter : ISparqlResultsWriter
    {
        private IRdfWriter _writer;

        /// <summary>
        /// Creates a new SPARQL RDF Writer which will save Result Sets in the RDF serialization using Turtle syntax
        /// </summary>
        public SparqlRdfWriter()
            : this(new CompressingTurtleWriter(WriterCompressionLevel.High)) { }

        /// <summary>
        /// Creates a new SPARQL RDF Writer which will save Result Sets in the RDF serialization in your chosen RDF Syntax
        /// </summary>
        /// <param name="writer">RDF Writer to use</param>
        public SparqlRdfWriter(IRdfWriter writer)
        {
            _writer = writer;
        }

        
        /// <summary>
        /// Saves the SPARQL Result Set to the given File
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, string filename)
        {
            _writer.Save(GenerateOutput(results), filename);
        }

        /// <summary>
        /// Saves the SPARQL Result Set to the given Stream
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            _writer.Save(GenerateOutput(results), output);
        }

        /// <summary>
        /// Method which generates the RDF Graph of a SPARQL Result Set
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <returns></returns>
        public IGraph GenerateOutput(SparqlResultSet results)
        {
            // Create the Graph for the Output
            IGraph g = new Graph();

            // Add the relevant namespaces
            g.NamespaceMap.AddNamespace("rs", UriFactory.Create(SparqlSpecsHelper.SparqlRdfResultsNamespace));

            // Create relevant Nodes
            IUriNode rdfType = g.CreateUriNode("rdf:type");
            IUriNode resultSetClass = g.CreateUriNode("rs:ResultSet");
            IUriNode resultVariable = g.CreateUriNode("rs:resultVariable");
            IUriNode solution = g.CreateUriNode("rs:solution");
            IUriNode binding = g.CreateUriNode("rs:binding");
            IUriNode value = g.CreateUriNode("rs:value");
            IUriNode variable = g.CreateUriNode("rs:variable");
            IUriNode boolean = g.CreateUriNode("rs:boolean");

            // First we declare a Result Set
            IBlankNode rset = g.CreateBlankNode();
            g.Assert(new Triple(rset, rdfType, resultSetClass));

            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                // Assert a Triple for each Result Variable
                foreach (String v in results.Variables)
                {
                    g.Assert(new Triple(rset, resultVariable, g.CreateLiteralNode(v)));
                }

                // Then we're going to define a solution for each result
                foreach (SparqlResult r in results)
                {
                    IBlankNode sln = g.CreateBlankNode();
                    g.Assert(new Triple(rset, solution, sln));

                    foreach (String v in results.Variables)
                    {
                        // Only define Bindings if there is a value and it is non-null
                        if (r.HasValue(v) && r[v] != null)
                        {
                            IBlankNode bnd = g.CreateBlankNode();
                            g.Assert(new Triple(sln, binding, bnd));
                            g.Assert(new Triple(bnd, variable, g.CreateLiteralNode(v)));
                            switch (r[v].NodeType) 
                            {
                                case NodeType.Blank:
                                    IBlankNode b = (IBlankNode)r[v];
                                    IBlankNode bMapped;
                                    if (b.GraphUri == null)
                                    {
                                        bMapped = g.CreateBlankNode(b.InternalID + "def");
                                    }
                                    else
                                    {
                                        bMapped = g.CreateBlankNode(b.InternalID + b.GraphUri.GetEnhancedHashCode());
                                    }
                                    g.Assert(new Triple(bnd, value, bMapped));
                                    break;
                                case NodeType.GraphLiteral:
                                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("SPARQL Results RDF Serialization"));
                                case NodeType.Literal:
                                case NodeType.Uri:
                                    g.Assert(new Triple(bnd, value, r[v].CopyNode(g)));
                                    break;
                                default:
                                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("SPARQL Results RDF Serialization"));
                            }
                        }
                    }
                }
            }
            else
            {
                // A Boolean Result Set
                g.Assert(new Triple(rset, boolean, g.CreateLiteralNode(results.Result.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean))));
            }

            return g;
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
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
        /// Event raised when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results in RDF as " + _writer.ToString();
        }
    }
}
