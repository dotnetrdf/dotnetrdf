using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
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


        override public void Print(ISparqlPrinter context)
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


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}