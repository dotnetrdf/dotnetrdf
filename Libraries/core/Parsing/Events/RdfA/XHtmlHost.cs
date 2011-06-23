using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events.RdfA
{
    public class XHtmlHost : BaseHost
    {
        private Uri _baseUri;
        private String _version;

        public override Uri InitialBaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }

        public override string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }

        public override void InitTermMappings(RdfACoreParserContext context)
        {
            //TODO: Load RDFa default profile and HTML+RDFa profile
        }

        public override IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader)
        {
            return new HtmlDomEventGenerator(reader, this);
        }

        public override void ParseLiteralLanguage(RdfACoreParserContext context, IRdfAEvent evt)
        {
            if (evt.HasAttribute("xml:lang"))
            {
                if (RdfSpecsHelper.IsValidLangSpecifier(evt["xml:lang"]))
                {
                    context.Language = evt["xml:lang"];
                }
            }
            else if (evt.HasAttribute("lang"))
            {
                if (RdfSpecsHelper.IsValidLangSpecifier(evt["lang"]))
                {
                    context.Language = evt["lang"];
                }
            }
        }

        public override bool IsRootElement(IRdfAEvent evt)
        {
            if (evt.EventType == Event.Element)
            {
                String name = ((ElementEvent)evt).Name;
                return name.Equals("head") || name.Equals("body");
            }
            else
            {
                return false;
            }
        }
    }

    public class HtmlHost : XHtmlHost
    {

    }
}
