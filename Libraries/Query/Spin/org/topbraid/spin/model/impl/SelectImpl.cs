/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class SelectImpl : QueryImpl, ISelect
    {

        public SelectImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<IResource> getResultVariables()
        {
            List<IResource> results = new List<IResource>();
            foreach (IResource node in getList(SP.PropertyResultVariables))
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

        override public void printSPINRDF(IContextualSparqlPrinter p)
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
            List<IResource> vars = getResultVariables();
            if (vars.Count == 0)
            {
                p.print("*");
            }
            else
            {
                for (IEnumerator<IResource> vit = vars.GetEnumerator(); vit.MoveNext(); )
                {
                    IResource var = vit.Current;
                    if (var is IVariable)
                    {
                        if (getModel().ContainsTriple(var, SP.PropertyExpression, null))
                        {
                            printProjectExpression(p, (IVariable)var);
                        }
                        else
                        {
                            ((IVariable)var).print(p);
                        }
                    }
                    else if (var is IAggregation)
                    {
                        ((IPrintable)var).print(p);
                    }
                    else
                    {
                        p.print("(");
                        ((IPrintable)var).print(p);
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


        private void printGroupBy(IContextualSparqlPrinter p)
        {
            List<IResource> groupBy = getList(SP.PropertyGroupBy);
            if (groupBy.Count > 0)
            {
                IEnumerator<IResource> it = groupBy.GetEnumerator();
                p.println();
                p.printIndentation(p.getIndentation());
                p.printKeyword("GROUP BY");
                while (it.MoveNext())
                {
                    p.print(" ");
                    IResource node = it.Current;
                    printNestedExpressionString(p, node);
                }
            }
        }


        private void printHaving(IContextualSparqlPrinter p)
        {
            List<IResource> havings = getList(SP.PropertyHaving);
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


        private void printProjectExpression(IContextualSparqlPrinter p, IVariable var)
        {
            p.print("((");
            IResource expr = var.getResource(SP.PropertyExpression);
            IPrintable expression = (IPrintable)SPINFactory.asExpression(expr);
            expression.print(p);
            p.print(") ");
            p.printKeyword("AS");
            p.print(" ");
            p.print(var.ToString());
            p.print(")");
        }

    }
}