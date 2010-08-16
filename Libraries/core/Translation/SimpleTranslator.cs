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

namespace VDS.RDF.Translation
{
    /// <summary>
    /// Class which provides a Naive implementation of a Translator as a wrapper to a supplied Reader and Writer
    /// </summary>
    public class SimpleTranslator : IRdfTranslator
    {

        private IRdfReader _reader;
        private IRdfWriter _writer;

        /// <summary>
        /// Creates a new Translator using the supplied Reader and Writer
        /// </summary>
        /// <param name="reader">The Reader to use for Parsing the Input</param>
        /// <param name="writer">The Writer to use for Writing the Output</param>
        public SimpleTranslator(IRdfReader reader, IRdfWriter writer)
        {
            this._reader = reader;
            this._writer = writer;
        }

        /// <summary>
        /// Translates the RDF in one file into RDF in the other file using the Reader and Writer this class was instantiated with
        /// </summary>
        /// <param name="fileIn">Filename of the Input File</param>
        /// <param name="fileOut">Filename of the Output File</param>
        /// <remarks>Open streams for both files then uses the overloaded version which takes two streams to do the actual translation</remarks>
        public void Translate(string fileIn, string fileOut)
        {
            //Open the Input Stream
            StreamReader input = new StreamReader(fileIn);

            //Open the Output Stream
            StreamWriter output = new StreamWriter(fileOut);

            //Use overloaded method to do the work
            this.Translate(input, output);
        }

        /// <summary>
        /// Translates the RDF in one file into RDF to the Stream using the Reader and Writer this class was instantiated with
        /// </summary>
        /// <param name="fileIn">Filename of the Input File</param>
        /// <param name="output">Stream to write the Output RDF to</param>
        /// <remarks>Open streams for the input file then uses the overloaded version which takes two streams to do the actual translation</remarks>
        public void Translate(string fileIn, StreamWriter output)
        {
            //Open the Input Stream
            StreamReader input = new StreamReader(fileIn);

            //Use overloaded method to do the work
            this.Translate(input, output);
        }

        /// <summary>
        /// Translates the RDF in the Stream into RDF in the file using the Reader and Writer this class was instantiated with
        /// </summary>
        /// <param name="input">Stream to read the Input RDF from</param>
        /// <param name="fileOut">Filename of the Output File</param>
        /// <remarks>Open streams for the input file then uses the overloaded version which takes two streams to do the actual translation</remarks>
        public void Translate(StreamReader input, string fileOut)
        {
            //Open the Output Stream
            StreamWriter output = new StreamWriter(fileOut);

            //Use overloaded method to do the work
            this.Translate(input, output);
        }

        /// <summary>
        /// Translates the RDF from one Stream into RDF in the other Stream using the Reader and Writer this class was instantiated with
        /// </summary>
        /// <param name="input">Stream to read the Input RDF from</param>
        /// <param name="output">Stream to write the Output RDF to</param>
        public void Translate(StreamReader input, StreamWriter output)
        {
            //Make a Temporary Graph
            Graph temp = new Graph();

            //Attempt to Parse the Input
            this._reader.Load(temp, input);

            //Attempt to Write the Output
            this._writer.Save(temp, output);
        }
    }
}
