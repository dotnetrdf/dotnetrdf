/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;

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
                return _stylesheet;
            }
            set
            {
                if (value != null) _stylesheet = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display the URIs of URI Nodes
        /// </summary>
        public string CssClassUri
        {
            get
            {
                return _uriClass;
            }
            set
            {
                if (value != null) _uriClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Blank Node IDs
        /// </summary>
        public string CssClassBlankNode
        {
            get
            {
                return _bnodeClass;
            }
            set
            {
                if (value != null) _bnodeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literals
        /// </summary>
        public string CssClassLiteral
        {
            get
            {
                return _literalClass;
            }
            set
            {
                if (value != null) _literalClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display Literal datatypes
        /// </summary>
        public string CssClassDatatype
        {
            get
            {
                return _datatypeClass;
            }
            set
            {
                if (value != null) _datatypeClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literal language specifiers
        /// </summary>
        public string CssClassLangSpec
        {
            get
            {
                return _langClass;
            }
            set
            {
                if (value != null) _langClass = value;
            }
        }

        /// <summary>
        /// Gets/Sets the CSS class used for div tags used to group chunks of markup into a box
        /// </summary>
        public String CssClassBox
        {
            get
            {
                return _boxClass;
            }
            set
            {
                if (value != null) _boxClass = value;
            }
        }
        
        /// <summary>
        /// Gets/Sets the Prefix applied to href attributes
        /// </summary>
        public String UriPrefix
        {
            get
            {
                return _uriPrefix;
            }
            set
            {
                if (value != null) _uriPrefix = value;
            }
        }

        #endregion
    }
}
