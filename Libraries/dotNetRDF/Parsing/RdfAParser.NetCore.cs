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

using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.IO;
using VDS.RDF.Parsing.Contexts;
using System;
using System.Text;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for reading RDF embedded as RDFa from within HTML web pages
    /// </summary>
    /// <remarks>
    /// <para>
    /// The RDFa parser uses a HTML parser (<a href="http://www.codeplex.com/htmlagilitypack">Html Agility Pack</a>) that is highly tolerant of real-world HTML and so is able to extract RDFa from pages that are not strictly valid HTML/XHTML
    /// </para>
    /// </remarks>
    public class RdfAParser : RdfAParserBase<IHtmlDocument, IElement, AngleSharp.Dom.INode, IAttr>
    {
        /// <summary>
        /// Creates a new RDFa Parser which will auto-detect which RDFa version to use (assumes 1.1 if none explicitly specified)
        /// </summary>
        public RdfAParser() : base()
        {
        }

        /// <summary>
        /// Creates a new RDFa Parser which will use the specified RDFa syntax
        /// </summary>
        /// <param name="syntax">RDFa Syntax Version</param>
        public RdfAParser(RdfASyntax syntax) : base(syntax)
        {
        }

        /// <inheritdoc />
        protected override IHtmlDocument LoadAndParse(TextReader input)
        {
            var parser = new HtmlParser();
            return parser.Parse(input.ReadToEnd());
        }

        /// <inheritdoc />
        protected override bool HasAttribute(IElement element, string attributeName)
        {
            return element.HasAttribute(attributeName);
        }

        /// <inheritdoc/>
        protected override string GetAttribute(IElement element, string attributeName)
        {
            return element.GetAttribute(attributeName);
        }

        /// <inheritdoc/>
        protected override void SetAttribute(IElement element, string attributeName, string value)
        {
            element.SetAttribute(attributeName, value);
        }

        /// <inheritdoc/>
        protected override IElement GetBaseElement(IHtmlDocument document)
        {
            return document.QuerySelector("head > base");
        }

        /// <inheritdoc/>
        protected override bool IsXmlBaseIsPermissible(IHtmlDocument document)
        {
            var docType = document.Doctype.SystemIdentifier;
            if (!string.IsNullOrEmpty(docType))
            {
                if (docType.Equals(XHtmlPlusRdfADoctype))
                {
                    // XHTML+RDFa does not permit xml:base
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        protected override IElement GetHtmlElement(IHtmlDocument document)
        {
            return document.QuerySelector("html");
        }

        /// <inheritdoc/>
        protected override void ProcessDocument(RdfAParserContext<IHtmlDocument> context, RdfAEvaluationContext evalContext)
        {
            foreach (var child in context.Document.Children)
            {
                ProcessElement(context, evalContext, child);
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IAttr> GetAttributes(IElement element)
        {
            return element.Attributes;
        }

        /// <inheritdoc/>
        protected override string GetAttributeName(IAttr attribute)
        {
            return attribute.Name;
        }

        /// <inheritdoc/>
        protected override string GetAttributeValue(IAttr attribute)
        {
            return attribute.Value;
        }

        /// <inheritdoc/>
        protected override string GetElementName(IElement element)
        {
            return element.LocalName;
        }

        /// <inheritdoc/>
        protected override IEnumerable<AngleSharp.Dom.INode> GetChildren(IElement element)
        {
            return element.ChildNodes;
        }

        /// <inheritdoc/>
        protected override bool HasChildren(IElement element)
        {
            return element.HasChildNodes;
        }

        /// <inheritdoc/>
        protected override void GrabText(StringBuilder output, AngleSharp.Dom.INode node)
        {
            output.Append(node.TextContent.Trim());
        }

        /// <inheritdoc/>
        protected override string GetInnerText(AngleSharp.Dom.INode node)
        {
            return node.TextContent.Trim();
        }

        /// <inheritdoc/>
        protected override string GetInnerHtml(IElement element)
        {
            return element.InnerHtml;
        }

        /// <inheritdoc/>
        protected override bool IsTextNode(AngleSharp.Dom.INode node)
        {
            return node.NodeType == AngleSharp.Dom.NodeType.Text;
        }
    }
}
