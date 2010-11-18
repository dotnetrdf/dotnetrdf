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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for reading SPARQL Results which have been encoded in the RDF schema for Result Sets and serialized as RDF
    /// </summary>
    public class SparqlRdfParser : ISparqlResultsReader
    {
        /// <summary>
        /// Loads a SPARQL Result Set from RDF contained in the given Stream
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="input">Stream to read from</param>
        /// <remarks>
        /// Uses the <see cref="StringParser">StringParser</see> which will use simple heuristics to 'guess' the format of the RDF
        /// </remarks>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Stream");

            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a Stream into a non-empty Result Set");
            }

            try
            {
                Graph g = new Graph();
                String data = input.ReadToEnd();
                StringParser.Parse(g, data);
                this.Parse(results, g);
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
                    //No Catch actions here just trying to clean up
                }
            }
        }

        /// <summary>
        /// Loads a SPARQL Result Set from RDF contained in the given File
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="filename">File to read from</param>
        /// <remarks>
        /// Uses the <see cref="FileLoader">FileLoader</see> to load the RDF from the file which will attempt to determine the format of the RDF based on the file extension
        /// </remarks>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");

            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a File into a non-empty Result Set");
            }

            Graph g = new Graph();
            FileLoader.Load(g, filename);
            this.Parse(results, g);
        }

        /// <summary>
        /// Internal method which actually parses the Result Set by traversing the RDF Graph appropriately
        /// </summary>
        /// <param name="results">SPARQL Result Set to populate</param>
        /// <param name="g">RDF Graph of the Result Set</param>
        private void Parse(SparqlResultSet results, Graph g)
        {
            //Create relevant Nodes
            g.NamespaceMap.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            g.NamespaceMap.AddNamespace("rs", new Uri(SparqlSpecsHelper.SparqlRdfResultsNamespace));
            UriNode rdfType = g.CreateUriNode("rdf:type");
            UriNode resultSetClass = g.CreateUriNode("rs:ResultSet");
            UriNode resultVariable = g.CreateUriNode("rs:resultVariable");
            UriNode solution = g.CreateUriNode("rs:solution");
            UriNode binding = g.CreateUriNode("rs:binding");
            UriNode value = g.CreateUriNode("rs:value");
            UriNode variable = g.CreateUriNode("rs:variable");
            UriNode boolean = g.CreateUriNode("rs:boolean");

            //Try to get a ResultSet object
            Triple rset = g.Triples.WithPredicateObject(rdfType, resultSetClass).FirstOrDefault();
            if (rset != null)
            {
                INode rsetID = rset.Subject;

                //Find the Variables the Result Set contains or the Boolean Value
                List<Triple> temp = g.Triples.WithSubjectPredicate(rsetID, boolean).ToList();
                if (temp.Count > 0)
                {
                    if (temp.Count > 1) throw new RdfParseException("Result Set has more than one boolean result defined for it");

                    Triple booleanResult = temp.First();
                    INode result = booleanResult.Object;
                    if (result.NodeType == NodeType.Literal)
                    {
                        LiteralNode lit = (LiteralNode)result;
                        if (lit.DataType != null)
                        {
                            if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                            {
                                bool b;
                                if (Boolean.TryParse(lit.Value, out b))
                                {
                                    results.SetResult(b);
                                    results.SetEmpty(false);
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
                    //We're expected one/more variables
                    temp = g.Triples.WithSubjectPredicate(rsetID, resultVariable).ToList();
                    if (temp.Count > 0)
                    {
                        foreach (Triple t in temp)
                        {
                            if (t.Object.NodeType == NodeType.Literal)
                            {
                                results.AddVariable(((LiteralNode)t.Object).Value);
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

                    //Then we're expecting some Solutions
                    temp = g.Triples.WithSubjectPredicate(rsetID, solution).ToList();
                    foreach (Triple slnTriple in temp)
                    {
                        //Each Solution has some Bindings
                        INode slnID = slnTriple.Object;
                        bool ok = false;
                        SparqlResult r = new SparqlResult();

                        foreach (Triple bindingTriple in g.Triples.WithSubjectPredicate(slnID, binding))
                        {
                            //Each Binding has a Variable and a Value
                            ok = true;
                            INode bindingID = bindingTriple.Object;
                            String var = String.Empty;
                            INode val = null;

                            //Retrieve the Variable and the Bound Value
                            foreach (Triple valueTriple in g.Triples.WithSubject(bindingID))
                            {
                                if (valueTriple.Predicate.Equals(variable))
                                {
                                    if (!var.Equals(String.Empty)) throw new RdfParseException("Result Set contains a Binding which refers to more than one Variable");
                                    if (valueTriple.Object.NodeType != NodeType.Literal) throw new RdfParseException("Result Set contains a Binding which refers to a Variable but not by a Literal Node as required");
                                    var = ((LiteralNode)valueTriple.Object).Value;
                                }
                                else if (valueTriple.Predicate.Equals(value))
                                {
                                    if (val != null) throw new RdfParseException("Result Set contains a Binding which has more than one Value");
                                    val = valueTriple.Object;
                                }
                            }
                            if (var.Equals(String.Empty) || val == null) throw new RdfParseException("Result Set contains a Binding which doesn't contain both a Variable and a Value");

                            r.SetValue(var, val);
                        }
                        if (!ok) throw new RdfParseException("Result Set contains a Solution which has no Bindings");

                        results.AddResult(r);
                    }
                }
            }
            else
            {
                throw new RdfParseException("No Result Set object is defined in the Graph");
            }
        }
    }
}
