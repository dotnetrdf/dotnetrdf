/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    public class DescribeImpl : QueryImpl, IDescribe
    {

        public DescribeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public List<IResource> getResultNodes()
        {
            List<IResource> results = new List<IResource>();
            foreach (IResource node in getList(SP.PropertyResultNodes))
            {
                IVariable variable = SPINFactory.asVariable(node);
                if (variable != null)
                {
                    results.Add(variable);
                }
                else if (node.isUri())
                {
                    results.Add(node);
                }
            }
            return results;
        }


        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printKeyword("DESCRIBE");
            context.print(" ");
            List<IResource> nodes = getResultNodes();
            if (nodes.Count == 0)
            {
                context.print("*");
            }
            else
            {
                for (IEnumerator<IResource> nit = nodes.GetEnumerator(); nit.MoveNext(); )
                {
                    IResource node = nit.Current;
                    if (node is IVariable)
                    {
                        context.print(node.ToString());
                    }
                    else
                    {
                        printVarOrResource(context, node);
                    }
                    if (nit.MoveNext())
                    {
                        context.print(" ");
                    }
                }
            }
            printStringFrom(context);
            if (getWhereElements().Count != 0)
            {
                context.println();
                printWhere(context);
            }
            printSolutionModifiers(context);
            printValues(context);
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }

    }
}