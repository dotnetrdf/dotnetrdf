/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Parsing;
using System.Threading;

namespace VDS.RDF.Query.Spin.Core.Runtime.Registries
{

    /// <summary>
    /// A singleton class managing spin:imports.
    /// </summary>
    public static class SPINImports
    {

        /**
         * Attempts to load a graph with a given URI.
         * In the default implementation, this uses the Jena
         * OntDocumentManager and default loading mechanisms.
         * Subclasses can override this. 
         * @param uri  the base URI of the graph to load
         * @return the Graph or null to ignore this
         * @throws IOException 
         */
        public static IGraph Load(Uri uri, IRdfReader rdfReader = null)
        {
            Uri physicalUri = UriHelper.ResolveUri(uri);
            if (rdfReader == null) {
                String fileExtension = MimeTypesHelper.GetTrueFileExtension(uri.AbsolutePath);
                if (!String.IsNullOrEmpty(fileExtension))
                {
                    rdfReader = MimeTypesHelper.GetParserByFileExtension(fileExtension);
                }
            }
            // TODO handle the case where the server does not respond with the correct Content-Type
            IGraph importGraph = InMemoryGraphsRegistry.Get(physicalUri);
            if (importGraph != null)
            {
                return importGraph;
            }
            importGraph = new ThreadSafeGraph();
            importGraph.BaseUri = uri;
            if (rdfReader == null)
            {
                importGraph.LoadFromUri(physicalUri);
            }
            else
            {
                importGraph.LoadFromUri(physicalUri, rdfReader);
            }
            InMemoryGraphsRegistry.Register(importGraph);
            return importGraph;
        }

    }
}