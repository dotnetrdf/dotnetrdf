/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.util;
using System.Collections.Generic;
namespace org.topbraid.spin.inference
{

    /**
     * A Comparator of spin:rules to determine the order of execution.
     *
     * @author Holger Knublauch
     */
    public interface ISPINRuleComparer : IComparer<CommandWrapper>
    {
    }
}