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
            context.Events.Enqueue(new ElementEvent(node.Name, this.GetAttributes(node), new PositionInfo(node.Line, node.LinePosition)));

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

            context.Events.Enqueue(new EndElementEvent());
        }

        private IEnumerable<KeyValuePair<String, String>> GetAttributes(HtmlNode node)
        {
            return (from attr in node.Attributes
                    select new KeyValuePair<String, String>(attr.Name, attr.Value));
        }
    }
}
