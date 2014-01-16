/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
namespace org.topbraid.spin.model.update
{

    /**
     * A SPARQL Update operation representing a DELETE/INSERT.
     *
     * @author Holger Knublauch
     */
    public interface IModify : IUpdate, ICommandWithWhere
    {
    }
}