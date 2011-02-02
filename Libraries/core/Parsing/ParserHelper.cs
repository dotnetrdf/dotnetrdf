using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper class containing useful methods for Parsers
    /// </summary>
    public static class ParserHelper
    {
        /// <summary>
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        public static INode TryResolveUri(TokenisingParserContext context, IToken t)
        {
            return ParserHelper.TryResolveUri(context.Graph, t);
        }

        /// <summary>
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        public static INode TryResolveUri(IGraph g, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    try
                    {
                        //if (Options.UriNormalization)
                        //{
                            return g.CreateUriNode(t.Value);
                        //}
                        //else
                        //{
                        //    return new NonNormalizedUriNode(g, Tools.ResolveQName(t.Value, g.NamespaceMap, g.BaseUri));
                        //}
                    }
                    catch (UriFormatException formatEx)
                    {
                        throw new RdfParseException("Unable to resolve the QName '" + t.Value + "' due to the following error:\n" + formatEx.Message, t, formatEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the QName '" + t.Value + "' due to the following error:\n" + rdfEx.Message, t, rdfEx);
                    }

                case Token.URI:
                    try
                    {
                        String uri = Tools.ResolveUri(t.Value, g.BaseUri.ToSafeString());
                        //if (Options.UriNormalization)
                        //{
                            return g.CreateUriNode(new Uri(uri));
                        //}
                        //else
                        //{
                        //    return new NonNormalizedUriNode(g, uri);
                        //}
                    }
                    catch (UriFormatException formatEx)
                    {
                        throw new RdfParseException("Unable to resolve the URI '" + t.Value + "' due to the following error:\n" + formatEx.Message, t, formatEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the URI '" + t.Value + "' due to the following error:\n" + rdfEx.Message, t, rdfEx);
                    }

                default:
                    throw ParserHelper.Error("Unexpected Token '" + t.GetType().ToString() + "' encountered, expected a URI/QName Token to resolve into a URI", t);
            }
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="t">The Token that is the cause of the Error</param>
        /// <returns></returns>
        public static RdfParseException Error(String msg, IToken t)
        {
            StringBuilder output = new StringBuilder();
            output.Append("[");
            output.Append(t.GetType().Name);
            output.Append(" at Line ");
            output.Append(t.StartLine);
            output.Append(" Column ");
            output.Append(t.StartPosition);
            output.Append(" to Line ");
            output.Append(t.EndLine);
            output.Append(" Column ");
            output.Append(t.EndPosition);
            output.Append("] ");
            output.Append(msg);

            return new RdfParseException(output.ToString(), t);
        }
    }
}
