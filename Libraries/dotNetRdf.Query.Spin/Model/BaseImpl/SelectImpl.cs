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

using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model;

internal class SelectImpl : QueryImpl, ISelect
{

    public SelectImpl(INode node, IGraph graph, SpinProcessor spinModel)
        : base(node, graph, spinModel)
    {
    }


    public List<IResource> getResultVariables()
    {
        var results = new List<IResource>();
        foreach (var node in getList(SP.PropertyResultVariables))
        {
            results.Add(SPINFactory.asExpression(node));
        }
        return results;
    }


    public bool isDistinct()
    {
        return hasProperty(SP.PropertyDistinct, RDFUtil.TRUE);
    }


    public bool isReduced()
    {
        return hasProperty(SP.PropertyReduced, RDFUtil.TRUE);
    }

    override public void printSPINRDF(ISparqlPrinter p)
    {
        printComment(p);
        printPrefixes(p);
        p.printIndentation(p.getIndentation());
        p.printKeyword("SELECT");
        p.print(" ");
        if (isDistinct())
        {
            p.printKeyword("DISTINCT");
            p.print(" ");
        }
        if (isReduced())
        {
            p.printKeyword("REDUCED");
            p.print(" ");
        }
        var vars = getResultVariables();
        if (vars.Count == 0)
        {
            p.print("*");
        }
        else
        {
            for (IEnumerator<IResource> vit = vars.GetEnumerator(); vit.MoveNext(); )
            {
                var var = vit.Current;
                if (var is IVariable)
                {
                    if (getModel().ContainsTriple(var, SP.PropertyExpression, null))
                    {
                        printProjectExpression(p, (IVariable)var);
                    }
                    else
                    {
                        ((IVariable)var).Print(p);
                    }
                }
                else if (var is IAggregation)
                {
                    ((IPrintable)var).Print(p);
                }
                else
                {
                    p.print("(");
                    ((IPrintable)var).Print(p);
                    p.print(")");
                }
                if (vit.MoveNext())
                {
                    p.print(" ");
                }
            }
        }
        printStringFrom(p);
        p.println();
        printWhere(p);
        printGroupBy(p);
        printHaving(p);
        printSolutionModifiers(p);
        printValues(p);
    }


    private void printGroupBy(ISparqlPrinter p)
    {
        var groupBy = getList(SP.PropertyGroupBy);
        if (groupBy.Count > 0)
        {
            IEnumerator<IResource> it = groupBy.GetEnumerator();
            p.println();
            p.printIndentation(p.getIndentation());
            p.printKeyword("GROUP BY");
            while (it.MoveNext())
            {
                p.print(" ");
                var node = it.Current;
                printNestedExpressionString(p, node);
            }
        }
    }


    private void printHaving(ISparqlPrinter p)
    {
        var havings = getList(SP.PropertyHaving);
        if (havings.Count > 0)
        {
            IEnumerator<IResource> it = havings.GetEnumerator();
            p.println();
            p.printIndentation(p.getIndentation());
            p.printKeyword("HAVING");
            while (it.MoveNext())
            {
                p.print(" ");
                printNestedExpressionString(p, it.Current);
            }
        }
    }


    private void printProjectExpression(ISparqlPrinter p, IVariable var)
    {
        p.print("((");
        var expr = var.getResource(SP.PropertyExpression);
        var expression = (IPrintable)SPINFactory.asExpression(expr);
        expression.Print(p);
        p.print(") ");
        p.printKeyword("AS");
        p.print(" ");
        p.print(var.ToString());
        p.print(")");
    }

}