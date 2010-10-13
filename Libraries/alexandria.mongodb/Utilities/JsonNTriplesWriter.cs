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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for generating NTriples in Json Concrete Syntax
    /// </summary>
    /// <remarks>
    /// <p>
    /// Uses the Json.Net library by <a href="http://james.newtonking.com">James Newton-King</a> to output RDF/Json according to the specification located on the <a href="http://n2.talis.com/wiki/RDF_JSON_Specification">Talis n2 Wiki</a>
    /// </p>
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call the Save() method from multiple threads on different Graphs without issue</threadsafety>
    class JsonNTriplesWriter : IRdfWriter, IPrettyPrintingWriter
    {
        private bool _prettyprint = true;
        private NTriplesFormatter _formatter = new NTriplesFormatter();

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
        /// Saves a Graph in NTriples in JSON syntax to the given File
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="filename">Filename to save to</param>
        public void Save(IGraph g, string filename)
        {
            StreamWriter output = new StreamWriter(filename, false, Encoding.UTF8);
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
                this.RaiseWarning("NTriples in JSON does not contain any Namespace information.  If you read this serialized data back in at a later date you may not be able to reserialize it to Namespace reliant formats (like RDF/XML)");

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
        /// Internal method which generates the NTriples in Json Output for a Graph
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

            //Start the overall Array which contains the set of Triple Objects
            writer.WriteStartArray();

            //Start writing Triples
            foreach (Triple t in g.Triples)
            {
                writer.WriteStartObject();

                //Write Subject
                writer.WritePropertyName("subject");
                writer.WriteValue(this._formatter.Format(t.Subject));

                //Write Predicate
                writer.WritePropertyName("predicate");
                writer.WriteValue(this._formatter.Format(t.Predicate));

                //Write Object
                writer.WritePropertyName("object");
                writer.WriteValue(this._formatter.Format(t.Object));

                writer.WriteEndObject();
            }

            //Terminate the Array which represents the Graph
            writer.WriteEndArray();
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
    }
}
