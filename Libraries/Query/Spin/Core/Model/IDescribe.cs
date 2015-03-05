/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF;
namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A DESCRIBE query.
     * 
     * @author Holger Knublauch
     */
    public interface IDescribeResource : ISolutionModifierQueryResource
    {

        /**
         * Gets the result nodes of this query.  The resulting Resources will be
         * automatically typecast into Variable if they are variables.
         * @return a List of Resources (or Variables)
         */
        List<IResource> getResultNodes();
    }
}