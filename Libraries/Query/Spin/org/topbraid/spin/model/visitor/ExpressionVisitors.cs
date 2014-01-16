/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace org.topbraid.spin.model.visitor
{

    /**
     * Utility functions for ExpressionVisitors.
     * 
     * @author Holger Knublauch
     */
    public class ExpressionVisitors
    {

        public static void visit(INode node, IExpressionVisitor visitor)
        {
            if (node is IVariable)
            {
                visitor.visit((IVariable)node);
            }
            else if (node is IFunctionCall)
            {
                visitor.visit((IFunctionCall)node);
            }
            else if (node is IAggregation)
            {
                visitor.visit((IAggregation)node);
            }
            else if (node != null)
            {
                visitor.visit(node);
            }
        }
    }
}