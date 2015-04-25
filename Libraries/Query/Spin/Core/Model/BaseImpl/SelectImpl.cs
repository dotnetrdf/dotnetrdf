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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public class SelectImpl : QueryImpl, ISelectResource
    {
        public SelectImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public List<IResource> getResultVariables()
        {
            List<IResource> results = new List<IResource>();
            foreach (IResource node in getList(SP.PropertyResultVariables))
            {
                results.Add(ResourceFactory.asExpression(node));
            }
            return results;
        }

        public bool isDistinct()
        {
            return HasProperty(SP.PropertyDistinct, RDFHelper.TRUE);
        }

        public bool isReduced()
        {
            return HasProperty(SP.PropertyReduced, RDFHelper.TRUE);
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
                    if (var is IVariableResource)
                    {
                        if (GetModel().GetTriplesWithSubjectPredicate(var, SP.PropertyExpression).Any())
                        {
                            printProjectExpression(p, (IVariableResource)var);
                        }
                        else
                        {
                            ((IVariableResource)var).Print(p);
                        }
                    }
                    else if (var is IAggregationResource)
                    {
                        ((IPrintable)var).Print(p);
                    }
                    else
                    {
                        p.print("(");
                        ((IPrintable)var).Print(p);
                        p.print(")");
                    }
                    p.print(" ");
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

        private void printHaving(ISparqlPrinter p)
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

        private void printProjectExpression(ISparqlPrinter p, IVariableResource var)
        {
            p.print("((");
            IResource expr = var.GetResource(SP.PropertyExpression);
            IPrintable expression = (IPrintable)ResourceFactory.asExpression(expr);
            expression.Print(p);
            p.print(") ");
            p.printKeyword("AS");
            p.print(" ");
            p.print(var.ToString());
            p.print(")");
        }
    }
}