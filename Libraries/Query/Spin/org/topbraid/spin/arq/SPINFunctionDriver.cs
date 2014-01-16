/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.model;

namespace org.topbraid.spin.arq
{

    /**
     * Can be used to define custom function factories such as spinx.
     * 
     * @author Holger Knublauch
     */
    public interface SPINFunctionDriver
    {

        /**
         * If this factory is responsible for the provided function INode
         * then it must create a FunctionFactory which can then be registered.
         * @param function  the SPIN Function's resource
         * @return the FunctionFactory or null if this is not responsible
         */
        SPINFunctionFactory create(Function function);
    }
}
