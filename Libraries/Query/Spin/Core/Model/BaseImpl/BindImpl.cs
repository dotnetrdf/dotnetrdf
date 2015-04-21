/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class BindImpl : ElementImpl, IBindResource
    {
        public BindImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getExpression()
        {
            IResource expr = GetResource(SP.PropertyExpression);
            if (expr != null)
            {
                return ResourceFactory.asExpression(expr);
            }
            else
            {
                return null;
            }
        }

        public IVariableResource getVariable()
        {
            IResource var = GetResource(SP.PropertyVariable);
            if (var != null)
            {
                return (IVariableResource)var.As(typeof(VariableImpl));
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
            IVariableResource variable = getVariable();
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