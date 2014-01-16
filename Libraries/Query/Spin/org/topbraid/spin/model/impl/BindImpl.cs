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

    public class BindImpl : ElementImpl, IBind
    {

        public BindImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getExpression()
        {
            IResource expr = getResource(SP.PropertyExpression);
            if (expr != null)
            {
                return SPINFactory.asExpression(expr);
            }
            else
            {
                return null;
            }
        }


        public IVariable getVariable()
        {
            IResource var = getResource(SP.PropertyVariable);
            if (var != null)
            {
                return (IVariable)var.As(typeof(VariableImpl));
            }
            else
            {
                return null;
            }
        }


        override public void print(IContextualSparqlPrinter context)
        {
            context.printKeyword("BIND");
            context.print(" (");
            IResource expression = getExpression();
            if (expression != null)
            {
                printNestedExpressionString(context, expression);
            }
            else
            {
                context.print("<Exception: Missing expression>");
            }
            context.print(" ");
            context.printKeyword("AS");
            context.print(" ");
            IVariable variable = getVariable();
            if (variable != null)
            {
                context.print(variable.ToString());
            }
            else
            {
                context.print("<Exception: Missing variable>");
            }
            context.print(")");
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}