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

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Interface for classes which can format Nodes into Strings
    /// </summary>
    public interface INodeFormatter
    {
        /// <summary>
        /// Formats a Node as a String
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        String Format(INode n);

        /// <summary>
        /// Formats a Node as a String for a specific segment of a Triple
        /// </summary>
        /// <param name="n">Node</param>
        /// <param name="segment">Segment</param>
        /// <returns></returns>
        String Format(INode n, TripleSegment? segment);
    }
}
