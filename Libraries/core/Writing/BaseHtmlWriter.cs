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

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Abstract Base Class for HTML Writers which provides basic implementation of the IHtmlWriter interface
    /// </summary>
    public abstract class BaseHtmlWriter 
        : IHtmlWriter
    {
        #region IHtmlWriter Members

        private String _stylesheet = String.Empty;
        private String _uriClass = "uri",
                       _bnodeClass = "bnode",
                       _literalClass = "literal",
                       _datatypeClass = "datatype",
                       _langClass = "langspec",
                       _boxClass = "box";
        private String _uriPrefix = String.Empty;


        /// <summary>
        /// Gets/Sets a path to a Stylesheet which is used to format the Graph output
        /// </summary>
        public string Stylesheet
        {
            get
            {
                return this._stylesheet;
            }
            set
            {
                if (value != null) this._stylesheet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display the URIs of URI Nodes
        /// </summary>
        public string CssClassUri
        {
            get
            {
                return this._uriClass;
            }
            set
            {
                if (value != null) this._uriClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Blank Node IDs
        /// </summary>
        public string CssClassBlankNode
        {
            get
            {
                return this._bnodeClass;
            }
            set
            {
                if (value != null) this._bnodeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literals
        /// </summary>
        public string CssClassLiteral
        {
            get
            {
                return this._literalClass;
            }
            set
            {
                if (value != null) this._literalClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display Literal datatypes
        /// </summary>
        public string CssClassDatatype
        {
            get
            {
                return this._datatypeClass;
            }
            set
            {
                if (value != null) this._datatypeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literal language specifiers
        /// </summary>
        public string CssClassLangSpec
        {
            get
            {
                return this._langClass;
            }
            set
            {
                if (value != null) this._langClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for div tags used to group chunks of markup into a box
        /// </summary>
        public String CssClassBox
        {
            get
            {
                return this._boxClass;
            }
            set
            {
                if (value != null) this._boxClass = value;
            }
        }
        
        /// <summary>
        /// Gets/Sets the Prefix applied to href attributes
        /// </summary>
        public String UriPrefix
        {
            get
            {
                return this._uriPrefix;
            }
            set
            {
                if (value != null) this._uriPrefix = value;
            }
        }

        #endregion
    }
}
