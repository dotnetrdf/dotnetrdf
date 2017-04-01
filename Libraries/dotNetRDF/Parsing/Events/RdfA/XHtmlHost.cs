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
            // TODO: Load RDFa default profile and HTML+RDFa profile
        }

        public override IEventGenerator<IRdfAEvent> GetEventGenerator(TextReader reader)
        {
            return new HtmlDomEventGenerator(reader, this);
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

#endif