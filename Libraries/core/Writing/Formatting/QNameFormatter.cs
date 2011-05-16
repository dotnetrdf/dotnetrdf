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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Abstract Base Class for Formatters that can compress URIs to QNames
    /// </summary>
    public abstract class QNameFormatter : BaseFormatter, INamespaceFormatter
    {
        /// <summary>
        /// QName Map used for compressing URIs to QNames
        /// </summary>
        protected QNameOutputMapper _qnameMapper;
        private bool _allowAKeyword = true;

        /// <summary>
        /// Creates a new QName Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        public QNameFormatter(String formatName, QNameOutputMapper qnameMapper)
            : base(formatName)
        {
            this._qnameMapper = qnameMapper;
            foreach (String prefix in this._qnameMapper.Prefixes.ToList())
            {
                if (!this.IsValidQName(prefix + ":"))
                {
                    this._qnameMapper.RemoveNamespace(prefix);
                }
            }
        }

        /// <summary>
        /// Creates a new QName Formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        /// <param name="qnameMapper">QName Map</param>
        /// <param name="allowAKeyword">Whether the 'a' keyword can be used for the RDF type predicate</param>
        public QNameFormatter(String formatName, QNameOutputMapper qnameMapper, bool allowAKeyword)
            : this(formatName, qnameMapper)
        {
            this._allowAKeyword = allowAKeyword;
        }

        /// <summary>
        /// Determines whether a QName is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected virtual bool IsValidQName(String value)
        {
            return TurtleSpecsHelper.IsValidQName(value);
        }

        /// <summary>
        /// Formats a URI Node using QName compression if possible
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatUriNode(IUriNode u, TripleSegment? segment)
        {
            StringBuilder output = new StringBuilder();
            String qname;

            if (this._allowAKeyword && segment == TripleSegment.Predicate && u.Uri.ToString().Equals(RdfSpecsHelper.RdfType))
            {
                output.Append('a');
            }
            else if (this._qnameMapper.ReduceToQName(u.Uri.ToString(), out qname))
            {
                if (this.IsValidQName(qname))
                {
                    output.Append(qname);
                }
                else
                {
                    output.Append('<');
                    output.Append(this.FormatUri(u.Uri));
                    output.Append('>');
                }
            }
            else
            {
                output.Append('<');
                output.Append(this.FormatUri(u.Uri));
                output.Append('>');
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats a Literal Node using QName compression for the datatype if possible
        /// </summary>
        /// <param name="l">Literal Node</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected abstract override string FormatLiteralNode(ILiteralNode l, TripleSegment? segment);

        /// <summary>
        /// Formats a Namespace as a String
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        public abstract string FormatNamespace(String prefix, Uri namespaceUri);
    }
}
