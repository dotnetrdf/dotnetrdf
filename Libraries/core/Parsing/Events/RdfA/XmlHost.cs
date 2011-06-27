using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events.RdfA
{
    public class XmlHost : BaseHost
    {
        public override IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader)
        {
            throw new NotImplementedException();
        }

        public override void InitTermMappings(RdfACoreParserContext context)
        {
            //TODO: Load the RDFa default terms
        }

        public override void ParsePrefixMappings(RdfACoreParserContext context, IRdfAEvent evt)
        {
            foreach (KeyValuePair<String, String> attr in evt.Attributes)
            {
                if (attr.Key.Equals("xmlns"))
                {
                    context.Namespaces.AddNamespace(String.Empty, new Uri(Tools.ResolveUri(attr.Value, context.BaseUri.ToSafeString())));
                }
                else if (attr.Key.StartsWith("xmlns:"))
                {
                    String prefix = attr.Key.Substring(attr.Key.IndexOf(':') + 1);
                    context.Namespaces.AddNamespace(prefix, new Uri(Tools.ResolveUri(attr.Value, context.BaseUri.ToSafeString())));
                }
            }
        }

        public override void ParseExtensions(RdfACoreParserContext context, IRdfAEvent evt)
        {
            if (evt.HasAttribute("xml:base"))
            {
                try
                {
                    context.BaseUri = new Uri(Tools.ResolveUri(evt["xml:base"], context.BaseUri.ToSafeString()));
                }
                catch
                {
                    //If URI resolution fails then cannot change the Base URI
                }
            }
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
        }
    }
}
