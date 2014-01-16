/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using org.topbraid.spin.model;
using org.topbraid.spin.vocabulary;
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
        public static SPINImports GetInstance() {
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
            if (_registeredImports.ContainsKey(uri)) {
                return _registeredImports[uri];
            }
            IGraph importGraph = new ThreadSafeGraph();
            importGraph.BaseUri = uri;
            importGraph.LoadFromUri(uri);
            _registeredImports[uri] = importGraph;
            return importGraph;
        }


        // Not needed anymore : it is taken care of in the SPINReasoner class
        /**
         * Checks if spin:imports have been declared and adds them to a union model.
         * Will also register any SPIN modules defined in those imports that haven't
         * been loaded before.
         * @param model  the base Model to operate on
         * @return either model or the union of model and its spin:imports
         * @ 
         */
        //public Model getImportsModel(Model model)
        //{
        //    HashSet<Uri> uris = new HashSet<Uri>(RDFUtil.uriComparer);
        //    IEnumerator<Triple> it = model.GetTriplesWithPredicate(SPIN.imports).GetEnumerator();
        //    while (it.MoveNext())
        //    {
        //        Triple s = it.Current;
        //        if (s.Object is IUriNode)
        //        {
        //            uris.Add(((IUriNode)s.Object).Uri);
        //        }
        //    }
        //    if (uris.Count == 0)
        //    {
        //        return model;
        //    }
        //    else
        //    {
        //        Graph baseGraph = model.getGraph();

        //        MultiUnion union = JenaUtil.createMultiUnion();
        //        union.addGraph(baseGraph);
        //        union.setBaseGraph(baseGraph);

        //        bool needsRegistration = false;
        //        foreach (String uri in uris)
        //        {
        //            Graph graph = getImportedGraph(uri);
        //            if (graph != null)
        //            {
        //                union.addGraph(graph);
        //                if (!registeredURIs.contains(uri))
        //                {
        //                    registeredURIs.add(uri);
        //                    needsRegistration = true;
        //                }
        //            }
        //        }

        //        // Ensure that SP, SPIN and SPL are present
        //        ensureImported(union, SP.BASE_URI, SP.GetModel());
        //        ensureImported(union, SPL.BASE_URI, SPL.getModel());
        //        ensureImported(union, SPIN.BASE_URI, SPIN.GetModel());

        //        Model unionModel = ModelFactory.createModelForGraph(union);
        //        if (needsRegistration)
        //        {
        //            SPINModuleRegistry.get().registerAll(unionModel, null);
        //        }
        //        return unionModel;
        //    }
        //}


        //private void ensureImported(MultiUnion union, String baseURI, Model model)
        //{
        //    if (!union.contains(new ITriple(NodeFactory.createURI(baseURI), RDF.type.asNode(), OWL.Ontology.asNode())))
        //    {
        //        union.addGraph(model.getGraph());
        //    }
        //}

    }
}