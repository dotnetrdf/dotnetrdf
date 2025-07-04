/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model;

internal class BindImpl : ElementImpl, IBind
{

    public BindImpl(INode node, IGraph graph, SpinProcessor spinModel)
        : base(node, graph, spinModel)
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