/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Core.Runtime.Registries
{
    /// <summary>
    /// A singleton class managing spin:imports.
    /// </summary>
    internal static class SPINImports
    {
        private static Dictionary<Uri, int> _refCount = new Dictionary<Uri, int>(RDFHelper.uriComparer);
        private static Dictionary<Uri, IGraph> _graphs = new Dictionary<Uri, IGraph>(RDFHelper.uriComparer);

        internal static IGraph Get(Uri graphUri)
        {
            if (_graphs.ContainsKey(graphUri))
            {
                return _graphs[graphUri];
            }
            return null;
        }

        internal static void Register(IGraph g)
        {
            if (g.BaseUri == null)
            {
                throw new InvalidOperationException("The graph's BaseUri cannot be null");
            }
            _refCount[g.BaseUri] = _refCount.ContainsKey(g.BaseUri) ? _refCount[g.BaseUri] + 1 : 1;
            _graphs[g.BaseUri] = g;
        }

        internal static void UnRegister(IGraph g)
        {
            if (g.BaseUri == null)
            {
                throw new InvalidOperationException("The graph's BaseUri cannot be null");
            }
            UnRegister(g.BaseUri);
        }

        internal static void UnRegister(Uri graphUri)
        {
            if (graphUri == null) return;
            if (_refCount.ContainsKey(graphUri))
            {
                _refCount[graphUri]--;
                if (_refCount[graphUri] == 0)
                {
                    _refCount.Remove(graphUri);
                    _graphs.Remove(graphUri);
                }
            }
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

        internal static IGraph Load(Uri uri, IRdfReader rdfReader = null)
        {
            Uri physicalUri = UriHelper.ResolveUri(uri);
            if (rdfReader == null)
            {
                String fileExtension = MimeTypesHelper.GetTrueFileExtension(uri.AbsolutePath);
                if (!String.IsNullOrEmpty(fileExtension))
                {
                    rdfReader = MimeTypesHelper.GetParserByFileExtension(fileExtension);
                }
            }
            // TODO handle the case where the server does not respond with the correct Content-Type
            IGraph importGraph = Get(physicalUri);
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
            Register(importGraph);
            return importGraph;
        }
    }
}