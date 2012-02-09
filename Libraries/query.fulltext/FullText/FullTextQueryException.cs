/*

Copyright Robert Vesse 2009-12
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

namespace VDS.RDF.Query.FullText
{
    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Query
    /// </summary>
    public class FullTextQueryException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextQueryException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextQueryException(String message)
            : base(message) { }
    }

    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Indexing
    /// </summary>
    public class FullTextIndexException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextIndexException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextIndexException(String message)
            : base(message) { }
    }
}
