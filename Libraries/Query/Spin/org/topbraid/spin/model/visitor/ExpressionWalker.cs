/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF;

namespace org.topbraid.spin.model.visitor
{

    /**
     * An ExpressionVisitor that recursively visits all expressions under
     * a given root.
     * 
     * @author Holger Knublauch
     */
    public class ExpressionWalker : IExpressionVisitor
    {

        private IExpressionVisitor visitor;


        public ExpressionWalker(IExpressionVisitor visitor)
        {
            this.visitor = visitor;
        }


        public void visit(IAggregation aggregation)
        {
            visitor.visit(aggregation);
            IVariable as_ = aggregation.getAs();
            if (as_ != null)
            {
                visitor.visit(as_);
            }
            INode expr = aggregation.getExpression();
            if (expr != null)
            {
                ExpressionVisitors.visit(expr, this);
            }
        }


        public void visit(IFunctionCall functionCall)
        {
            visitor.visit(functionCall);
            List<IResource> args = functionCall.getArguments();
            foreach (INode arg in args)
            {
                ExpressionVisitors.visit(arg, this);
            }
        }


        public void visit(INode node)
        {
            visitor.visit(node);
        }


        public void visit(IVariable variable)
        {
            visitor.visit(variable);
        }
    }
}