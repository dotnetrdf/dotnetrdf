/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

namespace VDS.RDF.Query.Spin.Model
{
    /**
     * An RDFList representing a plain list of sub-Elements in a Query.
     * Example:
     *
     * ASK WHERE {
     *     {
     *         ?this is:partOf :ElementList
     * 	   }
     * }
     *
     * @author Holger Knublauch
     */

    public interface IElementListResource : IElementGroupResource
    {
    }
}