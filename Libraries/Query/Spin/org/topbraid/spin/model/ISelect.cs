/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System.Collections.Generic;
namespace org.topbraid.spin.model
{


    /**
     * A SELECT query.
     * 
     * @author Holger Knublauch
     */
    public interface ISelect : ISolutionModifierQuery
    {

        /**
         * Gets a list of result variables, or null if we have a star
         * results list.  Note that the "variables" may in fact be
         * wrapped aggregations or expressions.
         * The results can be tested with is against
         * <code>Variable</code>, <code>Aggregation</code> or
         * <code>FunctionCall</code>.  Variables can have an additional
         * <code>sp:expression</code>, representing AS expressions.
         * @return the result "variables"
         */
        List<IResource> getResultVariables();


        /**
         * Checks is this query has the DISTINCT flag set.
         * @return true if distinct
         */
        bool isDistinct();


        /**
         * Checks if this query has the REDUCED flag set.
         * @return true if reduced
         */
        bool isReduced();
    }
}