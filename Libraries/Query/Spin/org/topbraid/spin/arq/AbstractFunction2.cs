/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace org.topbraid.spin.arq
{
    /**
     * An abstract superclass for Functions with 2 arguments.
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractFunction2 : AbstractFunction
    {

        override protected NodeValue exec(INode[] nodes, FunctionEnv env)
        {
            INode arg1 = nodes.Length > 0 ? nodes[0] : null;
            INode arg2 = nodes.Length > 1 ? nodes[1] : null;
            return exec(arg1, arg2, env);
        }


        protected abstract NodeValue exec(INode arg1, INode arg2, FunctionEnv env);
    }
}