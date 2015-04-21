/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;

namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A template class definition.
     *
     * @author Holger Knublauch
     */

    public interface ITemplateResource : IModuleResource
    {
        /**
         * Gets the declared spin:labelTemplate (if any exists).
         * @return the label template string or null
         */

        String getLabelTemplate();
    }
}