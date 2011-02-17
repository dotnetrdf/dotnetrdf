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
            this._writer = writer;
        }

        /// <summary>
        /// Saves the SPARQL Result Set to the given File
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, string filename)
        {
            this._writer.Save(this.GenerateOutput(results), filename);
        }

        /// <summary>
        /// Saves the SPARQL Result Set to the given Stream
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            this._writer.Save(this.GenerateOutput(results), output);
        }

        /// <summary>
        /// Method which generates the RDF Graph of a SPARQL Result Set
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <returns></returns>
        public Graph GenerateOutput(SparqlResultSet results)
        {
            //Create the Graph for the Output
            Graph g = new Graph();

            //Add the relevant namespaces
            g.NamespaceMap.AddNamespace("rs", new Uri(SparqlSpecsHelper.SparqlRdfResultsNamespace));

            //Create relevant Nodes
            UriNode rdfType = g.CreateUriNode("rdf:type");
            UriNode resultSetClass = g.CreateUriNode("rs:ResultSet");
            UriNode resultVariable = g.CreateUriNode("rs:resultVariable");
            UriNode solution = g.CreateUriNode("rs:solution");
            UriNode binding = g.CreateUriNode("rs:binding");
            UriNode value = g.CreateUriNode("rs:value");
            UriNode variable = g.CreateUriNode("rs:variable");
            UriNode boolean = g.CreateUriNode("rs:boolean");

            //First we declare a Result Set
            BlankNode rset = g.CreateBlankNode();
            g.Assert(new Triple(rset, rdfType, resultSetClass));

            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                //Assert a Triple for each Result Variable
                foreach (String v in results.Variables)
                {
                    g.Assert(new Triple(rset, resultVariable, g.CreateLiteralNode(v)));
                }

                //Then we're going to define a solution for each result
                foreach (SparqlResult r in results)
                {
                    BlankNode sln = g.CreateBlankNode();
                    g.Assert(new Triple(rset, solution, sln));

                    foreach (String v in results.Variables)
                    {
                        //Only define Bindings if there is a value and it is non-null
                        if (r.HasValue(v) && r[v] != null)
                        {
                            BlankNode bnd = g.CreateBlankNode();
                            g.Assert(new Triple(sln, binding, bnd));
                            g.Assert(new Triple(bnd, variable, g.CreateLiteralNode(v)));
                            switch (r[v].NodeType) {
                                case NodeType.Blank:
                                    BlankNode b = (BlankNode)r[v];
                                    g.Assert(new Triple(bnd, value, g.CreateBlankNode(b.InternalID)));
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
                //A Boolean Result Set
                g.Assert(new Triple(rset, boolean, g.CreateLiteralNode(results.Result.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean))));
            }

            return g;
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
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
        /// Event raised when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results in RDF as " + this._writer.ToString();
        }
    }
}
