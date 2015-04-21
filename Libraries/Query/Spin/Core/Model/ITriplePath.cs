/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A triple path element.
     *
     * @author Holger Knublauch
     */

    public interface ITriplePathResource : IElementResource
    {
        /**
         * Gets the subject.
         * @return the subject
         */

        IResource getSubject();

        /**
         * Gets the object.
         * @return the object
         */

        IResource getObject();
    }
}