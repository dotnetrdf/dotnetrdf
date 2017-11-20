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
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF;

namespace VDS.RDF.Query.Spin.Core
{


    /**
     * Manages extra prefixes that are always available even if not
     * explicitly declared.  Examples include fn and Jena's afn.
     * 
     * @author Holger Knublauch
     */
    internal static class ExtraPrefixes
    {

        private static NamespaceMapper map = new NamespaceMapper();

        static ExtraPrefixes()
        {
            map.AddNamespace("afn", UriFactory.Create("http://jena.hpl.hp.com/ARQ/function#"));
            map.AddNamespace("fn", UriFactory.Create("http://www.w3.org/2005/xpath-functions#"));
            map.AddNamespace("jfn", UriFactory.Create("java:com.hp.hpl.jena.sparql.function.library."));
            map.AddNamespace("pf", UriFactory.Create("http://jena.hpl.hp.com/ARQ/property#"));
            map.AddNamespace("smf", UriFactory.Create("http://topbraid.org/sparqlmotionfunctions#"));
            map.AddNamespace("tops", UriFactory.Create("http://www.topbraid.org/tops#"));
        }


        /**
         * Programmatically adds a new prefix to be regarded as an "extra"
         * prefix.  These are prefixes that are assumed to be valid even if
         * they haven't been declared in the current ontology.
         * This method has no effect if the prefix was already set before.
         * @param prefix  the prefix to add
         * @param namespace  the namespace to add
         */
        public static void Add(String prefix, String namespaceUri)
        {
            if (!map.HasNamespace(prefix))
            {
                map.AddNamespace(prefix, UriFactory.Create(namespaceUri));
            }
        }


        /**
         * Attempts to add an extra prefix for a given Resource.
         * This does nothing if the prefix does not exist or is empty.
         * @param resource  the resource to get the namespace of
         */
        public static void Add(IResource resource)
        {
            if (!resource.isUri() || resource.getSource().Graph == null) return;
            INamespaceMapper mapper = resource.getSource().Graph.NamespaceMap;
            String prefix;
            if (mapper.ReduceToQName(resource.Uri.ToString(), out prefix))
            {
                prefix = prefix.Split(':')[0];
                map.AddNamespace(prefix, mapper.GetNamespaceUri(prefix));
            }
        }


        /**
         * Gets a Map from prefixes to namespaces.
         * The result should be treated as read-only.
         * @return the extra prefixes
         */
        public static INamespaceMapper getExtraPrefixes()
        {
            return map;
        }
    }
}