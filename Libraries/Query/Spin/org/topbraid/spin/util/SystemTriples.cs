/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.util
{

    /**
     * Provides access to the RDF/RDFS/OWL system triples.
     * 
     * TopBraid and this API adds some extra triples (such as declaring
     * superclasses for each system class) that make life easier.
     * In order to expose those assumptions to 3rd party APIs, this is
     * part of the (open source) SPIN API.
     * 
     * @author Holger Knublauch
     */
    public class SystemTriples
    {

        private static IGraph vocabulary;


        private static void ensureSuperClasses(INode metaClass, INode superClass)
        {
            List<INode> toAdd = collectMissingSuperClasses(metaClass, superClass);
            foreach (INode c in toAdd)
            {
                vocabulary.Assert(c, RDFS.subClassOf, superClass);
            }
        }


        private static List<INode> collectMissingSuperClasses(INode metaClass, INode superClass)
        {
            List<INode> toAdd = new List<INode>();
            IEnumerator<Triple> it = vocabulary.GetTriplesWithPredicateObject(RDF.type, metaClass).GetEnumerator();
            while (it.MoveNext())
            {
                INode c = it.Current.Subject;
                if (!c.Equals(superClass))
                {
                    if (!vocabulary.GetTriplesWithSubjectPredicate(c, RDFS.subClassOf).Any())
                    {
                        toAdd.Add(c);
                    }
                }
            }
            return toAdd;
        }


        /**
         * Gets the system ontology (a shared copy).
         * @return the system ontology
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IGraph getVocabularyModel()
        {
            if (vocabulary == null)
            {
                vocabulary = new Graph();
                //org.topbraid.spin.util.JenaUtil.initNamespaces(vocabulary.getGraph());
                vocabulary.NamespaceMap.AddNamespace("xsd", UriFactory.Create(XSD.NS_URI));
                //InputStream rdfs = SP.getResourceAsStream("/etc/rdf-schema.rdf");
                //vocabulary.read(rdfs, RDFS.NS_URI);
                //InputStream owl = SP.getResourceAsStream("/etc/owl.rdf");
                //vocabulary.read(owl, OWL.NS_URI);
                //vocabulary.removeNsPrefix(""); // Otherwise OWL would be default namespace
                ensureSuperClasses(RDFS.Class, RDFS.Resource);
                ensureSuperClasses(OWL.Class, OWL.Thing);

                // Remove owl imports rdfs which only causes trouble
                vocabulary.Retract(vocabulary.GetTriplesWithPredicate(OWL.imports).ToList());

                vocabulary.Assert(OWL.Thing, RDFS.subClassOf, RDFS.Resource);
                vocabulary.Assert(OWL.inverseOf, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(OWL.equivalentClass, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(OWL.equivalentProperty, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(OWL.equivalentProperty, RDFS.range, RDF.Resource);
                vocabulary.Assert(OWL.differentFrom, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(OWL.sameAs, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(OWL.disjointWith, RDF.type, OWL.SymmetricProperty);
                vocabulary.Assert(RDF.XMLLiteral, RDFS.subClassOf, RDFS.Resource);

                foreach (Uri uri in RDFUtil.getDatatypeURIs())
                {
                    INode r = RDFUtil.CreateUriNode(uri);
                    if (vocabulary.GetTriplesWithSubjectPredicate(r, RDF.type).Any())
                    {
                        vocabulary.Assert(r, RDF.type, RDFS.Datatype);
                        vocabulary.Assert(r, RDFS.subClassOf, RDFS.Resource);
                    }
                }

                // Triples were formally in OWL 1, but dropped from OWL 2
                vocabulary.Assert(RDFS.comment, RDF.type, OWL.AnnotationProperty);
                vocabulary.Assert(RDFS.label, RDF.type, OWL.AnnotationProperty);
                vocabulary.Assert(RDFS.isDefinedBy, RDF.type, OWL.AnnotationProperty);
                vocabulary.Assert(RDFS.seeAlso, RDF.type, OWL.AnnotationProperty);

                // Add rdfs:labels for XSD types
                IEnumerator<Triple> datatypes = vocabulary.GetTriplesWithPredicateObject(RDF.type, RDFS.Datatype).GetEnumerator();
                while (datatypes.MoveNext())
                {
                    INode datatype = datatypes.Current.Subject;
                    // TODO replace datatype.ToString() with datatype qName
                    vocabulary.Assert(datatype, RDFS.label, RDFUtil.CreateLiteralNode(datatype.ToString()));
                }
                // TODO make this graph readonly
                //vocabulary = JenaUtil.asReadOnlyModel(vocabulary);
            }
            return vocabulary;
        }
    }
}