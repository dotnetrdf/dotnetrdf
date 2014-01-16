/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF;
using System;
namespace org.topbraid.spin.inference
{

    /**
     * A service that can be used to provide "explanations" of inferred
     * triples.  This is populated by the TopSPIN engine and will keep
     * a Map from Triples to the strings of the query.
     * 
     * @author Holger Knublauch
     */
    public class SPINExplanations
    {

        private Dictionary<Triple, INode> classes = new Dictionary<Triple, INode>();

        private Dictionary<Triple, String> texts = new Dictionary<Triple, String>();


        /**
         * Stores a Triple - query assignment.
         * @param triple  the inferred Triple
         * @param text  the query text to associate with the triple
         * @param cls  the class that was holding the rule
         */
        public void put(Triple triple, String text, INode cls)
        {
            texts[triple] = text;
            classes[triple] = cls;
        }


        /**
         * Gets the explanation text for a given inferred triple.
         * @param triple  the Triple to explain
         * @return the explanation or null if none found for triple
         */
        public String getText(Triple triple)
        {
            if (!texts.ContainsKey(triple)) 
            {
                return null;
            }
            return texts[triple];
        }


        /**
         * Gets the class node that holds the rule that inferred a given inferred triple.
         * @param triple  the Triple to explain
         * @return the class or null if none found for triple
         */
        public INode getClass(Triple triple)
        {
            if (!classes.ContainsKey(triple))
            {
                return null;
            }
            return classes[triple];
        }
    }
}