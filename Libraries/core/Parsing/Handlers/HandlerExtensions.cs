using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    static class HandlerExtensions
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
    }
}
