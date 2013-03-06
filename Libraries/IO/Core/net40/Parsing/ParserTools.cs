using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    public static class ParserTools
    {
        /// <summary>
        /// Resolves a QName/Uri into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="t">QName/Uri to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns></returns>
        public static String ResolveUriOrQName(IToken t, INamespaceMapper nsmap, Uri baseUri)
        {
            if (t.TokenType == Token.QNAME)
            {
                return Tools.ResolveQName(t.Value, nsmap, baseUri);
            }
            else if (t.TokenType == Token.URI)
            {
                String uriBase = (baseUri == null) ? String.Empty : baseUri.AbsoluteUri;
                return Tools.ResolveUri(t.Value, uriBase);
            }
            else
            {
                throw new RdfParseException("Unable to resolve a '" + t.GetType().ToString() + "' Token into a URI", t);
            }
        }
    }
}
