/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System;

namespace org.topbraid.spin.model
{

    /**
     * Represents instances of sp:Command (Queries or Update requests).
     *
     * @author Holger Knublauch
     */
    public interface ICommand : IPrintable, IResource
    {

        /**
         * Gets the comment if any has been stored as rdfs:comment.
         * @return the comment or null
         */
        String getComment();
    }
}