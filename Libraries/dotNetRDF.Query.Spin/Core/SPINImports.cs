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
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Spin.Core
{

    /**
     * A singleton managing spin:imports.
     * 
     * Subclasses can be installed that implement different loaders or
     * otherwise change the default behavior.
     * 
     * @author Holger Knublauch
     */
    internal class SPINImports
    {

        private Dictionary<Uri, IGraph> _registeredImports = new Dictionary<Uri, IGraph>(RDFUtil.uriComparer);

        private static SPINImports _instance = new SPINImports();

        /* Not sure this is really needed bu we use it all the same */
        public static SPINImports GetInstance()
        {
            return _instance;
        }

        public static void SetInstance(SPINImports importsManager)
        {
            _instance = importsManager;
        }

        /**
         * Attempts to load a graph with a given URI.
         * In the default implementation, this uses the Jena
         * OntDocumentManager and default loading mechanisms.
         * Subclasses can override this. 
         * @param uri  the base URI of the graph to load
         * @return the Graph or null to ignore this
         * @throws IOException 
         */
        public virtual IGraph getImportedGraph(Uri uri, IRdfReader rdfReader = null)
        {
            if (_registeredImports.ContainsKey(uri))
            {
                return _registeredImports[uri];
            }
            IGraph importGraph = new ThreadSafeGraph();
            importGraph.BaseUri = uri;
            try
            {
                if (rdfReader == null)
                {
                    importGraph.LoadFromUri(uri);
                }
                else
                {
                    importGraph.LoadFromUri(uri, rdfReader);
                }
            }
            catch (Exception)
            {
            }
            RegisterGraph(importGraph);
            return importGraph;
        }

        internal static void RegisterGraph(IGraph g) {
            GetInstance()._registeredImports[g.BaseUri] = g;
        }

    }
}