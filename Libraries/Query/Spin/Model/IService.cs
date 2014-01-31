/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A SERVICE element group.
     *
     * @author Holger Knublauch
     */
    public interface IService : IElementGroup
    {

        /**
         * Gets the URI of the SPARQL end point to invoke.
         * @return the service URI (or null if this is a Variable)
         */
        Uri getServiceURI();


        /**
         * The the variable of the SPARQL end point to invoke.
         * @return the Variable (or null if this is a URI)
         */
        IVariable getServiceVariable();
    }
}