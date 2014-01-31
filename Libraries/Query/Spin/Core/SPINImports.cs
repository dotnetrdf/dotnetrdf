/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;

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
    public class SPINImports
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
        public virtual IGraph getImportedGraph(Uri uri)
        {
            if (_registeredImports.ContainsKey(uri))
            {
                return _registeredImports[uri];
            }
            IGraph importGraph = new ThreadSafeGraph();
            importGraph.BaseUri = uri;
            try
            {
                importGraph.LoadFromUri(uri);
            }
            catch (Exception any)
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