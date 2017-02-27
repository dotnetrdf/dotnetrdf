using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
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


        override public void Print(ISparqlPrinter context)
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


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}