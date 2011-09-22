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

namespace VDS.RDF.Query.FullText.Schema
{
    /// <summary>
    /// Abstract Base Implementation of an Index Schema
    /// </summary>
    public abstract class BaseIndexSchema
        : IFullTextIndexSchema
    {
        /// <summary>
        /// Gets the field in which the full text is indexed
        /// </summary>
        public string IndexField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the hash is stored
        /// </summary>
        public String HashField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node type is stored
        /// </summary>
        public string NodeTypeField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node value is stored
        /// </summary>
        public string NodeValueField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node meta is stored
        /// </summary>
        public string NodeMetaField
        {
            get;
            protected set;
        }
    }
}
