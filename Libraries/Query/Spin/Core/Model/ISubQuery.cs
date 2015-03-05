/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model
{


    /**
     * A nested sub-query.  Right now, only SELECT queries seem to be allowed
     * but this might change in the future.
     * 
     * @author Holger Knublauch
     */
    public interface ISubQueryResource : IElementResource
    {

        /**
         * Gets the nested query.
         * @return the query
         */
        IQueryResource getQuery();
    }
}