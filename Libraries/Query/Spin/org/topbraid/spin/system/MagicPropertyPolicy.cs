/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF;


namespace org.topbraid.spin.system
{

    /**
     * A singleton used by the evaluation of magic properties
     * (SPINARQPFunction) to determine whether asserted values
     * and/or dynamically computed values shall be returned in
     * SPARQL queries.  Applications can replace the singleton
     * and override the default behavior, which is to return
     * both the existing triples and the dynamically computed ones.
     * This allows applications to control caches of pre-computed
     * values to avoid costly function calls.
     *
     * @author Holger Knublauch
     */
    public class MagicPropertyPolicy
    {

        private static MagicPropertyPolicy singleton = new MagicPropertyPolicy();

        public static MagicPropertyPolicy get()
        {
            return singleton;
        }

        public static void set(MagicPropertyPolicy value)
        {
            singleton = value;
        }


        public enum Policy
        {
            TRIPLES_ONLY, QUERY_RESULTS_ONLY, BOTH
        };


        /**
         * Checks whether a given magic property call should return asserted
         * triples, dynamically computed query results, or both for a given
         * subject/object combination.  
         * @param functionURI  the URI of the function
         * @param graph  the Graph to query
         * @param matchSubject  the subject Node or null
         * @param matchObject  the object Node or null
         * @return the Policy
         */
        public Policy getPolicy(String functionURI, IGraph graph, INode matchSubject, INode matchObject)
        {
            return Policy.BOTH;
        }
    }
}