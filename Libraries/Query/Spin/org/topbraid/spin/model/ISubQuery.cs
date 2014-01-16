/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace org.topbraid.spin.model
{


    /**
     * A nested sub-query.  Right now, only SELECT queries seem to be allowed
     * but this might change in the future.
     * 
     * @author Holger Knublauch
     */
    public interface ISubQuery : IElement
    {

        /**
         * Gets the nested query.
         * @return the query
         */
        IQuery getQuery();
    }
}