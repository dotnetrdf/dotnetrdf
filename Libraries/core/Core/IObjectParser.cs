/*

Copyright Robert Vesse 2009-11
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
using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for parsers that generate objects of some sort
    /// </summary>
    /// <typeparam name="T">Generated Object Type</typeparam>
    /// <remarks>
    /// <para>
    /// Primarily used as a marker interface in relation to <see cref="MimeTypesHelper">MimeTypesHelper</see> to provide a mechanism whereby parsers for arbitrary objects can be registered and associated with MIME Types and File Extensions
    /// </para>
    /// </remarks>
    public interface IObjectParser<T>
    {
        /// <summary>
        /// Parses an Object from an Input Stream
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <returns></returns>
        T Parse(StreamReader input);

        /// <summary>
        /// Parses an Object from a Text Stream
        /// </summary>
        /// <param name="input">Text Stream</param>
        /// <returns></returns>
        T Parse(TextReader input);

        /// <summary>
        /// Parses an Object from a File
        /// </summary>
        /// <param name="file">Filename</param>
        /// <returns></returns>
        T ParseFromFile(String file);

        /// <summary>
        /// Parses an Object from a String
        /// </summary>
        /// <param name="data">String</param>
        /// <returns></returns>
        T ParseFromString(String data);

        /// <summary>
        /// Parses an Object from a Parameterized String
        /// </summary>
        /// <param name="cmdString">Parameterized String</param>
        /// <returns></returns>
        T ParseFromString(SparqlParameterizedString cmdString);
    }
}
