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
                            //Watch out for setting the Base URI and the Version
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
                                        //Ignore errors in setting the Base URI using the <base> element
                                    }
                                }
                            }
                            else if (node.Name.Equals("html"))
                            {
                                this._host.Version = (node.Attributes["version"] != null ? node.Attributes["version"].Value : null);
                            }

                            //Recurse to generate further events
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