/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class FilterImpl : ElementImpl, IFilter
    {

        public FilterImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getExpression()
        {
            IResource expr = getObject(SP.PropertyExpression);
            if (expr != null)
            {
                return SPINFactory.asExpression(expr);
            }
            else
            {
                return null;
            }
        }


        override public void print(IContextualSparqlPrinter context)
        {
            context.printKeyword("FILTER");
            context.print(" ");
            IResource expression = getExpression();
            if (expression == null)
            {
                context.print("<Exception: Missing expression>");
            }
            else
            {
                printNestedExpressionString(context, expression, true);
            }
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}