/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.model.visitor;
namespace org.topbraid.spin.model
{

    /**
     * The abstract base interface for the various Element types.
     * 
     * @author Holger Knublauch
     */
    public interface IElement : IPrintable, IResource
    {

        /**
         * Visits this with a given visitor.
         * @param visitor  the visitor to visit this with
         */
        void visit(IElementVisitor visitor);
    }
}