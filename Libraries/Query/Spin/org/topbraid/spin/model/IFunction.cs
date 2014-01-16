/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace org.topbraid.spin.model
{

    /**
     * A SPIN Function module (not: FunctionCall).
     * 
     * @author Holger Knublauch
     */
    public interface IFunction : IModule
    {

        /**
         * Gets the value of the spin:returnType property, if any.
         * @return the return type or null
         */
        INode getReturnType();


        /**
         * Checks if this function is a magic property, marked by having
         * rdf:type spin:MagicProperty.
         * @return true  if this is a magic property
         */
        bool isMagicProperty();


        /**
         * Indicates if spin:private is set to true for this function.
         * @return true  if marked private
         */
        bool isPrivate();
    }
}