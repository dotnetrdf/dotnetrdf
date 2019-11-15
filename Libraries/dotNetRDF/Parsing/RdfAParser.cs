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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using VDS.RDF.Parsing.Contexts;

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
    public class RdfAParser : RdfAParserBase<HtmlDocument, HtmlNode, HtmlNode, HtmlAttribute>
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

        /// <inheritdoc/>
        protected override HtmlDocument LoadAndParse(TextReader input)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(input);
            return doc;
        }

        /// <inheritdoc/>
        protected override bool HasAttribute(HtmlNode element, string attributeName)
        {
            return element.Attributes.Contains(attributeName);
        }

        /// <inheritdoc/>
        protected override string GetAttribute(HtmlNode element, string attributeName)
        {
            return element.Attributes[attributeName].Value;
        }

        /// <inheritdoc/>
        protected override void SetAttribute(HtmlNode element, string attributeName, string value)
        {
            element.Attributes.Add(attributeName, value);
        }

        /// <inheritdoc/>
        protected override HtmlNode GetBaseElement(HtmlDocument document)
        {
            return document.DocumentNode.SelectSingleNode("/html/head/base");
        }

        /// <inheritdoc/>
        protected override bool IsXmlBaseIsPermissible(HtmlDocument document)
        {
            HtmlNodeCollection docTypes = document.DocumentNode.SelectNodes("comment()");
            if (docTypes != null)
            {
                foreach (HtmlNode docType in docTypes)
                {
                    if (docType.InnerText.StartsWith("<!DOCTYPE"))
                    {
                        // Extract the Document Type
                        Match dtd = Regex.Match(docType.InnerText, "\"([^\"]+)\">");
                        if (dtd.Success)
                        {
                            if (dtd.Groups[1].Value.Equals(XHtmlPlusRdfADoctype))
                            {
                                // XHTML+RDFa does not permit xml:base
                                return false;
                            }
                            break;
                        }
                    }
                }
            }

            return true;
        }

        /// <inheritdoc/>
        protected override HtmlNode GetHtmlElement(HtmlDocument document)
        {
            return document.DocumentNode.SelectSingleNode("html");
        }

        /// <inheritdoc/>
        protected override void ProcessDocument(RdfAParserContext<HtmlDocument> context, RdfAEvaluationContext evalContext)
        {
            ProcessElement(context, evalContext, context.Document.DocumentNode);
        }

        /// <inheritdoc/>
        protected override IEnumerable<HtmlAttribute> GetAttributes(HtmlNode element)
        {
            return element.Attributes;
        }

        /// <inheritdoc/>
        protected override string GetAttributeName(HtmlAttribute attribute)
        {
            return attribute.Name;
        }

        /// <inheritdoc/>
        protected override string GetAttributeValue(HtmlAttribute attribute)
        {
            return attribute.Value;
        }

        /// <inheritdoc/>
        protected override string GetElementName(HtmlNode element)
        {
            return element.Name;
        }

        /// <inheritdoc/>
        protected override IEnumerable<HtmlNode> GetChildren(HtmlNode element)
        {
            return element.ChildNodes;
        }

        /// <inheritdoc/>
        protected override string GetInnerText(HtmlNode node)
        {
            return node.InnerText;
        }

        /// <inheritdoc/>
        protected override string GetInnerHtml(HtmlNode element)
        {
            return element.InnerHtml;
        }

        /// <inheritdoc/>
        protected override bool HasChildren(HtmlNode element)
        {
            return element.HasChildNodes;
        }

        /// <inheritdoc/>
        protected override bool IsTextNode(HtmlNode node)
        {
            return node.NodeType == HtmlNodeType.Text;
        }

        /// <inheritdoc/>
        protected override void GrabText(StringBuilder output, HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Document:
                case HtmlNodeType.Element:
                    foreach (HtmlNode child in node.ChildNodes)
                    {
                        GrabText(output, child);
                    }
                    break;
                case HtmlNodeType.Text:
                    output.Append(node.InnerText);
                    break;
            }
        }
    }
}
