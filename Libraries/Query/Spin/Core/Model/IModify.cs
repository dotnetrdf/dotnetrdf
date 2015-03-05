/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A SPARQL Update operation representing a DELETE/INSERT.
     *
     * @author Holger Knublauch
     */
    public interface IModifyResource : IUpdateResource, ICommandWithWhereResource
    {
    }
}