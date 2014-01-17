/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using org.topbraid.spin.model;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Core
{

    /**
     * Manages the registered SPARQL aggregations (such as SUM).
     * These are loaded from the sp system ontology.
     * 
     * @author Holger Knublauch
     */
    public class Aggregations
    {

        private static Dictionary<String, INode> name2Type = new Dictionary<String, INode>();

        private static Dictionary<INode, String> type2Name = new Dictionary<INode, String>();

        // Load types from sp system ontology
        static Aggregations()
        {
            IGraph model = SP.GetModel();
            IEnumerator<INode> it = model.GetTriplesWithPredicateObject(RDFS.PropertySubClassOf, SP.ClassAggregation).Select(t => t.Subject).GetEnumerator();
            while (it.MoveNext())
            {
                INode aggType = it.Current;
                Triple labelTriple = model.GetTriplesWithSubjectPredicate(aggType, RDFS.PropertyLabel).FirstOrDefault();
                if (labelTriple != null) {
                    String name = ((ILiteralNode)labelTriple.Object).Value;
                    register(aggType, name);
                }
            }
        }


        /**
         * If registered, returns the display name of a given aggregation type.
         * @param aggType  the aggregation type, e.g. sp:Sum
         * @return the name (e.g., "SUM") or null if not registered
         */
        public static String getName(INode aggType)
        {
            if (type2Name.ContainsKey(aggType))
            {
                return type2Name[aggType];
            }
            return null;
        }


        /**
         * If registered, returns the aggregation INode for a given display name. 
         * @param name  the name (e.g., "SUM")
         * @return the type or null if not registered
         */
        public static INode getType(String name)
        {
            if (name2Type.ContainsKey(name))
            {
                return name2Type[name];
            }
            return null;
        }


        /**
         * Programatically adds a new aggregation type.  This is usually only
         * populated from the sp system ontology, but API users may want to
         * bypass (and extend) this mechanism.
         * @param aggType  the type to register
         * @param name  the display name
         */
        public static void register(INode aggType, String name)
        {
            type2Name[aggType] = name;
            name2Type[name] = aggType;
        }
    }
}