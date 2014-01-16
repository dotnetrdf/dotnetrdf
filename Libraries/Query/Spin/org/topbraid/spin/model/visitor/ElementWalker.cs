/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System.Collections.Generic;
namespace org.topbraid.spin.model.visitor
{


    /**
     * An object that can be used to recursively walk through an Element
     * and the embedded expressions.
     * 
     * @author Holger Knublauch
     */
    public class ElementWalker : IElementVisitor
    {

        private IElementVisitor elementVisitor;

        private IExpressionVisitor expressionVisitor;


        public ElementWalker(IElementVisitor elementVisitor, IExpressionVisitor expressionVisitor)
        {
            this.elementVisitor = elementVisitor;
            this.expressionVisitor = expressionVisitor;
        }


        public void visit(IBind bind)
        {
            elementVisitor.visit(bind);
            visitExpression(bind.getExpression());
        }


        public void visit(IElementList elementList)
        {
            elementVisitor.visit(elementList);
            visitChildren(elementList);
        }


        public void visit(IExists exists)
        {
            elementVisitor.visit(exists);
            visitChildren(exists);
        }


        public void visit(IFilter filter)
        {
            elementVisitor.visit(filter);
            visitExpression(filter.getExpression());
        }


        public void visit(IMinus minus)
        {
            elementVisitor.visit(minus);
            visitChildren(minus);
        }


        public void visit(INamedGraph namedGraph)
        {
            elementVisitor.visit(namedGraph);
            visitChildren(namedGraph);
        }


        public void visit(INotExists notExists)
        {
            elementVisitor.visit(notExists);
            visitChildren(notExists);
        }


        public void visit(IOptional optional)
        {
            elementVisitor.visit(optional);
            visitChildren(optional);
        }


        public void visit(IService service)
        {
            elementVisitor.visit(service);
            visitChildren(service);
        }


        public void visit(ISubQuery subQuery)
        {
            elementVisitor.visit(subQuery);
        }


        public void visit(ITriplePath triplePath)
        {
            elementVisitor.visit(triplePath);
        }


        public void visit(ITriplePattern triplePattern)
        {
            elementVisitor.visit(triplePattern);
        }


        public void visit(IUnion union)
        {
            elementVisitor.visit(union);
            visitChildren(union);
        }


        public void visit(IValues values)
        {
            elementVisitor.visit(values);
        }


        protected virtual void visitChildren(IElementGroup group)
        {
            List<IElement> childElements = group.getElements();
            foreach (IElement childElement in childElements)
            {
                childElement.visit(this);
            }
        }


        private void visitExpression(INode node)
        {
            if (expressionVisitor != null)
            {
                ExpressionWalker expressionWalker = new ExpressionWalker(expressionVisitor);
                ExpressionVisitors.visit(node, expressionWalker);
            }
        }
    }
}