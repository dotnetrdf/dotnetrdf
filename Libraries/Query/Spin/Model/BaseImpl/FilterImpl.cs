/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.visitor;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

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


        override public void print(ISparqlFactory context)
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