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
                    // Handle the Base URI if present
                    if (g.BaseUri != null)
                    {
                        if (!handler.HandleBaseUri(g.BaseUri)) ParserHelper.Stop();
                    }
                    // Handle any namespaces
                    foreach (String prefix in g.NamespaceMap.Prefixes)
                    {
                        if (!handler.HandleNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                    }
                    // Finally handle triples
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
                        // Does nothing     
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
