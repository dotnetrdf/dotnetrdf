/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model
{


    /**
     * Shared functions of those Query types that can have solution modifiers.
     * 
     * @author Holger Knublauch
     */
    public interface ISolutionModifierQuery : IQuery
    {

        /**
         * Gets the LIMIT or null.
         * @return the specified limit or null
         */
        long? getLimit();


        /**
         * Gets the OFFSET or null
         * @return the specified offset or null
         */
        long? getOffset();
    }
}