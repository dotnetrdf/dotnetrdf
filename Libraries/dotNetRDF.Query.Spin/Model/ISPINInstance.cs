/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

using VDS.RDF;
using System.Collections.Generic;

namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A INode that also may have spin constraints or rules attached to it.
     * This is basically a convenience layer that can be used to access those
     * constraints and rules more easily.
     * 
     * @author Holger Knublauch
     */
    public interface ISPINInstance : IResource
    {

        /**
         * Gets the queries and template calls associated with this.
         * @param predicate  the predicate such as <code>spin:rule</code>
         * @return a List of QueryOrTemplateCall instances
         */
        List<QueryOrTemplateCall> getQueriesAndTemplateCalls(INode predicate);
    }
}