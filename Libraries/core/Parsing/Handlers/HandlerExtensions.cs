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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Static Class of extension methods for use with Handler classes
    /// </summary>
    public static class HandlerExtensions
    {
        /// <summary>
        /// Gets the Base URI from the RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <returns></returns>
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

        /// <summary>
        /// Applies the triples of a Graph to an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="g">Graph</param>
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

        /// <summary>
        /// Applies the triples to an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="ts">Triples</param>
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

        /// <summary>
        /// Applies the result set to a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="results">Result Set</param>
        public static void Apply(this ISparqlResultsHandler handler, SparqlResultSet results)
        {
            try
            {
                handler.StartResults();

                switch (results.ResultsType)
                {
                    case SparqlResultsType.Boolean:
                        handler.HandleBooleanResult(results.Result);
                        break;
                    case SparqlResultsType.VariableBindings:
                        foreach (String var in results.Variables)
                        {
                            if (!handler.HandleVariable(var)) ParserHelper.Stop();
                        }
                        foreach (SparqlResult r in results)
                        {
                            if (!handler.HandleResult(r)) ParserHelper.Stop();
                        }
                        break;
                    default:
                        //Does nothing     
                        break;
                }

                handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndResults(true);
            }
            catch
            {
                handler.EndResults(false);
                throw;
            }
        }
    }
}
