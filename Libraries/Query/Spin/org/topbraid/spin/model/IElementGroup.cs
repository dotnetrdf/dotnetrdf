/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
namespace org.topbraid.spin.model
{

    /**
     * A collection of zero or more child Elements.
     * Implementations include Optional, Union etc.
     * 
     * @author Holger Knublauch
     */
    public interface IElementGroup : IElement
    {

        /**
         * Gets the List of child Elements.
         * @return a List of children
         */
        List<IElement> getElements();
    }
}