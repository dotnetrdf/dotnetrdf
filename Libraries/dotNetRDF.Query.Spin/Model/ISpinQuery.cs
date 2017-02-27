/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using System;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Base interface of the various SPARQL query types such as
     * Ask, Construct, Describe and Select.
     * 
     * @author Holger Knublauch
     */
    public interface IQuery : ICommandWithWhere
    {

        /**
         * Gets the list of URIs specified in FROM clauses.
         * @return a List of URI Strings
         */
        List<IResource> getFrom();


        /**
         * Gets the list of URIs specified in FROM NAMED clauses.
         * @return a List of URI Strings
         */
        List<IResource> getFromNamed();


        /**
         * Gets the VALUES block at the end of the query if it exists. 
         * @return the Values or null
         */
        IValues getValues();


        /**
         * Gets the elements in the WHERE clause of this query.
         * The Elements will be typecast into the best suitable subclass.
         * @return a List of Elements
         */
        List<IElement> getWhereElements();
    }
}