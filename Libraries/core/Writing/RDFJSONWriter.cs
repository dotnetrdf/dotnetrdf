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
using Newtonsoft.Json;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating RDF/Json Concrete Syntax
    /// </summary>
    /// <remarks>
    /// <p>
    /// Uses the Json.Net library by <a href="http://james.newtonking.com">James Newton-King</a> to output RDF/Json according to the specification located on the <a href="http://n2.talis.com/wiki/RDF_JSON_Specification">Talis n2 Wiki</a>
    /// </p>
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    public class RdfJsonWriter : IRdfWriter, IPrettyPrintingWriter
    {
        private bool _prettyprint = true;

        /// <summary>
        /// Gets/Sets Pretty Print Mode for the Writer
        /// </summary>
        public bool PrettyPrintMode
        {
            get
            {
                return this._prettyprint;
            }
            set
            {
                this._prettyprint = value;
            }
        }

        /// <summary>
        /// Saves a Graph in RDF/Json syntax to the given File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public void Save(IGraph g, string filename)
        {
            StreamWriter output = new StreamWriter(filename, false, new UTF8Encoding(Options.UseBomForUtf8));
            this.Save(g, output);
        }

        /// <summary>
        /// Saves a Graph to an arbitrary output stream
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(IGraph g, TextWriter output)
        {
            try
            {
                //Always issue a Warning
                this.RaiseWarning("RDF/JSON does not contain any Namespace information.  If you read this serialized data back in at a later date you may not be able to reserialize it to Namespace reliant formats (like RDF/XML)");

                this.GenerateOutput(g, output);
                output.Close();
            }
            catch
            {
                try
                {
                    //Close the Output Stream
                    output.Close();
                }
                catch
                {
                    //No Catch actions here
                }
                throw;
            }
        }

        /// <summary>
        /// Internal method which generates the RDF/Json Output for a Graph
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="output">Stream to save to</param>
        private void GenerateOutput(IGraph g, TextWriter output)
        {
            //Get a Blank Node Output Mapper
            BlankNodeOutputMapper bnodeMapper = new BlankNodeOutputMapper(WriterHelper.IsValidBlankNodeID);

            //Get the Writer and Configure Options
            JsonTextWriter writer = new JsonTextWriter(output);
            if (this._prettyprint)
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;
            }
            else
            {
                writer.Formatting = Newtonsoft.Json.Formatting.None;
            }

            //Start the overall Object which represents the Graph
            writer.WriteStartObject();

            //Get the Triples as a Sorted List
            List<Triple> ts = g.Triples.ToList();
            ts.Sort();

            //Variables we need to track our writing
            INode lastSubj, lastPred;
            lastSubj = lastPred = null;

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = ts[i];
                if (lastSubj == null || !t.Subject.Equals(lastSubj))
                {
                    //Terminate previous Triples
                    if (lastSubj != null)
                    {
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                    }

                    //Start a new set of Triples
                    //Validate Subject
                    switch (t.Subject.NodeType)
                    {
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/JSON"));
                        case NodeType.Literal:
                            throw new RdfOutputException(WriterErrorMessages.LiteralSubjectsUnserializable("RDF/JSON"));
                        case NodeType.Blank:
                            break;
                        case NodeType.Uri:
                            //OK
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/JSON"));
                    }

                    //Write out the Subject
                    if (t.Subject.NodeType != NodeType.Blank)
                    {
                        writer.WritePropertyName(t.Subject.ToString());
                    }
                    else
                    {
                        //Remap Blank Node IDs as appropriate
                        writer.WritePropertyName("_:" + bnodeMapper.GetOutputID(((IBlankNode)t.Subject).InternalID));
                    }

                    //Start an Object for the Subject
                    writer.WriteStartObject();

                    lastSubj = t.Subject;

                    //Write the first Predicate
                    //Validate Predicate
                    switch (t.Predicate.NodeType)
                    {
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/JSON"));
                        case NodeType.Blank:
                            throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/JSON"));
                        case NodeType.Literal:
                            throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/JSON"));
                        case NodeType.Uri:
                            //OK
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/JSON"));
                    }

                    //Write the Predicate
                    writer.WritePropertyName(t.Predicate.ToString());

                    //Create an Array for the Objects
                    writer.WriteStartArray();

                    lastPred = t.Predicate;
                }
                else if (lastPred == null || !t.Predicate.Equals(lastPred))
                {
                    //Terminate previous Predicate Object list
                    writer.WriteEndArray();

                    //Write the next Predicate
                    //Validate Predicate
                    switch (t.Predicate.NodeType)
                    {
                        case NodeType.GraphLiteral:
                            throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/JSON"));
                        case NodeType.Blank:
                            throw new RdfOutputException(WriterErrorMessages.BlankPredicatesUnserializable("RDF/JSON"));
                        case NodeType.Literal:
                            throw new RdfOutputException(WriterErrorMessages.LiteralPredicatesUnserializable("RDF/JSON"));
                        case NodeType.Uri:
                            //OK
                            break;
                        default:
                            throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/JSON"));
                    }

                    //Write the Predicate
                    writer.WritePropertyName(t.Predicate.ToString());

                    //Create an Array for the Objects
                    writer.WriteStartArray();

                    lastPred = t.Predicate;
                }

                //Write the Object
                //Create an Object for the Object
                INode obj = t.Object;
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                switch (obj.NodeType)
                {
                    case NodeType.Blank:
                        //Remap Blank Node IDs as appropriate
                        writer.WriteValue("_:" + bnodeMapper.GetOutputID(((IBlankNode)obj).InternalID));
                        writer.WritePropertyName("type");
                        writer.WriteValue("bnode");
                        break;

                    case NodeType.GraphLiteral:
                        throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("RDF/JSON"));

                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)obj;
                        writer.WriteValue(lit.Value);

                        if (!lit.Language.Equals(String.Empty))
                        {
                            writer.WritePropertyName("lang");
                            writer.WriteValue(lit.Language);
                        }
                        else if (lit.DataType != null)
                        {
                            writer.WritePropertyName("datatype");
                            writer.WriteValue(lit.DataType.ToString());
                        }
                        writer.WritePropertyName("type");
                        writer.WriteValue("literal");
                        break;
                    case NodeType.Uri:
                        writer.WriteValue(obj.ToString());
                        writer.WritePropertyName("type");
                        writer.WriteValue("uri");
                        break;
                    default:
                        throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("RDF/JSON"));
                }
                writer.WriteEndObject();
            }

            //Terminate the Object which represents the Graph
            writer.WriteEndObject();
        }

        /// <summary>
        /// Internal Helper method for raising the Warning event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (this.Warning != null)
            {
                this.Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the RDF being output
        /// </summary>
        public event RdfWriterWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "RDF/JSON (Talis Specification)";
        }
    }
}
