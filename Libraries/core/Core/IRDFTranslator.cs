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

namespace VDS.RDF
{
    /// <summary>
    /// Interface to be implemented by RDF Translators which convert from one concrete syntax into another
    /// </summary>
    public interface IRdfTranslator
    {

        /// <summary>
        /// Method for Translating RDF in one file into RDF in another file
        /// </summary>
        /// <param name="fileIn">File to Read RDF from</param>
        /// <param name="fileOut">File to Writer RDF to</param>
        void Translate(String fileIn, String fileOut);

        /// <summary>
        /// Method for Translating RDF in a file into RDF in a Stream
        /// </summary>
        /// <param name="fileIn">File to Read RDF from</param>
        /// <param name="output">Stream to output RDF to</param>
        void Translate(String fileIn, StreamWriter output);

        /// <summary>
        /// Method for Translating RDF from a Stream into RDF in a File
        /// </summary>
        /// <param name="input">Stream to read RDF from</param>
        /// <param name="fileOut">File to Writer RDF to</param>
        void Translate(StreamReader input, String fileOut);

        /// <summary>
        /// Method for Translating RDF from a Stream into RDF in another Stream
        /// </summary>
        /// <param name="input">Stream to read RDF from</param>
        /// <param name="output">Stream to output RDF to</param>
        void Translate(StreamReader input, StreamWriter output);
    }
}
