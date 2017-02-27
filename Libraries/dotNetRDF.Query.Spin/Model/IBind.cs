/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A BIND assignment element.
     * 
     * @author Holger Knublauch
     */
    public interface IBind : IElement
    {

        /**
         * Gets the SPARQL expression delivering the assigned value.
         * @return the expression
         */
        IResource getExpression();


        /**
         * Gets the variable on the right hand side of the BIND.
         * @return the Variable
         */
        IVariable getVariable();
    }
}