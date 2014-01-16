/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace org.topbraid.spin.arq
{
    /**
     * An abstract superclass for functions with 0 arguments.
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractFunction0 : AbstractFunction
    {

        override protected NodeValue exec(INode[] nodes, FunctionEnv env)
        {
            return exec(env);
        }


        protected abstract NodeValue exec(FunctionEnv env);
    }
}