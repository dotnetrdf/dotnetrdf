using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events;
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
        public static INode TryResolveUri(IParserContext context, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    try
                    {
                        return context.Handler.CreateUriNode(new Uri(Tools.ResolveQName(t.Value, context.Namespaces, context.BaseUri)));
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
                        String uri = Tools.ResolveUri(t.Value, context.BaseUri.ToSafeString());
                        return context.Handler.CreateUriNode(new Uri(uri));
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
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        public static INode TryResolveUri(IStoreParserContext context, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    try
                    {
                        return context.Handler.CreateUriNode(new Uri(Tools.ResolveQName(t.Value, context.Namespaces, context.BaseUri)));
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
                        String uri = Tools.ResolveUri(t.Value, context.BaseUri.ToSafeString());
                        return context.Handler.CreateUriNode(new Uri(uri));
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
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        /// <remarks>
        /// It is <strong>not</strong> recommended to use this overload since an <see cref="IRdfHandler">IRdfHandler</see> cannot resolve QNames
        /// </remarks>
        internal static INode TryResolveUri(IRdfHandler handler, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    throw new RdfException("Unable to resolve the QName since an RDF Handler does not have a Namespace Map that can be used to resolve QNames");

                case Token.URI:
                    try
                    {
                        String uri = Tools.ResolveUri(t.Value, String.Empty);
                        return handler.CreateUriNode(new Uri(uri));
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

        internal static INode TryResolveUri(IResultsParserContext context, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.URI:
                    try
                    {
                        String uri = Tools.ResolveUri(t.Value, String.Empty);
                        return context.Handler.CreateUriNode(new Uri(uri));
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
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        /// <remarks>
        /// It is <strong>not</strong> recommended to use this overload since an <see cref="IRdfHandler">IRdfHandler</see> cannot resolve QNames
        /// </remarks>
        internal static INode TryResolveUri(ISparqlResultsHandler handler, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    throw new RdfException("Unable to resolve the QName since a Results Handler does not have a Namespace Map that can be used to resolve QNames");

                case Token.URI:
                    try
                    {
                        String uri = Tools.ResolveUri(t.Value, String.Empty);
                        return handler.CreateUriNode(new Uri(uri));
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
                        return g.CreateUriNode(t.Value);
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
                        return g.CreateUriNode(new Uri(uri));
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

        /// <summary>
        /// Helper function which generates standardised Error Messages
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="evt">Event causing the Error</param>
        /// <returns></returns>
        public static RdfParseException Error(String message, IRdfXmlEvent evt)
        {
            return Error(message, String.Empty, evt);
        }

        /// <summary>
        /// Helper function which generates standardised Error Messages
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="production">The Production where the Error occurred</param>
        /// <param name="evt">Event causing the Error</param>
        /// <returns></returns>
        public static RdfParseException Error(String message, String production, IRdfXmlEvent evt)
        {
            StringBuilder output = new StringBuilder();
            if (evt.Position != null)
            {
                output.Append('[');
                output.Append("Line ");
                output.Append(evt.Position.StartLine);
                output.Append(" Column ");
                output.Append(evt.Position.StartPosition);
                output.Append("] ");
            }
            output.AppendLine(message);
            if (!production.Equals(String.Empty)) output.AppendLine("Occurred in Grammar Production '" + production + "'");
            if (!evt.SourceXml.Equals(String.Empty))
            {
                output.AppendLine("[Source XML]");
                output.AppendLine(evt.SourceXml);
            }

            if (evt.Position != null)
            {
                return new RdfParseException(output.ToString(), evt.Position);
            }
            else
            {
                return new RdfParseException(output.ToString());
            }
        }

        /// <summary>
        /// Throws a <see cref="RdfParsingTerminatedException">RdfParsingTerminatedException</see> which is used to tell the parser that it should stop parsing.
        /// </summary>
        /// <returns></returns>
        public static void Stop()
        {
            throw new RdfParsingTerminatedException();
        }
    }
}
