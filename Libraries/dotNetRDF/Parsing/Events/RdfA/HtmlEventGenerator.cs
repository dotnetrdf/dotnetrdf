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

#if UNFINISHED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events.RdfA
{
    /// <summary>
    /// A DOM based event generator for RDFa parsing
    /// </summary>
    public class HtmlDomEventGenerator : IPreProcessingEventGenerator<IRdfAEvent, RdfACoreParserContext>
    {
        private TextReader _reader;
        private XHtmlHost _host;

        public HtmlDomEventGenerator(TextReader reader, XHtmlHost host)
        {
            this._reader = reader;
            this._host = host;
        }

        public void GetAllEvents(RdfACoreParserContext context)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(this._reader);
            this.GenerateEvents(context, doc.DocumentNode);
        }

        private void GenerateEvents(RdfACoreParserContext context, HtmlNode node)
        {
            bool noEnd = false;
            if (node.Name.Equals("#document") || node.Name.Equals("?xml"))
            {
                noEnd = true;
            }
            else
            {
                context.Events.Enqueue(new ElementEvent(node.Name, this.GetAttributes(node), new PositionInfo(node.Line, node.LinePosition)));
            }

            if (node.ChildNodes.Count > 0)
            {
                foreach (HtmlNode child in node.ChildNodes)
                {
                    switch (child.NodeType)
                    {
                        case HtmlNodeType.Element:
                            // Watch out for setting the Base URI and the Version
                            if (node.Name.Equals("base"))
                            {
                                if (node.Attributes["href"] != null)
                                {
                                    try
                                    {
                                        this._host.InitialBaseUri = new Uri(node.Attributes["href"].Value);
                                    }
                                    catch
                                    {
                                        // Ignore errors in setting the Base URI using the <base> element
                                    }
                                }
                            }
                            else if (node.Name.Equals("html"))
                            {
                                this._host.Version = (node.Attributes["version"] != null ? node.Attributes["version"].Value : null);
                            }

                            // Recurse to generate further events
                            this.GenerateEvents(context, child);
                            break;
                        case HtmlNodeType.Text:
                            context.Events.Enqueue(new TextEvent(child.InnerText));
                            break;
                        default:
                            continue;
                    }
                }
            }

            if (!noEnd) context.Events.Enqueue(new EndElementEvent());
        }

        private IEnumerable<KeyValuePair<String, String>> GetAttributes(HtmlNode node)
        {
            return (from attr in node.Attributes
                    select new KeyValuePair<String, String>(attr.Name, attr.Value));
        }
    }
}

#endif