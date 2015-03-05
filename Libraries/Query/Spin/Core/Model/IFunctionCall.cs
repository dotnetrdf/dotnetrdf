/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System.Collections.Generic;
using VDS.RDF;
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Part of a SPARQL expression that calls a Function.
     * 
     * @author Holger Knublauch
     */
    public interface IFunctionCallResource : IPrintable, IModuleCallResource
    {

        /**
         * Gets a list of argument RDFNodes, whereby each INode is already cast
         * into the most specific subclass possible.  In particular, arguments are
         * either instances of Variable, FunctionCall or INode (constant)
         * @return the List of arguments
         */
        List<IResource> getArguments();


        /**
         * Gets a Map from properties (such as sp:arg1, sp:arg2) to their declared
         * argument values.  The map will only contain non-null arguments.
         * @return a Map of arguments
         */
        Dictionary<IResource, IResource> getArgumentsMap();


        /**
         * Gets the URI INode of the Function being called here.
         * The resulting INode will be in the function's defining
         * Model, for example if loaded into the library from a .spin. file.
         * @return the function in its original Model
         */
        IResource getFunction();
    }
}