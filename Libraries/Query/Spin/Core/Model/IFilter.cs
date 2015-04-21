/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A SPARQL FILTER element.
     *
     * @author Holger Knublauch
     */

    public interface IFilterResource : IElementResource
    {
        /**
         * Gets the expression representing the filter condition.
         * The result object will be typecast into the most specific
         * subclass of INode, e.g. FunctionCall or Variable.
         * @return the expression or null
         */

        IResource getExpression();
    }
}