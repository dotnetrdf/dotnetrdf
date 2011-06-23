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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Events
{

    /// <summary>
    /// Interface for RDFa events
    /// </summary>
    public interface IRdfAEvent : IEvent
    {
        /// <summary>
        /// Gets the attributes of the event i.e. the attributes of the source element
        /// </summary>
        IEnumerable<KeyValuePair<String, String>> Attributes
        {
            get;
        }

        /// <summary>
        /// Gets whether the Event has a given attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        bool HasAttribute(String name);

        /// <summary>
        /// Gets the value of a specific attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        String this[String name]
        {
            get;
        }
    }
}
