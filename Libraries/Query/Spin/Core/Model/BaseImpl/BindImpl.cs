/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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