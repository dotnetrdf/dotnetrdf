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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Abstract Base Class for Formatters that can compress URIs to QNames
    /// </summary>
    public abstract class QNameFormatter 
        : BaseFormatter, INamespaceFormatter
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
            _qnameMapper = qnameMapper;
            foreach (String prefix in _qnameMapper.Prefixes.ToList())
            {
                if (!IsValidQName(prefix + ":"))
                {
                    _qnameMapper.RemoveNamespace(prefix);
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
            _allowAKeyword = allowAKeyword;
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

            if (_allowAKeyword && segment == TripleSegment.Predicate && u.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfType))
            {
                output.Append('a');
            }
            else if (_qnameMapper.ReduceToQName(u.Uri.AbsoluteUri, out qname))
            {
                if (IsValidQName(qname))
                {
                    output.Append(qname);
                }
                else
                {
                    output.Append('<');
                    output.Append(FormatUri(u.Uri));
                    output.Append('>');
                }
            }
            else
            {
                output.Append('<');
                output.Append(FormatUri(u.Uri));
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
