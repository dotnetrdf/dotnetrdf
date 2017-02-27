/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System;

namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Shared base class for Argument and Attribute.
     * 
     * @author Holger Knublauch
     */
    public interface IAbstractAttribute : IResource
    {

        /**
         * Gets the description (stored in rdfs:comment) of this.
         * @return the description (if any exists)
         */
        String getComment();

        /**
         * Gets the specified sp:argProperty (if any).
         * @return the argProperty
         */
        IResource getPredicate();

        /**
         * Gets the specified spl:valueType (if any).
         * @return the value type
         */
        IResource getValueType();


        /**
         * Checks if this argument has been declared to be optional.
         * For Arguments this means spl:optional = true.
         * For Attributes this means spl:minCardinality = 0
         * @return  true if optional
         */
        bool IsOptional();
    }
}