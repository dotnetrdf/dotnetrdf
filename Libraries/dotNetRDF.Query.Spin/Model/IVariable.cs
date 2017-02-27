/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{


    /**
     * A variable in a SPIN query.
     * 
     * @author Holger Knublauch
     */
    public interface IVariable : IResource, IPrintable
    {

        /**
         * Gets the name of this variable (without the '?').
         * @return the variable name
         */
        String getName();


        /**
         * Gets all TriplePatterns where this Variable is mentioned.
         * @return the TriplePatterns
         */
        HashSet<ITriplePattern> getTriplePatterns();


        /**
         * Checks if this represents a blank node var.
         * @return true  if a blank node var
         */
        bool isBlankNodeVar();
    }
}