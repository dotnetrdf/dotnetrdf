/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System.Collections.Generic;
using System;
namespace org.topbraid.spin.model
{

    /**
     * Instances of spin:Module (or subclasses thereof).
     * 
     * @author Holger Knublauch
     */
    public interface IModule : IResource
    {

        /**
         * Gets a List of all declared Arguments.
         * If ordered, then the local names of the predicates are used.
         * @param ordered  true to get an ordered list back (slower)
         * @return the (possibly empty) List of Arguments
         */
        List<IArgument> getArguments(bool ordered);


        /**
         * Gets a Map of variable names to Arguments.
         * @return a Map of variable names to Arguments
         */
        Dictionary<String, IArgument> getArgumentsMap();


        /**
         * Gets the body (if defined).  The result will be type cast into the
         * most specific subclass of Command if possible.
         * @return the body or null
         */
        ICommand getBody();


        /**
         * Gets the rdfs:comment of this (if any).
         * @return the comment or null
         */
        String getComment();


        /**
         * Checks if this Module has been declared to be abstract using spin:abstract.
         * @return true  if this is abstract
         */
        bool isAbstract();
    }
}