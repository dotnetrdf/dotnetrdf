/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace VDS.RDF.Query.Spin.Model.visitor
{

    /**
     * An "empty" base implementation of ExpressionVisitor.
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractExpressionVisitor : IExpressionVisitor
    {

        public virtual void visit(IAggregation aggregation)
        {
        }


        public virtual void visit(IFunctionCall functionCall)
        {
        }


        public virtual void visit(INode node)
        {
        }


        public virtual void visit(IVariable variable)
        {
        }
    }
}