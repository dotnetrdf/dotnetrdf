using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{

    /// <summary>
    /// Static Class of extension methods for use with Handler classes
    /// </summary>
    public static class HandlerExtensions
    {
        internal static Uri GetBaseUri(this IRdfHandler handler)
        {
            if (handler is GraphHandler)
            {
                return ((GraphHandler)handler).BaseUri;
            }
            else if (handler is IWrappingRdfHandler)
            {
                IRdfHandler temp = ((IWrappingRdfHandler)handler).InnerHandlers.FirstOrDefault(h => h.GetBaseUri() != null);
                if (temp == null)
                {
                    return null;
                }
                else 
                {
                   return temp.GetBaseUri();
                }
            }
            else
            {
                return null;
            }
        }

        public static void Apply(this IRdfHandler handler, IGraph g)
        {
            try
            {
                handler.StartRdf();
                if (g != null)
                {
                    //Handle the Base URI if present
                    if (g.BaseUri != null)
                    {
                        if (!handler.HandleBaseUri(g.BaseUri)) ParserHelper.Stop();
                    }
                    //Handle any namespaces
                    foreach (String prefix in g.NamespaceMap.Prefixes)
                    {
                        if (!handler.HandleNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                    }
                    //Finally handle triples
                    foreach (Triple t in g.Triples)
                    {
                        if (!handler.HandleTriple(t)) ParserHelper.Stop();
                    }
                }

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        public static void Apply(this IRdfHandler handler, IEnumerable<Triple> ts)
        {
            try
            {
                handler.StartRdf();
                foreach (Triple t in ts)
                {
                    if (!handler.HandleTriple(t)) ParserHelper.Stop();
                }
                handler.EndRdf(false);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }
    }
}
